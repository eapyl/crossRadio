using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using Serilog;
using plr.Providers;
using plr.Commands;
using plr.Entities;
using DryIoc;

namespace plr
{
    class Program
    {
        static async Task Main(string[] args) => await SetUp().Resolve<IMainLoop>().Run();

        private static IContainer SetUp()
        {
            var container = new Container();
            container.Register(made: Made.Of(() => LocationFactory.Get()), reuse: Reuse.Singleton);
            container.RegisterDelegate<Action<string>>((r) => x => Console.WriteLine(x));
            container.RegisterDelegate<Func<string>>((r) => () => Console.ReadLine());
            container.Register<ILogger>(made: Made.Of(() => LogFactory.Get(Arg.Of<string>())), reuse: Reuse.Singleton);
            container.Register<IConfigurationProvider>(made: Made.Of(() => ConfigurationProviderFactory.Get(Arg.Of<string>())), reuse: Reuse.Singleton);
            container.Register<StationValidator>(reuse: Reuse.Singleton);
            container.Register<IStationProvider, StationProvider>(Reuse.Singleton);
            container.Register<IRadio, Radio>(Reuse.Singleton);
            container.Register<ICommand, DatabaseCommand>();
            container.Register<ICommand, HelpCommand>();
            container.Register<ICommand, ListCommand>();
            container.Register<ICommand, PauseCommand>();
            container.Register<ICommand, PlayCommand>();
            container.Register<ICommand, StartCommand>();
            container.Register<ICommand, VolumeCommand>();
            container.Register<ICommand, StopCommand>();
            container.Register<ICommand, StatusCommand>();
            container.Register<ICommand, VolumeUpCommand>();
            container.Register<ICommand, VolumeDownCommand>();
            container.Register<IMainLoop, MainLoop>(reuse: Reuse.Singleton);

            return container;
        }

        private static class LogFactory
        {
            public static ILogger Get(string location) => 
                new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo.RollingFile(Path.Combine(location, "log-{Date}.txt"))
                    .CreateLogger();
        }

        private static class LocationFactory
        {
            public static string Get() => Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        }

        private static class ConfigurationProviderFactory
        {
            public static IConfigurationProvider Get(string location) => 
                new ConfigurationProvider(
                    location,
                    async x => await File.ReadAllTextAsync(x),
                    async (x, y) => await File.WriteAllTextAsync(x, y));
        }
    }
}
