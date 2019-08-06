using Newtonsoft.Json;

namespace Lunch.TelegramBot.Common.Utils
{
    public static class ConfigUtils
    {
        public static T DeserializeObject<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
