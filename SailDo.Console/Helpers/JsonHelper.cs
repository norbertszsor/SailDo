using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace SailDo.Console.Helpers
{
    public static class JsonHelper
    {
        private static readonly JsonSerializerSettings Settings = CreateJsonSerializerSettings();

        private static JsonSerializerSettings CreateJsonSerializerSettings()
        {
            var settings = new JsonSerializerSettings();

            settings.Converters.Add(new StringEnumConverter());

            settings.ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };

            settings.Formatting = Formatting.Indented;

            return settings;
        }

        public static T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, Settings)!;
        }

        public static void Serialize<T>(T obj, string fileName)
        {
            File.WriteAllText(fileName, JsonConvert.SerializeObject(obj, Settings));
        }
    }
}
