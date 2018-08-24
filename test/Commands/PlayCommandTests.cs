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
    public class PlayCommandTests
    {
        [Fact]
        public void Create()
        {
            var log = A.Fake<ILogger>();
            var radio = A.Fake<IRadio>();
            var stationProvider = A.Fake<IStationProvider>();

            var command = new PlayCommand(log, s => {}, stationProvider, radio);
            Assert.NotNull(command);
        }

        [Fact]
        public async Task ExecuteWithoutValue()
        {
            var log = A.Fake<ILogger>();
            var radio = A.Fake<IRadio>();
            var stationProvider = A.Fake<IStationProvider>();

            var command = new PlayCommand(log, s => {}, stationProvider, radio);

            var result = await command.Execute(new string[0]);
            Assert.Equal(CommandResult.Error, result);
        }

        [Fact]
        public async Task ExecuteWithValue()
        {
            var log = A.Fake<ILogger>();
            var radio = A.Fake<IRadio>();
            var stationProvider = A.Fake<IStationProvider>();

            A.CallTo(() => stationProvider.Search(0)).Returns(new Station{Uri = new[] { "http:://test" }});

            var command = new PlayCommand(log, s => {}, stationProvider, radio);

            var result = await command.Execute(new string[]{ "0" });
            A.CallTo(() => radio.Play(A<string>.That.IsEqualTo("http:://test"))).MustHaveHappened();
            Assert.Equal(CommandResult.OK, result);
        }

        [Fact]
        public async Task ExecuteWithNoStation()
        {
            var log = A.Fake<ILogger>();
            var radio = A.Fake<IRadio>();
            var stationProvider = A.Fake<IStationProvider>();

            A.CallTo(() => stationProvider.Search(0)).Returns<Station>(null);

            var command = new PlayCommand(log, s => {}, stationProvider, radio);

            var result = await command.Execute(new string[]{ "0" });
            Assert.Equal(CommandResult.Error, result);
        }

        [Fact]
        public void CheckName()
        {
            var log = A.Fake<ILogger>();
            var radio = A.Fake<IRadio>();
            var stationProvider = A.Fake<IStationProvider>();

            var command = new PlayCommand(log, s => {}, stationProvider, radio);

            Assert.Contains("-p", command.Name);
            Assert.Contains("--play", command.Name);
        }
    }
}