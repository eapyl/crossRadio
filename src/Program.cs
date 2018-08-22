using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using Serilog;
using plr.Providers;
using plr.Commands;
using plr.Entities;

namespace plr
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var locationOfRadioApplication = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

            var log = new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo.RollingFile(Path.Combine(locationOfRadioApplication, "log-{Date}.txt"), fileSizeLimitBytes: 10 * 1000000, retainedFileCountLimit: 5)
                    .CreateLogger();

            Action<string> output = x => Console.WriteLine(x);
            var configurationProvider = new ConfigurationProvider(
                locationOfRadioApplication,
                async x => await File.ReadAllTextAsync(x),
                async (x, y) => await File.WriteAllTextAsync(x, y));

            var stationValidator = new StationValidator();
            var stationProvider = new StationProvider(log, configurationProvider, stationValidator);
            
            var radio = new Radio();

            var commands = new ICommand[]
            {
                new DatabaseCommand(log, output, configurationProvider, stationProvider),
                new HelpCommand(output),
                new ListCommand(log, output, stationProvider),
                new PauseCommand(log, radio),
                new PlayCommand(log, output, stationProvider, radio),
                new StartCommand(log, radio),
                new StopCommand(log, radio),
                new VolumeUpCommand(log, radio),
                new VolumeDownCommand(log, radio)
            };

            log.Verbose("Start");

            try
            {
                if (!radio.Init())
                {
                    log.Error("Can't init bass library.");
                    return;
                }
                
                await stationProvider.LoadStation();

                log.Information("Listening commands");
                while (true)
                {
                    var input = Console.ReadLine();
                    var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 0) continue;
                    var command = commands.FirstOrDefault(x => x.Name.Any(name => name == parts[0]));
                    if (command == null) continue;
                    var result = await command.Execute(parts.Skip(1));
                    if (result == CommandResult.Exit) break;
                }
            }
            catch (Exception ex)
            {
                log.Error("{ex}", ex);
            }
        }
    }
}
