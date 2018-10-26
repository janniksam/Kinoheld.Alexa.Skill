using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Kinoheld.Api.Client.Model;
using Kinoheld.Application.Extensions;
using Kinoheld.Application.ResponseMessages;
using Kinoheld.Base;
using Kinoheld.Base.Utils;
using Kinoheld.Domain.Model.Model;
using Kinoheld.Domain.Services.Abstractions.Database;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kinoheld.Application.Services
{
    public class CinemaDialogWorker : ICinemaDialogWorker
    {
        private readonly IKinoheldDbAccess m_dbAccess;
        private readonly ILogger<CinemaDialogWorker> m_logger;
        private readonly IMessages m_messages;
        private readonly IAmazonService m_amazonService;
        private readonly IKinoheldService m_kinoheldService;

        public CinemaDialogWorker(
            IKinoheldDbAccess kinoheldDbAccess,
            ILogger<CinemaDialogWorker> logger,
            IMessages messages,
            IAmazonService amazonService,
            IKinoheldService kinoheldService)
        {
            m_dbAccess = kinoheldDbAccess;
            m_logger = logger;
            m_messages = messages;
            m_amazonService = amazonService;
            m_kinoheldService = kinoheldService;
        }


        public async Task<GetOverviewInput> GetInput(SkillRequest skillRequest, KinoheldUser user)
        {
            var pendingResponse = await PendingDialog(skillRequest, user).ConfigureAwait(false);
            if (pendingResponse != null)
            {
                return new GetOverviewInput {PendingResponse = pendingResponse};
            }

            m_logger.LogDebug("Preparing input");
            var input = PrepareInput((IntentRequest)skillRequest.Request);
            if (input?.PendingResponse != null)
            {
                return input;
            }

            m_logger.LogDebug("Cinema selection started");
            var cinemaSelectionResponse = await SelectCinema(skillRequest, input, user).ConfigureAwait(false);
            if (cinemaSelectionResponse.PendingResponse != null)
            {
                return cinemaSelectionResponse;
            }

            return cinemaSelectionResponse;
        }

        private async Task<SkillResponse> PendingDialog(SkillRequest skillRequest, KinoheldUser user)
        {
            var request = (IntentRequest) skillRequest.Request;
            if (request.DialogState == DialogState.Started)
            {
                await FillLocationData(skillRequest, user).ConfigureAwait(false);
                return ResponseBuilder.DialogDelegate(request.Intent);
            }

            if (request.DialogState != DialogState.Completed)
            {
                return ResponseBuilder.DialogDelegate();
            }

            return null;
        }

        private async Task FillLocationData(SkillRequest skillRequest, KinoheldUser user)
        {
            var request = (IntentRequest) skillRequest.Request;

            if (!string.IsNullOrEmpty(request.Intent.GetSlot(Slots.City)))
            {
                return;
            }

            if (!string.IsNullOrEmpty(user?.City))
            {
                request.Intent.Slots[Slots.City].Value = user.City;
                request.Intent.Slots[Slots.City].ConfirmationStatus = "CONFIRMED";
                return;
            }

            var amazonLocationInfo = await m_amazonService.GetLocationInfoAsync(skillRequest.Context).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(amazonLocationInfo?.PostalCode))
            {
                request.Intent.Slots[Slots.City].Value = amazonLocationInfo.PostalCode;
                request.Intent.Slots[Slots.City].ConfirmationStatus = "CONFIRMED";
                return;
            }
        }

        private GetOverviewInput PrepareInput(IntentRequest request)
        {
            var city = request.Intent.GetSlot(Slots.City);
            var date = request.Intent.GetSlot(Slots.Vorstellungdatum);
            var dayTime = request.Intent.GetSlot(Slots.Vorstellungzeit);
            if (city == null)
            {
                var response = new GetOverviewInput
                {
                    PendingResponse = ResponseBuilder.Tell("Es wurde keine Stadt gewählt.")
                };
                return response;
            }

            if (date == null)
            {
                var response = new GetOverviewInput
                {
                    PendingResponse = ResponseBuilder.Tell("Es wurde kein Datum gewählt.")
                };
                return response;
            }

            if (!DateTime.TryParse(date, out var vorstellungsdatum))
            {
                var response = new GetOverviewInput
                {
                    PendingResponse = ResponseBuilder.Tell($"Das Datum {date} hat ein ungültiges Format.")
                };
                return response;
            }

            if (!string.IsNullOrEmpty(dayTime))
            {
                if (!dayTime.Equals("AF", StringComparison.OrdinalIgnoreCase) &&
                    !dayTime.Equals("EV", StringComparison.OrdinalIgnoreCase))
                {
                    var response = new GetOverviewInput
                    {
                        PendingResponse = ResponseBuilder.Tell($"Die angegebene Zeit {date} ist ungültig.")
                    };
                    return response;
                }
            }

            var input = new GetOverviewInput
            {
                SelectedDate = vorstellungsdatum,
                SelectedDayTime = dayTime,
                SelectedCity = city
            };
            return input;
        }

        private async Task<GetOverviewInput> SelectCinema(SkillRequest skillRequest, GetOverviewInput input, KinoheldUser user)
        {
            var request = (IntentRequest) skillRequest.Request;

            var cinemaPreferenceForCity =
                user?.CityCinemaAssignments.FirstOrDefault(p => p.City == input.SelectedCity);
            if (cinemaPreferenceForCity != null)
            {
                m_logger.LogDebug("Taking cinema from the preferences");
                input.SelectedCinema = JsonHelper.Deserialize<Cinema>(cinemaPreferenceForCity.Cinema);
                return input;
            }

            var cinemaSelection = request.Intent.GetSlot(Slots.CinemaSelection);
            if (string.IsNullOrEmpty(cinemaSelection) ||
                !int.TryParse(cinemaSelection, out var cinemaSelectionInt))
            {
                m_logger.LogDebug("Getting cinemas near the selected city to choose from");
                var kinos = await m_kinoheldService.GetCinemas(input.SelectedCity).ConfigureAwait(false);
                if (!kinos.Any())
                {
                    input.PendingResponse = ResponseBuilder.Tell(string.Format(m_messages.ErrorNoCinemaInCity, input.SelectedCity));
                    return input;
                }

                if (kinos.Count == 1)
                {
                    input.SelectedCinema = kinos.First();
                    return input;
                }

                input.PendingResponse = GetResponseCinemaSelection(input, kinos);
                return input;
            }

            if (!skillRequest.Session.Attributes.ContainsKey(Constants.Session.Cinemas))
            {
                input.PendingResponse = ResponseBuilder.Tell(string.Format(m_messages.ErrorMissingSessionCinemaObject,
                    Constants.Session.Cinemas));
            }

            m_logger.LogDebug("Getting cinemas from session data");

            var jsonCinemas = JArray.Parse(skillRequest.Session.Attributes[Constants.Session.Cinemas].ToString());
            var cinemas = jsonCinemas.ToObject<IEnumerable<Cinema>>().ToList();
            if (cinemas.Count < cinemaSelectionInt)
            {
                input.PendingResponse = GetResponseCinemaSelection(input, cinemas);
                return input;
            }

            m_logger.LogDebug("Taking the chosen cinema from the list");
            input.SelectedCinema = cinemas[cinemaSelectionInt - 1];

            var savePreference = request.Intent.GetSlot(Slots.SaveCinemaToPreferences);
            if (string.IsNullOrEmpty(savePreference))
            {
                m_logger.LogDebug("Asking user if he wishes to save the cinema selection for upcoming requests");
                input.PendingResponse = GetResponseCinemaSaveToPreference(input, skillRequest.Session);
                return input;
            }

            if (savePreference.Equals("Ja", StringComparison.OrdinalIgnoreCase))
            {
                m_logger.LogDebug("Saving cinema selection for future requests");
                var cinemaData = JsonHelper.Serialize(input.SelectedCinema);
                await m_dbAccess.SaveCityCinemaPreferenceAsync(skillRequest.Session.User.UserId, input.SelectedCity, cinemaData).ConfigureAwait(false);
            }

            return input;
        }

        private static SkillResponse GetResponseCinemaSelection(GetOverviewInput input, IReadOnlyList<Cinema> kinos)
        {
            string outputText;
            if (input.SelectedCity.All(char.IsNumber))
            {
                var plzSplitted = string.Join(" ", Regex.Matches(input.SelectedCity, @"\d{1}"));
                outputText = $"<speak><p>Folgende Kinos wurden in der Nähe von {plzSplitted} gefunden:</p><p>";
            }
            else
            {
                outputText = $"<speak><p>Folgende Kinos wurden in der Nähe von {input?.SelectedCity} gefunden:</p><p>";
            }

            for (var i = 0; i < kinos.Count; i++)
            {
                outputText += $"{i + 1}:. {kinos[i].Name}. ";
            }

            outputText +=
                $"</p><p>Bitte suchen Sie sich ein Kino aus, indem Sie eine Zahl zwischen 1 und {kinos.Count} wählen.</p></speak>";

            var json = JsonConvert.SerializeObject(kinos);
            return ResponseBuilder.DialogElicitSlot(
                new SsmlOutputSpeech { Ssml = outputText },
                Slots.CinemaSelection,
                new Session { Attributes = new Dictionary<string, object> { { Constants.Session.Cinemas, json } } });
        }

        private SkillResponse GetResponseCinemaSaveToPreference(GetOverviewInput input, Session oldSession)
        {
            var outputText =
                $"<speak><p>Sie haben {input?.SelectedCinema?.Name} gewählt.</p><p>Wollen Sie dieses Kino für die Stadt {input?.SelectedCity} zukünftig immer nutzen?</p></speak>";

            return ResponseBuilder.DialogElicitSlot(
                new SsmlOutputSpeech { Ssml = outputText },
                Slots.SaveCinemaToPreferences,
                new Session { Attributes = oldSession.Attributes });
        }
    }
}
