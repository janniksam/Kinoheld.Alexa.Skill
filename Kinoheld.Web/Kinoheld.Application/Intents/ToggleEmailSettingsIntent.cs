using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Kinoheld.Application.ResponseMessages;
using Kinoheld.Base;
using Kinoheld.Domain.Services.Abstractions.Database;
using Microsoft.Extensions.Logging;

namespace Kinoheld.Application.Intents
{
    public class ToggleEmailSettingsIntent : IIntent
    {
        private readonly IMessages m_messages;
        private readonly ILogger<ToggleEmailSettingsIntent> m_logger;
        private readonly IKinoheldDbAccess m_kinoheldDbAccess;

        public ToggleEmailSettingsIntent(
            IMessages messages, 
            ILogger<ToggleEmailSettingsIntent> logger,
            IKinoheldDbAccess kinoheldDbAccess)
        {
            m_messages = messages;
            m_logger = logger;
            m_kinoheldDbAccess = kinoheldDbAccess;
        }

        public bool IsResponseFor(string intent)
        {
            return intent == Constants.Intents.ToggleEmailSettings;
        }

        public async Task<SkillResponse> GetResponse(SkillRequest skillRequest)
        {
            var request = (IntentRequest)skillRequest.Request;
            var session = skillRequest.Session;

            var dialogResponse = PendingDialog(request);
            if (dialogResponse != null)
            {
                return dialogResponse;
            }

            if (request.Intent.ConfirmationStatus == "DENIED")
            {
                return ResponseBuilder.Tell(m_messages.EmailSettingsAbort);
            }

            var disableEmails = true;
            m_logger.LogDebug("Retrieving user preferences");
            var userPreferences = await m_kinoheldDbAccess.GetPreferenceAsync(session.User.UserId).ConfigureAwait(false);
            if (userPreferences != null)
            {
                disableEmails = !userPreferences.DisableEmails;
            }

            m_logger.LogDebug("Saving user preferences");
            await m_kinoheldDbAccess.SetEmailPreferenceAsync(session.User.UserId, disableEmails).ConfigureAwait(false);

            if (disableEmails)
            {
                return ResponseBuilder.Tell(m_messages.EmailUnsubscribed);
            }

            return ResponseBuilder.Tell(m_messages.EmailSubscribed);
        }

        private SkillResponse PendingDialog(IntentRequest request)
        {
            if (request.DialogState == DialogState.Started)
            {
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