using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace plr.Commands
{
    internal class HelpCommand : ICommand
    {
        private Action<string> _output;

        public string[] Name => new [] { "-h", "--help" };

        public HelpCommand(Action<string> output)
        {
            _output = output;
        }

        public Task<CommandResult> Execute(IEnumerable<string> parameters)
        {
            _output("List of supported commands:");
            _output("  -h, --help: Show descriptopn of all commands;");
            _output("  -l, --list: Show all stations from database;");
            _output("  -p {ID}, --play {ID}: Play selected station using ID of station;");
            _output("  -pa, --pause: Pause playing;");
            _output("  -st, --start: Start play after pause;");
            _output("  -vu [delta], --volumeUp [delta]: Increase volume by 10% or defined by [delta]%");
            _output("  -vd [delta], --volumeDown [delta]: Decrease volume by 10% or defined by [delta]%;");
            _output("  -s, --stop: Stop plaing and exit;");
            _output("  -db {uri}, --database {uri}: Change link to database (default value is https://raw.githubusercontent.com/eapyl/radio-stations/master/db.json);");
            return Task.FromResult(CommandResult.OK);
        }
    }
}