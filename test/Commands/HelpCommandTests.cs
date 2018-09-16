using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using plr.Commands;
using plr.Entities;
using plr.Providers;
using Serilog;
using Xunit;

namespace test.Commands
{
    public class HelpCommandTests
    {
        [Fact]
        public void Create()
        {
            var commands = A.Fake<IEnumerable<ICommand>>();

            var command = new HelpCommand((s) => { }, commands);
            Assert.NotNull(command);
        }

        [Fact]
        public async Task Execute()
        {
            var output = A.Fake<Action<string>>();
            var command = A.Fake<ICommand>();
            var commands = new [] { command };

            var helpCommand = new HelpCommand(output, commands);

            var result = await helpCommand.Execute(new string[0]);
            A.CallTo(output).MustHaveHappenedANumberOfTimesMatching(n => n == 2);
            Assert.Equal(CommandResult.OK, result);
        }

        [Fact]
        public void CheckName()
        {
            var commands = A.Fake<IEnumerable<ICommand>>();
            var command = new HelpCommand((s) => { }, commands);

            Assert.Contains("-h", command.Name);
            Assert.Contains("--help", command.Name);
            Assert.Contains("Show help information", command.Description);
        }
    }
}