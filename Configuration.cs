using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace rsRadio
{
    internal class Configuration
    {
        [JsonProperty("telegramKey")]
        public string TelegramBotKey {get ; set; }

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
        public async Task<Configuration> Load() =>
            JsonConvert.DeserializeObject<Configuration>(await _fileContent(_fileName));
    }
}