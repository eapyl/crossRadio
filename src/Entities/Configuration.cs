using Newtonsoft.Json;

namespace plr.Entities
{
    internal class Configuration
    {
        [JsonProperty("databaseLink")]
        public string DatabaseLink { get; set; }
    }
}