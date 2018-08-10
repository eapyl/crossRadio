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
            var locationOfRadioApplication = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

            Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo.RollingFile(Path.Combine(locationOfRadioApplication, "log-{Date}.txt"), fileSizeLimitBytes: 10 * 1000000, retainedFileCountLimit: 5)
                    .CreateLogger();

            Log.Verbose("Started");

            var configurationLoader = new ConfigurationLoader(
                locationOfRadioApplication,
                async x => await File.ReadAllTextAsync(x),
                async (x, y) => await File.WriteAllTextAsync(x, y));
            var configuration = await configurationLoader.Load();
            var stationManager = new StationManager(configuration);
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
                        case "-h":
                        case "--help":
                            Console.WriteLine("List of supported commands:");
                            Console.WriteLine("  -h, --help: Show descriptopn of all commands;");
                            Console.WriteLine("  -l, --list: Show all stations from database;");
                            Console.WriteLine("  -p {ID}, --play {ID}: Play selected station using ID of station;");
                            Console.WriteLine("  -pa, --pause: Pause playing;");
                            Console.WriteLine("  -st, --start: Start play after pause;");
                            Console.WriteLine("  -vu [delta], --volumeUp [delta]: Increase volume by default value or defined by [delta];");
                            Console.WriteLine("  -vd [delta], --volumeDown [delta]: Decrease volume by default value or defined by [delta];");
                            Console.WriteLine("  -s, --stop: Stop plaing and exit;");
                            Console.WriteLine("  -db {uri}, --database {uri}: Change link to database (default value is https://raw.githubusercontent.com/eapyl/radio-stations/master/db.json);");
                            break;
                        case "--list":
                        case "-l":
                            Log.Verbose("Received list command.");
                            foreach (var station in stationManager.Search())
                                Console.WriteLine(station.ToString());
                            break;
                        case "--play":
                        case "-p":
                            Log.Verbose("Received play command.");
                            if (parts.Length < 2)
                            {
                                Log.Verbose("There is no selected radio station to play.");
                                continue;
                            }
                            if (Int32.TryParse(parts[1], out int id))
                            {
                                var station = stationManager.Search(id);
                                if (station == null)
                                {
                                    Log.Error("There is no station with selected ID.");
                                    continue;
                                }
                                radio.Play(station.Uri.First().ToString());
                            }
                            else
                            {
                                Log.Error("Id of station should be number.");
                            }
                            break;
                        case "-pa":
                        case "--pause":
                            Log.Verbose("Received pause command.");
                            radio.Pause();
                            break;
                        case "-st":
                        case "--start":
                            Log.Verbose("Received start command.");
                            radio.Start();
                            break;
                        case "--volumeUp":
                        case "-vu":
                            Log.Verbose("Received volume Up command.");
                            if (parts.Length > 1 && Double.TryParse(parts[1], out double deltaUp))
                            {
                                Log.Verbose($"Increase volume by {deltaUp}");
                                radio.VolumeUp(deltaUp);
                            }
                            else
                            {
                                radio.VolumeUp();
                            }
                            break;
                        case "--volumeDown":
                        case "-vd":
                            Log.Verbose("Received volume Down command.");
                            if (parts.Length > 1 && Double.TryParse(parts[1], out double deltaDown))
                            {
                                Log.Verbose($"Decrease volume by {deltaDown}");
                                radio.VolumeDown(deltaDown);
                            }
                            else
                            {
                                radio.VolumeDown();
                            }
                            break;
                        case "--stop":
                        case "-s":
                            Log.Verbose("Received stop command.");
                            toExit = true;
                            break;
                        case "--database":
                        case "-db":
                            Log.Verbose("Received database command.");
                            if (parts.Length < 2)
                            {
                                Log.Verbose("There is no url to new database.");
                                continue;
                            }
                            if (Uri.TryCreate(parts[1], UriKind.Absolute, out Uri url))
                            {
                                configuration.DatabaseLink = url.ToString();
                                await configurationLoader.Upload(configuration);
                                await stationManager.LoadStation();
                            }
                            else
                            {
                                Log.Error("Url to database is incorrect.");
                            }
                            break;
                        default:
                            Log.Verbose($"Command {mainCommand} is not supported.");
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
