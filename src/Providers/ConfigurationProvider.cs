using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using plr.Entities;

namespace plr.Providers
{
    internal class ConfigurationProvider : IConfigurationProvider
    {
        private const string _fileName = "settings.json";
        private readonly string _rootPath;
        private readonly Func<string, Task<string>> _fileContent;
        private readonly Func<string, string, Task> _save;

        public ConfigurationProvider(string rootPath, Func<string, Task<string>> fileContent, Func<string, string, Task> saveContent)
        {
            _rootPath = rootPath;
            _fileContent = fileContent;
            _save = saveContent;
        }

        public async Task<Configuration> Load() =>
            JsonConvert.DeserializeObject<Configuration>(await _fileContent(Path.Combine(_rootPath, _fileName)));

        public async Task Upload(Configuration settings) =>
           await _save(Path.Combine(_rootPath, _fileName), JsonConvert.SerializeObject(settings));
    }
}