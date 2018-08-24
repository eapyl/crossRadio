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
    public class StartCommandTests
    {
        [Fact]
        public void Create()
        {
            var log = A.Fake<ILogger>();
            var radio = A.Fake<IRadio>();

            var command = new StartCommand(log, radio);
            Assert.NotNull(command);
        }

        [Fact]
        public async Task Execute()
        {
            var log = A.Fake<ILogger>();
            var radio = A.Fake<IRadio>();

            var command = new StartCommand(log, radio);

            var result = await command.Execute(new string[0]);
            A.CallTo(() => radio.Start()).MustHaveHappened();
            Assert.Equal(CommandResult.OK, result);
        }

        [Fact]
        public void CheckName()
        {
            var log = A.Fake<ILogger>();
            var radio = A.Fake<IRadio>();

            var command = new StartCommand(log, radio);

            Assert.Contains("-st", command.Name);
            Assert.Contains("--start", command.Name);
        }
    }
}