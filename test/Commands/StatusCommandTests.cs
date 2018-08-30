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
    public class StatusCommandTests
    {
        [Fact]
        public void Create()
        {
            var log = A.Fake<ILogger>();
            var radio = A.Fake<IRadio>();
            var output = A.Fake<Action<string>>();

            var command = new StatusCommand(log, output, radio);
            Assert.NotNull(command);
        }

        [Fact]
        public async Task Execute()
        {
            var log = A.Fake<ILogger>();
            var radio = A.Fake<IRadio>();
            var output = A.Fake<Action<string>>();

            var command = new StatusCommand(log, output, radio);

            var result = await command.Execute(new string[0]);
            A.CallTo(() => radio.Status()).MustHaveHappened();
            Assert.Equal(CommandResult.OK, result);
        }

        [Fact]
        public void CheckName()
        {
            var log = A.Fake<ILogger>();
            var radio = A.Fake<IRadio>();
            var output = A.Fake<Action<string>>();

            var command = new StatusCommand(log, output, radio);

            Assert.Contains(string.Empty, command.Name);
        }
    }
}