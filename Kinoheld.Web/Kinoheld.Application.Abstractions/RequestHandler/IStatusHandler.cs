using System.Threading.Tasks;

namespace Kinoheld.Application.Abstractions.RequestHandler
{
    public interface IStatusHandler
    {
        Task<string> GetStatusAsync();
    }
}