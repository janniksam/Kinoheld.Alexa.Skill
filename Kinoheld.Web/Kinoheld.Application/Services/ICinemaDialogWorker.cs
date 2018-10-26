using System.Threading.Tasks;
using Alexa.NET.Request;
using Kinoheld.Domain.Model.Model;

namespace Kinoheld.Application.Services
{
    public interface ICinemaDialogWorker
    {
        Task<GetOverviewInput> GetInput(SkillRequest skillRequest, KinoheldUser user);
    }
}