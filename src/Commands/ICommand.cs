using System.Collections.Generic;
using System.Threading.Tasks;

namespace plr.Commands
{
    internal interface ICommand
    {
        string[] Name { get; }
        Task<CommandResult> Execute(IEnumerable<string> parameters);
    }
}