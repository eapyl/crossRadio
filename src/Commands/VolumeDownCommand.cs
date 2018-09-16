using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using plr.Providers;
using Serilog;

namespace plr.Commands
{
    internal class VolumeDownCommand : ICommand
    {
        private readonly ILogger _log;
        private readonly IRadio _radio;
        private readonly IConfigurationProvider _configurationProvider;

        public string[] Name => new [] { "-vd", "--volumeDown" };

        public string Description => "Decrease volume by 10%";

        public VolumeDownCommand(
            ILogger log,
            IConfigurationProvider configurationProvider,
            IRadio radio)
        {
            _log = log;
            _radio = radio;
            _configurationProvider = configurationProvider;
        }

        public async Task<CommandResult> Execute(IEnumerable<string> parameters)
        {
            _log.Verbose("Received volume Down command.");
            await UpdateConfiguration(_radio.VolumeDown());
            return CommandResult.OK;
        }

        private async Task UpdateConfiguration(double volume)
        {
             var configuration = await _configurationProvider.Load();
            configuration.Volume = volume.ToString();
            await _configurationProvider.Upload(configuration);
        }
    }
}