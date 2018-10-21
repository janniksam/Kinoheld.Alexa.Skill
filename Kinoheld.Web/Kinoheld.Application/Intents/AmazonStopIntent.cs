using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Kinoheld.Application.ResponseMessages;
using Kinoheld.Base;

namespace Kinoheld.Application.Intents
{
    public class AmazonStopIntent : IIntent
    {
        private readonly IMessages m_messages;

        public AmazonStopIntent(IMessages messages)
        {
            m_messages = messages;
        }

        public bool IsResponseFor(string intent)
        {
            return intent == Constants.Intents.Stop;
        }

        public Task<SkillResponse> GetResponse(SkillRequest request)
        {
            return Task.FromResult(ResponseBuilder.Tell(new PlainTextOutputSpeech
            {
                Text = m_messages.StopMessage
            }));
        }
    }
}