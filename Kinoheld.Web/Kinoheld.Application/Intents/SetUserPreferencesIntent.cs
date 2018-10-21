using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Kinoheld.Application.Extensions;
using Kinoheld.Application.ResponseMessages;
using Kinoheld.Base;
using Kinoheld.Domain.Services.Abstractions.Database;

namespace Kinoheld.Application.Intents
{
    public class SetUserPreferencesIntent : IIntent
    {
        private readonly IKinoheldDbAccess m_kinoheldDbAccess;
        private readonly IMessages m_messages;

        public SetUserPreferencesIntent(IKinoheldDbAccess kinoheldDbAccess, IMessages messages)
        {
            m_kinoheldDbAccess = kinoheldDbAccess;
            m_messages = messages;
        }

        public bool IsResponseFor(string intent)
        {
            return intent == Constants.Intents.SetUserPreferences;
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

            var city = request.Intent.GetSlot(Slots.City);
            if (string.IsNullOrEmpty(city))
            {
                return ResponseBuilder.Tell(m_messages.ErrorNoValidCity);
            }

            await m_kinoheldDbAccess.SaveCityPreferenceAsync(session.User.UserId, city).ConfigureAwait(false);

            return ResponseBuilder.Tell(string.Format(m_messages.UserPreferenceChanged, city));
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