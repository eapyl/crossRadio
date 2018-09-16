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
    public class VolumeCommandTests
    {
        [Fact]
        public void Create()
        {
            var log = A.Fake<ILogger>();
            var radio = A.Fake<IRadio>();
            var provider = A.Fake<IConfigurationProvider>();

            var command = new VolumeCommand(log, (s) => {}, provider, radio);
            Assert.NotNull(command);
        }

        [Fact]
        public async Task ExecuteWithoutValue()
        {
            var log = A.Fake<ILogger>();
            var radio = A.Fake<IRadio>();
            var provider = A.Fake<IConfigurationProvider>();
            var output = A.Fake<Action<string>>();

            var command = new VolumeCommand(log, output, provider, radio);

            var result = await command.Execute(new string[0]);
            A.CallTo(output).MustHaveHappened();
            Assert.Equal(CommandResult.OK, result);
        }

        [Fact]
        public async Task ExecuteWithValue()
        {
            var log = A.Fake<ILogger>();
            var radio = A.Fake<IRadio>();
            var provider = A.Fake<IConfigurationProvider>();
            var output = A.Fake<Action<string>>();

            var command = new VolumeCommand(log, output, provider, radio);

            var result = await command.Execute(new string[] {"0"});

            A.CallTo(output).MustNotHaveHappened();
            A.CallTo(() => radio.Volume(A<int>.That.IsEqualTo(0))).MustHaveHappened();
            A.CallTo(() => provider.Upload(A<Configuration>._)).MustHaveHappened();
            A.CallTo(() => provider.Load()).MustHaveHappened();
            Assert.Equal(CommandResult.OK, result);
        }

        [Fact]
        public void CheckName()
        {
            var log = A.Fake<ILogger>();
            var radio = A.Fake<IRadio>();
            var provider = A.Fake<IConfigurationProvider>();

            var command = new VolumeCommand(log, (s) => {}, provider, radio);

            Assert.Contains("-v", command.Name);
            Assert.Contains("--volume", command.Name);
            Assert.Contains("Set {VALUE}% volume directly", command.Description);
        }
    }
}