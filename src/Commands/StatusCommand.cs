using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using plr.Providers;
using Serilog;

namespace plr.Commands
{
    internal class StatusCommand : ICommand
    {
        private readonly ILogger _log;
        private readonly Action<string> _output;
        private readonly IRadio _radio;

        public string[] Name => new [] { string.Empty };

        public StatusCommand(
            ILogger log,
            Action<string> output,
            IRadio radio)
        {
            _log = log;
            _output = output;
            _radio = radio;
        }

        public Task<CommandResult> Execute(IEnumerable<string> parameters)
        {
            _log.Verbose("Received status command.");
            _output(_radio.Status());
            return Task.FromResult(CommandResult.OK);
        }
    }
}