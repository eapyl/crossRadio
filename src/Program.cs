using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using Serilog;

namespace plr
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo.RollingFile("log-{Date}.txt", fileSizeLimitBytes: 10 * 1000000, retainedFileCountLimit: 5)
                    .CreateLogger();

            Log.Verbose("Started");

            var configuration = await new ConfigurationLoader(async x => await File.ReadAllTextAsync(x)).Load();
            var stationManager = new StationManager(configuration.DatabaseLink);
            await stationManager.LoadStation();
            var radio = new Radio(stationManager);

            try
            {
                if (!radio.Init())
                {
                    Log.Error("Can't init bass library.");
                    return;
                }

                var toExit = false;
                Log.Information("Starting listening commands.");
                while (!toExit)
                {
                    var command = Console.ReadLine();
                    var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 0) continue;
                    var mainCommand = parts[0];
                    switch (mainCommand)
                    {
                        case "":
                        case "--list":
                        case "-l":
                            foreach (var station in stationManager.Search())
                                Console.WriteLine(station.ToString());
                            break;
                        case "--play":
                        case "-p":
                            if (parts.Length < 2) continue;
                            if (Int32.TryParse(parts[1], out int id))
                            {
                                var station = stationManager.Search(id);
                                if (station == null) continue;
                                radio.Play(station.Uri.First().ToString());
                            }
                            break;
                        case "-pa":
                        case "--pause":
                            radio.Pause();
                            break;
                        case "-st":
                        case "--start":
                            radio.Start();
                            break;
                        case "--volumeUp":
                        case "-vu":
                            radio.VolumeUp();
                            break;
                        case "--volumeDown":
                        case "-vd":
                            radio.VolumeDown();
                            break;
                        case "--stop":
                        case "-s":
                            toExit = true;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("{ex}", ex);
            }
            finally
            {
                radio.Stop();
            }
        }
    }
}
