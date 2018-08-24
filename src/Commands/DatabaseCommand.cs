using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using plr.Providers;
using Serilog;

namespace plr.Commands
{
    internal class DatabaseCommand : ICommand
    {
        private readonly ILogger _log;
        private readonly Action<string> _output;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly IStationProvider _stationProvider;

        public string[] Name => new [] { "-d", "--database" };

        public DatabaseCommand(
            ILogger log,
            Action<string> output,
            IConfigurationProvider configurationProvider,
            IStationProvider stationProvider)
        {
            _log = log;
            _output = output;
            _configurationProvider = configurationProvider;
            _stationProvider = stationProvider;
        }

        public async Task<CommandResult> Execute(IEnumerable<string> parameters)
        {
            _log.Verbose("Received database command.");
            if (parameters.Any() && Uri.TryCreate(parameters.First(), UriKind.Absolute, out Uri url))
            {
                var configuration = await _configurationProvider.Load();
                configuration.DatabaseLink = url.ToString();
                await _configurationProvider.Upload(configuration);
                _stationProvider.Reset();
                return CommandResult.OK;
            }
            _output("Can't parse provided url");
            _log.Error("Url to database is incorrect.");
            return CommandResult.Error;
        }
    }
}