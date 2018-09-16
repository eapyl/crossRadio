using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using plr.Commands;
using Serilog;

namespace plr
{
    internal class MainLoop : IMainLoop
    {
        private readonly Func<string> _input;
        private readonly ILogger _log;
        private readonly IRadio _radio;
        private readonly IEnumerable<ICommand> _commands;

        public MainLoop(
            Func<string> input,
            ILogger log,
            IRadio radio,
            IEnumerable<ICommand> commands)
        {
            _input = input;
            _log = log;
            _radio = radio;
            _commands = commands;
        }

        public async Task Run()
        {
            try
            {
                _log.Verbose("Start main loop");
                if (!(await _radio.Init()))
                {
                    _log.Error("Can't init bass library");
                    return;
                }

                _log.Information("Listen command");

                while (true)
                {
                    var parts = _input().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var commandName = parts.Length == 0 ? string.Empty : parts[0];
                    var command = _commands.FirstOrDefault(x => x.Name.Any(name => name == commandName));
                    if (command == null) continue;
                    var result = await command.Execute(parts.Skip(1));
                    if (result == CommandResult.Exit) break;
                }
            }
            catch (Exception ex)
            {
                _log.Error("{ex}", ex);
            }
        }
    }
}