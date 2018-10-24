using System.Threading.Tasks;
using Kinoheld.Domain.Model.Model;

namespace Kinoheld.Domain.Services.Abstractions.Database
{
    public interface IKinoheldDbAccess
    {
        Task SaveCityCinemaPreferenceAsync(string userId, string city, string cinemaData);

        Task<KinoheldUser> GetPreferenceAsync(string userId);

        Task TestConnectionAsync();

        Task SetEmailPreferenceAsync(string userId, bool disableEmails);

        Task SaveCityPreferenceAsync(string userId, string city);

        Task DeleteUserPreferenceAsync(string userId);
    }
}