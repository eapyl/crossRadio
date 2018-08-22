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
        private readonly IStationProvider _stationProvider;
        private readonly IRadio _radio;

        public string[] Name => new [] { "-p", "--play" };

        public PlayCommand(
            ILogger log,
            Action<string> output,
            IStationProvider stationProvider,
            IRadio radio)
        {
            _log = log;
            _output = output;
            _stationProvider = stationProvider;
            _radio = radio;
        }

        public Task<CommandResult> Execute(IEnumerable<string> parameters)
        {
            _log.Verbose("Received play command.");
            if (parameters.Count() > 0 && Int32.TryParse(parameters.First(), out int id))
            {
                var station = _stationProvider.Search(id);
                if (station == null)
                {
                    _output("No station with provided ID");
                    _log.Error("There is no station with selected ID.");
                    return Task.FromResult(CommandResult.Error);
                }
                _radio.Play(station.Uri.First().ToString());
                return Task.FromResult(CommandResult.OK);
            }
             _output("Can't parse ID");
            _log.Error("Id of station should be number.");
            return Task.FromResult(CommandResult.Error);
        }
    }
}