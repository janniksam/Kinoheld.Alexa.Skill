using System.Threading.Tasks;
using Kinoheld.Application.Model;

namespace Kinoheld.Application.Services
{
    public interface IEmailService
    {
        Task SendEmailOverviewAsync(string email, string subject, string htmlBody);

        Task TestConnectionAsync();
    }
}