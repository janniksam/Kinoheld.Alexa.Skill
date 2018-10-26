using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Kinoheld.Application.Abstractions.BackgroundTasks;
using Kinoheld.Application.Model;
using Kinoheld.Application.ResponseMessages;
using Kinoheld.Application.Services;
using Kinoheld.Base;
using Kinoheld.Base.Formatter;
using Kinoheld.Domain.Services.Abstractions.Database;
using Microsoft.Extensions.Logging;

namespace Kinoheld.Application.Intents
{
    public class GetOverviewDayIntent : IIntent
    {
        private readonly ILogger<GetOverviewDayIntent> m_logger;
        private readonly IMessages m_messages;
        private readonly IKinoheldDbAccess m_dbAccess;
        private readonly IKinoheldService m_kinoheldService;
        private readonly IEmailService m_emailService;
        private readonly ICinemaDialogWorker m_cinemaDialogWorker;
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
            ICinemaDialogWorker cinemaDialogWorker,
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
            m_cinemaDialogWorker = cinemaDialogWorker;
            m_dayOverviewEmailFormatter = dayOverviewEmailFormatter;
            m_dayOverviewSsmlFormatter = dayOverviewSsmlFormatter;
        }
        
        public bool IsResponseFor(string intent)
        {
            return intent == Constants.Intents.GetOverviewDay;
        }

        public async Task<SkillResponse> GetResponse(SkillRequest skillRequest)
        {
            var session = skillRequest.Session;
            var context = skillRequest.Context;

            m_logger.LogDebug("Getting preferences for user.");
            var user = await m_dbAccess.GetPreferenceAsync(session.User.UserId).ConfigureAwait(false);

            var input = await m_cinemaDialogWorker.GetInput(skillRequest, user).ConfigureAwait(false);
            if (input.PendingResponse != null)
            {
                return input.PendingResponse;
            }

            m_logger.LogDebug("Retrieving day overview");
            var dayOverview = await m_kinoheldService.GetDayOverviewForCinema(input.SelectedCinema, input.SelectedDate, input.SelectedDayTime).ConfigureAwait(false);
            if (dayOverview == null)
            {
                m_logger.LogWarning("Something went wrong during retrieval of the day overview");
                return ResponseBuilder.Tell(m_messages.ErrorRetrievingShows);
            }

            if (user != null && !user.DisableEmails)
            {
                m_logger.LogDebug("Sending email");
                dayOverview.AlexaId = session.User.UserId;
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
    }
}