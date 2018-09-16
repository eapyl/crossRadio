using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using plr.Providers;
using Serilog;

namespace plr.Commands
{
    internal class StartCommand : ICommand
    {
        private readonly ILogger _log;
        private readonly IRadio _radio;

        public string[] Name => new [] { "-st", "--start" };

        public string Description => "Start play after pause";

        public StartCommand(
            ILogger log,
            IRadio radio)
        {
            _log = log;
            _radio = radio;
        }

        public Task<CommandResult> Execute(IEnumerable<string> parameters)
        {
            _log.Verbose("Received start command.");
            _radio.Start();
            return Task.FromResult(CommandResult.OK);
        }
    }
}