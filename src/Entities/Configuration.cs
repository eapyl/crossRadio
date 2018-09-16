using Newtonsoft.Json;

namespace plr.Entities
{
    internal class Configuration
    {
        [JsonProperty("databaseLink")]
        public string DatabaseLink { get; set; }
        [JsonProperty("volume")]
        public string Volume { get; set; }
        [JsonProperty("link")]
        public string DefaultLink { get; set; }
    }
}