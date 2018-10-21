using System;
using System.Net;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Kinoheld.Application.Model;

namespace Kinoheld.Application.Services
{
    public class AmazonService : IAmazonService
    {
        public async Task<AmazonLocationInfo> GetLocationInfoAsync(Context context)
        {
            try
            {
                var localEndpoint = context?.System?.ApiEndpoint;
                var accesstoken = context?.System?.ApiAccessToken;
                var deviceId = context?.System?.Device?.DeviceID;
                if (string.IsNullOrEmpty(localEndpoint) ||
                    string.IsNullOrEmpty(accesstoken) ||
                    string.IsNullOrEmpty(deviceId))
                {
                    return null;
                }

                var locationUrl = $"{localEndpoint}/v1/devices/{deviceId}/settings/address/countryAndPostalCode";
                
                var request = (HttpWebRequest)WebRequest.Create(new Uri(locationUrl));
                request.Accept = "application/json";
                request.Method = "GET";

                var jsonObj = await NetworkingService.ReadJsonObjectFromUrlGet(locationUrl, null, $"Bearer {accesstoken}").ConfigureAwait(false);
                return jsonObj?.ToObject<AmazonLocationInfo>();
            }
            catch
            {
                return null;
            }
        }

        public async Task<string> GetEmailAsync(Context context)
        {
            try
            {
                var localEndpoint = context?.System?.ApiEndpoint;
                var accesstoken = context?.System?.ApiAccessToken;
                if (string.IsNullOrEmpty(localEndpoint) ||
                    string.IsNullOrEmpty(accesstoken))
                {
                    return null;
                }

                var locationUrl = $"{localEndpoint}/v2/accounts/~current/settings/Profile.email";
                var emailStr= await NetworkingService.ReadStringFromUrlGet(locationUrl, null, $"Bearer {accesstoken}").ConfigureAwait(false);
                return emailStr?.Replace("\"", string.Empty);
            }
            catch
            {
                return null;
            }
        }
    }
}