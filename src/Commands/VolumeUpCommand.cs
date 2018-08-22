using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using plr.Providers;
using Serilog;

namespace plr.Commands
{
    internal class VolumeUpCommand : ICommand
    {
        private readonly ILogger _log;
        private readonly IRadio _radio;

        public string[] Name => new [] { "-vu", "--volumeUp" };

        public VolumeUpCommand(
            ILogger log,
            IRadio radio)
        {
            _log = log;
            _radio = radio;
        }

        public Task<CommandResult> Execute(IEnumerable<string> parameters)
        {
            _log.Verbose("Received volume Up command.");
            if (parameters.Any() && Double.TryParse(parameters.First(), out double deltaUp))
            {
                _log.Verbose($"Increase volume by {deltaUp}");
                _radio.VolumeUp(deltaUp);
            }
            else
            {
                _radio.VolumeUp();
            }
            return Task.FromResult(CommandResult.OK);
        }
    }
}