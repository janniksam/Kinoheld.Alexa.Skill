using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Kinoheld.Application.ResponseMessages;
using Kinoheld.Base;

namespace Kinoheld.Application.Intents
{
    public class AmazonHelpIntent : IIntent
    {
        private readonly IMessages m_messages;

        public AmazonHelpIntent(IMessages messages)
        {
            m_messages = messages;
        }

        public bool IsResponseFor(string intent)
        {
            return intent == Constants.Intents.Help;
        }

        public Task<SkillResponse> GetResponse(SkillRequest request)  
        {
            var message = m_messages.HelpMessage;
            return Task.FromResult(ResponseBuilder.Tell(message));
        }
    }
}