using System;
using Newtonsoft.Json;

namespace SchoolClient
{
    public static class JsonHelper
    {
        public static string Serialize<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static T Deserialize<T>(string json) where T : class
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
