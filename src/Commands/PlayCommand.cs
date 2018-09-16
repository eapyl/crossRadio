using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using plr.Providers;
using Serilog;

namespace plr.Commands
{
    internal class PlayCommand : ICommand
    {
        private readonly ILogger _log;
        private readonly Action<string> _output;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly IStationProvider _stationProvider;
        private readonly IRadio _radio;

        public string[] Name => new [] { "-p", "--play" };

        public string Description => "Play selected station using {ID} argument";

        public PlayCommand(
            ILogger log,
            Action<string> output,
            IStationProvider stationProvider,
            IConfigurationProvider configurationProvider,
            IRadio radio)
        {
            _log = log;
            _output = output;
            _configurationProvider = configurationProvider;
            _stationProvider = stationProvider;
            _radio = radio;
        }

        public async Task<CommandResult> Execute(IEnumerable<string> parameters)
        {
            _log.Verbose("Received play command.");
            if (parameters.Any() && Int32.TryParse(parameters.First(), out int id))
            {
                var station = await _stationProvider.Search(id);
                if (station == null)
                {
                    _output("No station with provided ID");
                    _log.Error("There is no station with selected ID.");
                    return CommandResult.Error;
                }
                var link = station.Uri.First().ToString();
                _radio.Play(link);
                await UpdateConfiguration(link);
            }
            else
            {
                var configuration = await _configurationProvider.Load();
                 _radio.Play(configuration.DefaultLink);
            }
            return CommandResult.OK;
        }

        private async Task UpdateConfiguration(string link)
        {
            var configuration = await _configurationProvider.Load();
            configuration.DefaultLink = link;
            await _configurationProvider.Upload(configuration);
        }
    }
}