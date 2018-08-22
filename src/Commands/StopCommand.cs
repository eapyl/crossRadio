using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using plr.Providers;
using Serilog;

namespace plr.Commands
{
    internal class StopCommand : ICommand
    {
        private readonly ILogger _log;
        private readonly IRadio _radio;

        public string[] Name => new [] { "-s", "--stop" };

        public StopCommand(
            ILogger log,
            IRadio radio)
        {
            _log = log;
            _radio = radio;
        }

        public Task<CommandResult> Execute(IEnumerable<string> parameters)
        {
            _log.Verbose("Received stop command.");
            _radio.Stop();
            return Task.FromResult(CommandResult.Exit);
        }
    }
}