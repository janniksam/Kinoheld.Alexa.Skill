using System;
using Newtonsoft.Json;

namespace Kinoheld.Base.Utils
{
    public class JsonHelper
    {
        public static T Deserialize<T>(string inputJson) where T : class
        {
            try
            {
                var obj = JsonConvert.DeserializeObject<T>(inputJson);
                return obj;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string Serialize<T>(T obj) where T : class
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}