using System.Threading.Tasks;
using FakeItEasy;
using plr.Commands;
using plr.Entities;
using plr.Providers;
using Serilog;
using Xunit;

namespace test.Commands
{
    public class DatabaseCommandTests
    {
        [Fact]
        public void Create()
        {
            var log = A.Fake<ILogger>();
            var configurationProvider = A.Fake<IConfigurationProvider>();
            var stationProvider = A.Fake<IStationProvider>();
            var command = new DatabaseCommand(log, s => {}, configurationProvider, stationProvider);
            Assert.NotNull(command);
        }

        [Fact]
        public async Task Execute()
        {
            var log = A.Fake<ILogger>();
            var configurationProvider = A.Fake<IConfigurationProvider>();
            var stationProvider = A.Fake<IStationProvider>();
            var command = new DatabaseCommand(log, s => {}, configurationProvider, stationProvider);

            var result = await command.Execute(new[] {"http://test.com"});
            A.CallTo(() => configurationProvider.Upload(A<Configuration>._)).MustHaveHappened();
            A.CallTo(() => stationProvider.Reset()).MustHaveHappened();
            Assert.Equal(CommandResult.OK, result);
        }

        [Fact]
        public async Task ExecuteWithNoParameters()
        {
            var log = A.Fake<ILogger>();
            var configurationProvider = A.Fake<IConfigurationProvider>();
            var stationProvider = A.Fake<IStationProvider>();
            var command = new DatabaseCommand(log, s => {}, configurationProvider, stationProvider);

            var result = await command.Execute(new string[0]);
            A.CallTo(() => configurationProvider.Upload(A<Configuration>._)).MustNotHaveHappened();
            A.CallTo(() => stationProvider.Reset()).MustNotHaveHappened();
            Assert.Equal(CommandResult.Error, result);
        }
    }
}