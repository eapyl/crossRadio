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
    public class VolumeDownCommandTests
    {
        [Fact]
        public void Create()
        {
            var log = A.Fake<ILogger>();
            var radio = A.Fake<IRadio>();

            var command = new VolumeDownCommand(log, radio);
            Assert.NotNull(command);
        }

        [Fact]
        public async Task ExecuteWithoutValue()
        {
            var log = A.Fake<ILogger>();
            var radio = A.Fake<IRadio>();

            var command = new VolumeDownCommand(log, radio);

            var result = await command.Execute(new string[0]);
            A.CallTo(() => radio.VolumeDown(A<int>.That.IsEqualTo(10))).MustHaveHappened();
            Assert.Equal(CommandResult.OK, result);
        }

        [Fact]
        public async Task ExecuteWithValue()
        {
            var log = A.Fake<ILogger>();
            var radio = A.Fake<IRadio>();

            var command = new VolumeDownCommand(log, radio);

            var result = await command.Execute(new string[]{ "50" });
            A.CallTo(() => radio.VolumeDown(A<int>.That.IsEqualTo(50))).MustHaveHappened();
            Assert.Equal(CommandResult.OK, result);
        }

        [Fact]
        public void CheckName()
        {
            var log = A.Fake<ILogger>();
            var radio = A.Fake<IRadio>();

            var command = new VolumeDownCommand(log, radio);

            Assert.Contains("-vd", command.Name);
            Assert.Contains("--volumeDown", command.Name);
        }
    }
}