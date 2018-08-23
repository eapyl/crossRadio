using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using plr.Providers;
using Serilog;

namespace plr.Commands
{
    internal class ListCommand : ICommand
    {
        private readonly ILogger _log;
        private Action<string> _output;
        private readonly IStationProvider _stationProvider;

        public string[] Name => new [] { "-l", "--list" };

        public ListCommand(ILogger log, Action<string> output, IStationProvider stationProvider)
        {
            _log = log;
            _output = output;
            _stationProvider = stationProvider;
        }

        public async Task<CommandResult> Execute(IEnumerable<string> parameters)
        {
            _log.Verbose("Received list command.");
            foreach (var station in await _stationProvider.Search())
                _output(station.ToString());
            return CommandResult.OK;
        }
    }
}