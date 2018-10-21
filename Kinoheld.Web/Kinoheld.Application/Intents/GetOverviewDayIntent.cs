using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Kinoheld.Api.Client.Model;
using Kinoheld.Application.Abstractions.BackgroundTasks;
using Kinoheld.Application.Extensions;
using Kinoheld.Application.Model;
using Kinoheld.Application.ResponseMessages;
using Kinoheld.Application.Services;
using Kinoheld.Base;
using Kinoheld.Base.Formatter;
using Kinoheld.Base.Utils;
using Kinoheld.Domain.Model.Model;
using Kinoheld.Domain.Services.Abstractions.Database;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kinoheld.Application.Intents
{
    public class GetOverviewDayIntent : IIntent
    {
        private readonly ILogger<GetOverviewDayIntent> m_logger;
        private readonly IMessages m_messages;
        private readonly IKinoheldDbAccess m_dbAccess;
        private readonly IKinoheldService m_kinoheldService;
        private readonly IEmailService m_emailService;
        private readonly IWorkItemQueue m_workItemQueue;
        private readonly IAmazonService m_amazonService;
        private readonly IEmailBodyFormatter<DayOverview> m_dayOverviewEmailFormatter;
        private readonly ISsmlMessageFormatter<DayOverview> m_dayOverviewSsmlFormatter;

        public GetOverviewDayIntent(
            ILogger<GetOverviewDayIntent> logger,
            IMessages messages, 
            IKinoheldService kinoheldService,
            IKinoheldDbAccess dbAccess,
            IAmazonService amazonService,
            IWorkItemQueue workItemQueue,
            IEmailService emailService,
            IEmailBodyFormatter<DayOverview> dayOverviewEmailFormatter,
            ISsmlMessageFormatter<DayOverview> dayOverviewSsmlFormatter)
        {
            m_logger = logger;
            m_messages = messages;
            m_dbAccess = dbAccess;
            m_kinoheldService = kinoheldService;
            m_amazonService = amazonService;
            m_workItemQueue = workItemQueue;
            m_emailService = emailService;
            m_dayOverviewEmailFormatter = dayOverviewEmailFormatter;
            m_dayOverviewSsmlFormatter = dayOverviewSsmlFormatter;
        }
        
        public bool IsResponseFor(string intent)
        {
            return intent == Constants.Intents.GetOverviewDay;
        }

        public async Task<SkillResponse> GetResponse(SkillRequest skillRequest)
        {
            var request = (IntentRequest)skillRequest.Request;
            var session = skillRequest.Session;
            var context = skillRequest.Context;

            m_logger.LogDebug("Getting preferences for user.");
            var userPreference = await m_dbAccess.GetPreferenceAsync(session.User.UserId).ConfigureAwait(false);

            var dialogResponse = PendingDialog(request, userPreference);
            if (dialogResponse != null)
            {
                return dialogResponse;
            }

            m_logger.LogDebug("Dialog was completed");

            var city = request.Intent.GetSlot(Slots.City);
            var vorstellungdatumStr = request.Intent.GetSlot(Slots.Vorstellungdatum);
            var vorstellungszeitStr = request.Intent.GetSlot(Slots.Vorstellungzeit);
            var cinemaSelection = request.Intent.GetSlot(Slots.CinemaSelection);
            var savePreference = request.Intent.GetSlot(Slots.SaveCinemaToPreferences);

            m_logger.LogDebug("Preparing input");
            var validationFailedResponse = PrepareInput(city, vorstellungdatumStr, vorstellungszeitStr, out var vorstellungsdatum);
            if (validationFailedResponse != null)
            {
                return validationFailedResponse;
            }

            m_logger.LogDebug("Cinema selection started");
            var cinemaSelectionResponse = SelectCinema(userPreference, city, cinemaSelection, savePreference, session, out var selectedCinema);
            if (cinemaSelectionResponse != null)
            {
                return cinemaSelectionResponse;
            }

            if (selectedCinema == null)
            {
                m_logger.LogDebug("Getting cinemas near the selected city");
                var kinos = await m_kinoheldService.GetCinemas(city).ConfigureAwait(false);
                if (!kinos.Any())
                {
                    return ResponseBuilder.Tell(string.Format(m_messages.ErrorNoCinemaInCity, city));
                }

                if (kinos.Count == 1)
                {
                    selectedCinema = kinos.First();
                }
                else
                {
                    return GetResponseCinemaSelection(city, kinos);
                }
            }
            else if(savePreference != null && savePreference.Equals("Ja", StringComparison.OrdinalIgnoreCase))
            {
                m_logger.LogDebug("Saving cinema selection for future requests");
                var cinemaData = JsonHelper.Serialize(selectedCinema);
                await m_dbAccess.SaveCityCinemaPreferenceAsync(session.User.UserId, city, cinemaData).ConfigureAwait(false);
            }

            m_logger.LogDebug("Cinema was selected");
            m_logger.LogDebug("Retrieving day overview");

            var dayOverview = await m_kinoheldService.GetDayOverviewForCinema(selectedCinema, vorstellungsdatum, vorstellungszeitStr).ConfigureAwait(false);
            if (dayOverview == null)
            {
                m_logger.LogWarning("Something went wrong during retrieval of the day overview");
                return ResponseBuilder.Tell(m_messages.ErrorRetrievingShows);
            }

            if (userPreference != null && !userPreference.DisableEmails)
            {
                m_logger.LogDebug("Sending email");
                SendEmailAsync(context, dayOverview);
            }

            m_logger.LogDebug("Formatting day overview for Alexa");
            var result = m_dayOverviewSsmlFormatter.Format(dayOverview);
            return ResponseBuilder.Tell(new SsmlOutputSpeech
            {
                Ssml = result
            });
        }


        private void SendEmailAsync(Context context, DayOverview dayOverview)
        {
            m_workItemQueue.Enqueue(async cancellationToken =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var email = await m_amazonService.GetEmailAsync(context).ConfigureAwait(false);
                if (email == null)
                {
                    return;
                }

                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var subject = $"Übersicht {dayOverview.Cinema.Name}";
                    var htmlBody = m_dayOverviewEmailFormatter.Format(dayOverview);
                    if (string.IsNullOrEmpty(htmlBody))
                    {
                        return;
                    }

                    await m_emailService.SendEmailOverviewAsync(email, subject, htmlBody).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            });   
        }

        private static SkillResponse GetResponseCinemaSelection(string city, IReadOnlyList<Cinema> kinos)
        {
            var outputText = $"<speak><p>Folgende Kinos wurden in der Nähe von {city} gefunden:</p><p>";
            for (var i = 0; i < kinos.Count; i++)
            {
                outputText += $"{i + 1}:. {kinos[i].Name}. ";
            }

            outputText +=
                $"</p><p>Bitte suchen Sie sich ein Kino aus, indem Sie eine Zahl zwischen 1 und {kinos.Count} wählen.</p></speak>";

            var json = JsonConvert.SerializeObject(kinos);
            return ResponseBuilder.DialogElicitSlot(
                new SsmlOutputSpeech {Ssml = outputText},
                Slots.CinemaSelection,
                new Session {Attributes = new Dictionary<string, object> {{Constants.Session.Cinemas, json}}});
        }

        private SkillResponse PrepareInput(string city, string vorstellungdatumStr, string vorstellungszeitStr, out DateTime vorstellungsdatum)
        {
            vorstellungsdatum = default(DateTime);

            if (city == null)
            {
                return ResponseBuilder.Tell("Es wurde keine Stadt gewählt.");
            }

            if (vorstellungdatumStr == null)
            {
                return ResponseBuilder.Tell("Es wurde kein Datum gewählt.");
            }

            if (!DateTime.TryParse(vorstellungdatumStr, out vorstellungsdatum))
            {
                return ResponseBuilder.Tell($"Das Datum {vorstellungdatumStr} hat ein ungültiges Format.");
            }

            if (!string.IsNullOrEmpty(vorstellungszeitStr))
            {
                if (!vorstellungszeitStr.Equals("AF", StringComparison.OrdinalIgnoreCase) &&
                    !vorstellungszeitStr.Equals("EV", StringComparison.OrdinalIgnoreCase))
                {
                    return ResponseBuilder.Tell($"Die angegebene Zeit {vorstellungdatumStr} ist ungültig.");
                }
            }

            return null;
        }

        private SkillResponse SelectCinema(KinoheldUser userPreference, string city, string cinemaSelection, string savePreference, Session session, out Cinema selectedCinema)
        {
            selectedCinema = null;

            var cinemaPreferenceForCity = userPreference?.CityCinemaAssignments.FirstOrDefault(p => p.City == city);
            if (cinemaPreferenceForCity != null)
            {
                m_logger.LogDebug("Taking cinema from the preferences");
                selectedCinema = JsonHelper.Deserialize<Cinema>(cinemaPreferenceForCity.Cinema);
                return null;
            }

            if (string.IsNullOrEmpty(cinemaSelection) ||
                !int.TryParse(cinemaSelection, out var cinemaSelectionInt))
            {
                m_logger.LogDebug("No cinema was chosen yet");
                return null;
            }

            if (!session.Attributes.ContainsKey(Constants.Session.Cinemas))
            {
                return ResponseBuilder.Tell(string.Format(m_messages.ErrorMissingSessionCinemaObject, Constants.Session.Cinemas));
            }

            m_logger.LogDebug("Getting cinemas from session data");

            var jsonCinemas = JArray.Parse(session.Attributes[Constants.Session.Cinemas].ToString());
            var cinemas = jsonCinemas.ToObject<IEnumerable<Cinema>>().ToList();
            if (cinemas.Count < cinemaSelectionInt)
            {
                return GetResponseCinemaSelection(city, cinemas);
            }

            m_logger.LogDebug("Taking the chosen cinema from the list");
            selectedCinema = cinemas[cinemaSelectionInt - 1];

            if (string.IsNullOrEmpty(savePreference))
            {
                m_logger.LogDebug("Asking user if he wishes to save the cinema selection for upcoming requests");
                return GetResponseCinemaSaveToPreference(city, selectedCinema, session);
            }

            return null;
        }

        private SkillResponse GetResponseCinemaSaveToPreference(string city, Cinema selectedCinema, Session oldSession)
        {
            var outputText =
                $"<speak><p>Sie haben {selectedCinema.Name} gewählt.</p><p>Wollen Sie dieses Kino für die Stadt {city} zukünftig immer nutzen?</p></speak>";

            return ResponseBuilder.DialogElicitSlot(
                new SsmlOutputSpeech { Ssml = outputText },
                Slots.SaveCinemaToPreferences,
                new Session { Attributes = oldSession.Attributes });
        }

        private SkillResponse PendingDialog(IntentRequest request, KinoheldUser userPreference)
        {
            if (request.DialogState == DialogState.Started)
            {
                if (!string.IsNullOrEmpty(userPreference?.City))
                {
                    if (string.IsNullOrEmpty(request.Intent.Slots[Slots.City].Value))
                    {
                        request.Intent.Slots[Slots.City].Value = userPreference.City;
                    }
                }

                return ResponseBuilder.DialogDelegate(request.Intent);
            }

            if (request.DialogState != DialogState.Completed)
            {
                return ResponseBuilder.DialogDelegate();
            }

            return null;
        }
    }
}