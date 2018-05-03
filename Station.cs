using Newtonsoft.Json;

namespace rsRadio
{
    internal class Station
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }
    }
}