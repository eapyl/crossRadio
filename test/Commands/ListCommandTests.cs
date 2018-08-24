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
    public class ListCommandTests
    {
        [Fact]
        public void Create()
        {
            var log = A.Fake<ILogger>();
            var stationProvider = A.Fake<IStationProvider>();

            var command = new ListCommand(log, (s) => {}, stationProvider);
            Assert.NotNull(command);
        }

        [Fact]
        public async Task Execute()
        {
            var output = A.Fake<Action<string>>();
            var log = A.Fake<ILogger>();
            var stationProvider = A.Fake<IStationProvider>();
            var list = new List<Station>{ new Station() };

            A.CallTo(() => stationProvider.Search(A<string>._)).Returns(list);

            var command = new ListCommand(log, output, stationProvider);

            var result = await command.Execute(new string[0]);
            A.CallTo(output).MustHaveHappenedANumberOfTimesMatching(n => n == 1);
            Assert.Equal(CommandResult.OK, result);
        }

        [Fact]
        public void CheckName()
        {
            var log = A.Fake<ILogger>();
            var stationProvider = A.Fake<IStationProvider>();

            var command = new ListCommand(log, (s) => {}, stationProvider);

            Assert.Contains("-l", command.Name);
            Assert.Contains("--list", command.Name);
        }
    }
}