using Newtonsoft.Json;

namespace plr
{
    internal class Station
    {
        [JsonIgnore]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("uri")]
        public string[] Uri { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("style")]
        public string[] Style { get; set; }

        [JsonProperty("language")]
        public string[] Language { get; set; }

        public override string ToString() => $"{Id}:{Name}({Country} - {string.Join(',', Style)})";
    }
}