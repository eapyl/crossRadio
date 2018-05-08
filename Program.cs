using System;
using ManagedBass;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using System.Linq;
using Telegram.Bot.Types.ReplyMarkups;
using System.IO;
using Telegram.Bot.Types.InlineQueryResults;
using Serilog;

namespace rsRadio
{
    class Program
    {
        private static AutoResetEvent stop = new AutoResetEvent(false);


        static async Task Main(string[] args)
        {
            var log = new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo.RollingFile("log-{Date}.txt", fileSizeLimitBytes: 10 * 1000000, retainedFileCountLimit: 5)
                    .WriteTo.Console()
                    .CreateLogger();

            log.Verbose("Started");

            var exit = false;

            var configuration = await new ConfigurationLoader(async x => await File.ReadAllTextAsync(x)).Load();
            var stationManager = new StationManager(configuration.DatabaseLink);
            await stationManager.LoadStation();
            var bot = new TelegramBot(configuration.TelegramBotKey, stationManager, log);
            var radio = new Radio(stationManager, log);

            bot.CommandRequest += async (obj, data) =>
            {
                switch (data.Command)
                {
                    case BotCommand.Pause:
                        radio.Pause();
                        break;
                    case BotCommand.Start:
                        radio.Start();
                        break;
                    case BotCommand.Stop:
                        exit = true;
                        break;
                    case BotCommand.VolumeUp:
                        radio.VolumeUp();
                        break;
                    case BotCommand.VolumeDown:
                        radio.VolumeDown();
                        break;
                    case BotCommand.Update:
                        await stationManager.LoadStation();
                        break;
                }
            };

            bot.RadioChangeRequest += (obj, data) =>
            {
                radio.Play(data.Uri);
            };

            try
            {
                if (!radio.Init())
                {
                    log.Error("Can't init bass library.");
                    return;
                }
                bot.Start();
                radio.Play(stationManager.Current.Uri);

                while (true)
                {
                    Thread.Sleep(1000);
                    if (exit) break;
                }
            }
            finally
            {
                bot.Stop();
                radio.Stop();
            }
        }
    }
}
