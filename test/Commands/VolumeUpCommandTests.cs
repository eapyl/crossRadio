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
    public class VolumeUpCommandTests
    {
        [Fact]
        public void Create()
        {
            var log = A.Fake<ILogger>();
            var radio = A.Fake<IRadio>();
            var provider = A.Fake<IConfigurationProvider>();

            var command = new VolumeUpCommand(log, provider, radio);
            Assert.NotNull(command);
        }

        [Fact]
        public async Task ExecuteWithoutValue()
        {
            var log = A.Fake<ILogger>();
            var radio = A.Fake<IRadio>();
            var provider = A.Fake<IConfigurationProvider>();

            var command = new VolumeUpCommand(log, provider, radio);

            var result = await command.Execute(new string[0]);
            A.CallTo(() => radio.VolumeUp()).MustHaveHappened();
            Assert.Equal(CommandResult.OK, result);
        }

        [Fact]
        public void CheckName()
        {
            var log = A.Fake<ILogger>();
            var radio = A.Fake<IRadio>();
            var provider = A.Fake<IConfigurationProvider>();

            var command = new VolumeUpCommand(log, provider, radio);

            Assert.Contains("-vu", command.Name);
            Assert.Contains("--volumeUp", command.Name);
            Assert.Contains("Increase volume by 10%", command.Description);
        }
    }
}