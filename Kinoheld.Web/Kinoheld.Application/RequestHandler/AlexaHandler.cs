using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Kinoheld.Application.Abstractions.RequestHandler;
using Kinoheld.Application.Intents;
using Kinoheld.Application.ResponseMessages;
using Microsoft.Extensions.Logging;

namespace Kinoheld.Application.RequestHandler
{
    public class AlexaHandler : IAlexaHandler
    {
        private readonly IIntentHandler m_intentHandler;
        private readonly IMessages m_messages;
        private readonly ILogger<AlexaHandler> m_logger;

        public AlexaHandler(
            IIntentHandler intentHandler, 
            IMessages messages,
            ILogger<AlexaHandler> logger)
        {
            m_messages = messages;
            m_logger = logger;
            m_intentHandler = intentHandler;
        }

        public Task<SkillResponse> HandleAync(SkillRequest request)
        {
            var response = GetResponse(request);
            return response;
        }

        private async Task<SkillResponse> GetResponse(SkillRequest request)
        {
            if (request == null)
            {
                m_logger.LogWarning("The request is malformed.");
                return ResponseBuilder.Tell(m_messages.ErrorNotFound);
            }

            if (request.GetRequestType() == typeof(LaunchRequest))
            {
                var launchResponse = ResponseBuilder.Tell(m_messages.Launch);
                launchResponse.Response.ShouldEndSession = false;
                return launchResponse;
            }

            if (request.GetRequestType() != typeof(IntentRequest))
            {
                m_logger.LogWarning($"The request-type {request.GetRequestType()} isn't supported.");
                return ResponseBuilder.Tell(string.Format(m_messages.ErrorRequestTypeNotSupported, request.GetRequestType()));
            }

            var response = await m_intentHandler.GetResponseAsync(request).ConfigureAwait(false);
            return response;
        }
    }
}