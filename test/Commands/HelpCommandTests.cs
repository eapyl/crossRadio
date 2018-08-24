using System;
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
            var command = new HelpCommand((s) => { });
            Assert.NotNull(command);
        }

        [Fact]
        public async Task Execute()
        {
            var output = A.Fake<Action<string>>();
            var command = new HelpCommand(output);

            var result = await command.Execute(new string[0]);
            A.CallTo(output).MustHaveHappenedANumberOfTimesMatching(n => n == 10);
            Assert.Equal(CommandResult.OK, result);
        }
    }
}