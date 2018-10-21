using System.Threading.Tasks;
using Alexa.NET.Request;
using Kinoheld.Application.Model;

namespace Kinoheld.Application.Services
{
    public interface IAmazonService
    {
        Task<string> GetEmailAsync(Context context);
        Task<AmazonLocationInfo> GetLocationInfoAsync(Context context);
    }
}