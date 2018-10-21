using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Kinoheld.Application.ResponseMessages;
using Microsoft.Extensions.Logging;

namespace Kinoheld.Application.Intents
{
    public class IntentHandler : IIntentHandler
    {
        private readonly IMessages m_messages;
        private readonly IEnumerable<IIntent> m_intents;
        private readonly ILogger<IntentHandler> m_logger;

        public IntentHandler(
            IMessages messages,
            IEnumerable<IIntent> intents,
            ILogger<IntentHandler> logger)
        {            
            m_messages = messages;
            m_intents = intents;
            m_logger = logger;
        }

        public async Task<SkillResponse> GetResponseAsync(SkillRequest request)
        {
            var intentRequest = (IntentRequest) request.Request;

            var intent = m_intents.FirstOrDefault(p => p.IsResponseFor(intentRequest.Intent.Name));
            if (intent != null)
            {
                return await intent.GetResponse(request).ConfigureAwait(false);
            }

            m_logger.LogWarning($"An intendhandler for {intentRequest.Intent.Name} hasn't been found.");
            return ResponseBuilder.Tell(new PlainTextOutputSpeech
            {
                Text = m_messages.ErrorNotFoundIntent
            });
        }
    }
}