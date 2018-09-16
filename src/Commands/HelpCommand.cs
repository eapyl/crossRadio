using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace plr.Commands
{
    internal class HelpCommand : ICommand
    {
        private Action<string> _output;
        private readonly IEnumerable<ICommand> _commands;

        public string[] Name => new [] { "-h", "--help" };

        public string Description => "Show help information";

        public HelpCommand(Action<string> output, IEnumerable<ICommand> commands)
        {
            _output = output;
            _commands = commands;
        }

        public Task<CommandResult> Execute(IEnumerable<string> parameters)
        {
             _output("List of supported commands:");
            foreach (var command in _commands)
                _output($"{string.Join(',', command.Name)}: {command.Description}");
            return Task.FromResult(CommandResult.OK);
        }
    }
}