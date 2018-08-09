using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace plr
{
    internal class Settings
    {
        [JsonProperty("databaseLink")]
        public string DatabaseLink { get; set; }
    }

    internal class ConfigurationLoader
    {
        private const string _fileName = "settings.json";
        private readonly Func<string, Task<string>> _fileContent;

        public ConfigurationLoader(Func<string, Task<string>> fileContent)
        {
            _fileContent = fileContent;
        }

        public async Task<Settings> Load() =>
            JsonConvert.DeserializeObject<Settings>(await _fileContent(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), _fileName)));
    }
}