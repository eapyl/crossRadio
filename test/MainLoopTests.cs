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

namespace test
{
    public class MainLoopTests
    {
        [Fact]
        public void Create()
        {
            var log = A.Fake<ILogger>();
            var configurationProvider = A.Fake<IConfigurationProvider>();
            var radio = A.Fake<IRadio>();
            var input = A.Fake<Func<string>>();

            var loop = new MainLoop(input, log, radio, new ICommand[0]);
            Assert.NotNull(loop);
        }

        [Fact]
        public async Task Run()
        {
            var log = A.Fake<ILogger>();
            var configurationProvider = A.Fake<IConfigurationProvider>();
            var radio = A.Fake<IRadio>();
            var input = A.Fake<Func<string>>();
            var command = A.Fake<ICommand>();

            A.CallTo(() => input()).Returns("-h");
            A.CallTo(() => command.Name).Returns(new []{"-h"});
            A.CallTo(() => radio.Init()).Returns(true);
            A.CallTo(() => command.Execute(A<IEnumerable<string>>._)).Returns(CommandResult.Exit);

            var loop = new MainLoop(input, log, radio, new ICommand[]{command});
            await loop.Run();

            A.CallTo(() => log.Error(A<string>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task RunWithInitError()
        {
            var log = A.Fake<ILogger>();
            var configurationProvider = A.Fake<IConfigurationProvider>();
            var radio = A.Fake<IRadio>();
            var input = A.Fake<Func<string>>();

            var loop = new MainLoop(input, log, radio, new ICommand[0]);
            await loop.Run();

            A.CallTo(() => log.Error(A<string>._)).MustHaveHappened();
        }

        [Fact]
        public async Task RunWithException()
        {
            var log = A.Fake<ILogger>();
            var configurationProvider = A.Fake<IConfigurationProvider>();
            var radio = A.Fake<IRadio>();
            var input = A.Fake<Func<string>>();
            var command = A.Fake<ICommand>();

            A.CallTo(() => input()).Returns("-h");
            A.CallTo(() => command.Name).Returns(new []{"-h"});
            A.CallTo(() => radio.Init()).Returns(true);
            A.CallTo(() => command.Execute(A<IEnumerable<string>>._)).Throws(x => new NotImplementedException());

            var loop = new MainLoop(input, log, radio, new ICommand[]{command});
            await loop.Run();

            A.CallTo(() => log.Error(A<string>._, A<Exception>._)).MustHaveHappened();
        }

        [Fact]
        public async Task RunOkCommand()
        {
            var log = A.Fake<ILogger>();
            var configurationProvider = A.Fake<IConfigurationProvider>();
            var radio = A.Fake<IRadio>();
            var input = A.Fake<Func<string>>();
            var commandHelp = A.Fake<ICommand>();
            var commandStop = A.Fake<ICommand>();

            A.CallTo(() => input()).ReturnsNextFromSequence("-h", "-s");
            A.CallTo(() => commandHelp.Name).Returns(new []{"-h"});
            A.CallTo(() => commandStop.Name).Returns(new []{"-s"});
            A.CallTo(() => radio.Init()).Returns(true);
            A.CallTo(() => commandHelp.Execute(A<IEnumerable<string>>._)).Returns(CommandResult.OK);
            A.CallTo(() => commandStop.Execute(A<IEnumerable<string>>._)).Returns(CommandResult.Exit);

            var loop = new MainLoop(input, log, radio, new ICommand[]{ commandHelp, commandStop });
            await loop.Run();

            A.CallTo(() => log.Error(A<string>._)).MustNotHaveHappened();
        }
    }
}