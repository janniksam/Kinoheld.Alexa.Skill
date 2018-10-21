using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Response;

namespace Kinoheld.Application.Abstractions.RequestHandler
{
    public interface IAlexaHandler
    {
        Task<SkillResponse> HandleAync(SkillRequest requestJson);
    }
}