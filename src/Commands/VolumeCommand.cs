using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using plr.Providers;
using Serilog;

namespace plr.Commands
{
    internal class VolumeCommand : ICommand
    {
        private readonly Action<string> _output;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly ILogger _log;
        private readonly IRadio _radio;

        public string[] Name => new [] { "-v", "--volume" };

        public string Description => "Set {VALUE}% volume directly";

        public VolumeCommand(
            ILogger log,
            Action<string> output,
            IConfigurationProvider configurationProvider,
            IRadio radio)
        {
            _output = output;
            _configurationProvider = configurationProvider;
            _log = log;
            _radio = radio;
        }

        public async Task<CommandResult> Execute(IEnumerable<string> parameters)
        {
            _log.Verbose("Received volume command.");
            if (parameters.Any() && Int32.TryParse(parameters.First(), out int value))
            {
                _log.Verbose($"Set volume by {value}");
                await UpdateConfiguration(_radio.Volume(value));
            }
            else
            {
                _output("Can't parse volume value");
            }
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