using System.Threading.Tasks;

namespace Kinoheld.Application.Abstractions.RequestHandler
{
    public interface IEmailSubscriptionHandler
    {
        Task Unsubscribe(string alexaId);
    }
}