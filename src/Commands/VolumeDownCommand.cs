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

        public string[] Name => new [] { "-vd", "--volumeDown" };

        public VolumeDownCommand(
            ILogger log,
            IRadio radio)
        {
            _log = log;
            _radio = radio;
        }

        public Task<CommandResult> Execute(IEnumerable<string> parameters)
        {
            _log.Verbose("Received volume Down command.");
            if (parameters.Any() && Double.TryParse(parameters.First(), out double deltaDown))
            {
                _log.Verbose($"Decrease volume by {deltaDown}");
                _radio.VolumeDown(deltaDown);
            }
            else
            {
                _radio.VolumeDown();
            }
            return Task.FromResult(CommandResult.OK);
        }
    }
}