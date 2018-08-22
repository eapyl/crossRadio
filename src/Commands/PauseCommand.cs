using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using plr.Providers;
using Serilog;

namespace plr.Commands
{
    internal class PauseCommand : ICommand
    {
        private readonly ILogger _log;
        private readonly IRadio _radio;

        public string[] Name => new [] { "-pa", "--pause" };

        public PauseCommand(
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
                Log.Verbose($"Increase volume by {deltaUp}");
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