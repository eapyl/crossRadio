using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using plr;
using plr.Commands;
using plr.Entities;
using plr.Providers;
using Serilog;
using Xunit;

namespace test.Commands
{
    public class StopCommandTests
    {
        [Fact]
        public void Create()
        {
            var log = A.Fake<ILogger>();
            var radio = A.Fake<IRadio>();

            var command = new StopCommand(log, radio);
            Assert.NotNull(command);
        }

        [Fact]
        public async Task Execute()
        {
            var log = A.Fake<ILogger>();
            var radio = A.Fake<IRadio>();

            var command = new StopCommand(log, radio);

            var result = await command.Execute(new string[0]);
            A.CallTo(() => radio.Stop()).MustHaveHappened();
            Assert.Equal(CommandResult.Exit, result);
        }

        [Fact]
        public void CheckName()
        {
            var log = A.Fake<ILogger>();
            var radio = A.Fake<IRadio>();

            var command = new StopCommand(log, radio);

            Assert.Contains("-s", command.Name);
            Assert.Contains("--stop", command.Name);
            Assert.Contains("Stop playing and exit", command.Description);
        }
    }
}