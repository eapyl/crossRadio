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
        private readonly string _rootPath;
        private readonly Func<string, Task<string>> _fileContent;
        private readonly Func<string, string, Task> _save;

        public ConfigurationLoader(string rootPath, Func<string, Task<string>> fileContent, Func<string, string, Task> saveContent)
        {
            _rootPath = rootPath;
            _fileContent = fileContent;
            _save = saveContent;
        }

        public async Task<Settings> Load() =>
            JsonConvert.DeserializeObject<Settings>(await _fileContent(Path.Combine(_rootPath, _fileName)));

        public async Task Upload(Settings settings) =>
           await _save(Path.Combine(_rootPath, _fileName), JsonConvert.SerializeObject(settings));
    }
}