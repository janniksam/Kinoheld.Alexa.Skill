using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Response;

namespace Kinoheld.Application.Intents
{
    public interface IIntentHandler
    {
        Task<SkillResponse> GetResponseAsync(SkillRequest skillRequest);
    }
}