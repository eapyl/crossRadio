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
                .CreateLogger();

            var configuration = await new ConfigurationLoader(async x => await File.ReadAllTextAsync(x)).Load();
            var stationManager = new StationManager(configuration.DatabaseLink);
            var bot = new TelegramBot(configuration.TelegramBotKey, stationManager);
            var radio = new Radio(stationManager);

            bot.CommandRequest += (obj, data) =>
            {
                switch(data.Command)
                {
                    case BotCommand.Pause:
                        radio.Pause();
                        break;
                    case BotCommand.Start:
                        radio.Start();
                        break;
                    case BotCommand.Stop:
                        radio.Stop();
                        break;
                    case BotCommand.VolumeUp:
                        radio.VolumeUp();
                        break;
                    case BotCommand.VolumeDown:
                        radio.VolumeDown();
                        break;
                }
            };

            bot.RadioChangeRequest += (obj, data) =>
            {
                radio.Play(data.Uri);
            };

            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;

            var botTask = Task.Run(() =>
            {
                bot.Start();

                while (true)
                {
                    Thread.Sleep(1000);
                    if (token.IsCancellationRequested)
                        break;
                }

                bot.Stop();
            }, token);

            var radioTask = Task.Run(() =>
             {
                 if (!radio.Init())
                 {
                     log.Error("Can't init bass library.");
                     return;
                 }

                 radio.Play(stationManager.Current.Uri);

                 while (true)
                 {
                     Thread.Sleep(1000);
                     if (token.IsCancellationRequested)
                         break;
                 }

                 radio.Stop();
            }, token);

            Console.ReadKey();

            tokenSource.Cancel();

            Task.WaitAll(botTask, radioTask);
        }
    }
}
