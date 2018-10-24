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
    public class ResetUserPreferencesIntent : IIntent
    {
        private readonly ILogger<ResetUserPreferencesIntent> m_logger;
        private readonly IMessages m_messages;
        private readonly IKinoheldDbAccess m_dbAccess;

        public ResetUserPreferencesIntent(
            ILogger<ResetUserPreferencesIntent> logger, 
            IMessages messages,
            IKinoheldDbAccess dbAccess)
        {
            m_logger = logger;
            m_messages = messages;
            m_dbAccess = dbAccess;
        }

        public bool IsResponseFor(string intent)
        {
            return intent == Constants.Intents.ResetUserPreferences;
        }

        public async Task<SkillResponse> GetResponse(SkillRequest skillRequest)
        {
            var request = (IntentRequest)skillRequest.Request;
            var session = skillRequest.Session;

            m_logger.LogInformation($"{nameof(ResetUserPreferencesIntent)} called.");

            var dialogResponse = PendingDialog(request);
            if (dialogResponse != null)
            {
                return dialogResponse;
            }

            if (request.Intent.ConfirmationStatus == "DENIED")
            {
                return ResponseBuilder.Tell(m_messages.UserPreferencesResetAbort);
            }

            m_logger.LogInformation("Deleting user preferences");

            await m_dbAccess.DeleteUserPreferenceAsync(session.User.UserId).ConfigureAwait(false);

            return ResponseBuilder.Tell(m_messages.UserPreferencesResetted);
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