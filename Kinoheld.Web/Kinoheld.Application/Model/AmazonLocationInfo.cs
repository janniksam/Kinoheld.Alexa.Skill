using Newtonsoft.Json;

namespace Kinoheld.Application.Model
{
    public class AmazonLocationInfo
    {
        [JsonProperty("countryCode")]
        public string CountryCode { get; set; }

        [JsonProperty("postalCode")] 
        public string PostalCode { get; set; }
    }
}