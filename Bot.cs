using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace rsRadio
{
    public struct RadioChangeRequestEventArgs
    {
        public string Uri;
    }

    public enum BotCommand
    {
        Start,
        Stop,
        Pause,
        VolumeUp,
        VolumeDown,
    }

    public struct CommandRequestEventArgs
    {
        public BotCommand Command;
    }

    public delegate void RadioChangeRequestEventHandler(object sender, RadioChangeRequestEventArgs e);
    public delegate void CommandRequestEventHandler(object sender, CommandRequestEventArgs e);

    internal class TelegramBot
    {
        private readonly StationManager _stationManager;
        private readonly TelegramBotClient _bot;
        public TelegramBot(string key, StationManager stationManager)
        {
            _stationManager = stationManager;
            _bot = new TelegramBotClient(key);
        }
        public void Start()
        {
            var me = _bot.GetMeAsync().Result;

            _bot.OnMessage += BotOnMessageReceived;
            _bot.OnReceiveError += BotOnReceiveError;

            _bot.StartReceiving(Array.Empty<UpdateType>());
        }
        public void Stop()
        {
            _bot.StopReceiving();
        }
        public event RadioChangeRequestEventHandler RadioChangeRequest;
        public event CommandRequestEventHandler CommandRequest;
        private async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;

            if (message == null || message.Type != MessageType.Text) return;

            if (message.Text == "/up")
                CommandRequest(this, new CommandRequestEventArgs { Command = BotCommand.VolumeUp });
            else if (message.Text == "/down")
                CommandRequest(this, new CommandRequestEventArgs { Command = BotCommand.VolumeDown });
            else if (message.Text == "/start")
                CommandRequest(this, new CommandRequestEventArgs { Command = BotCommand.Start });
            else if (message.Text == "/stop")
                CommandRequest(this, new CommandRequestEventArgs { Command = BotCommand.Stop });
            else if (message.Text == "/pause")
                CommandRequest(this, new CommandRequestEventArgs { Command = BotCommand.Pause });
            else if (message.Text.StartsWith("/stations"))
            {
                var buttons = new List<List<InlineKeyboardButton>>();
                var middleArray = new List<InlineKeyboardButton>();
                var i = 0;
                var stations = _stationManager.Search();
                foreach (var station in stations)
                {
                    if (i % 3 == 0)
                    {
                        middleArray = new List<InlineKeyboardButton>();
                        buttons.Add(middleArray);
                    }
                    middleArray.Add(InlineKeyboardButton.WithCallbackData(station.Name, station.Uri));
                    i++;
                }
                var stationButtons = new InlineKeyboardMarkup(buttons);

                await _bot.SendTextMessageAsync(
                    message.Chat.Id,
                    "Stations:",
                    replyMarkup: stationButtons);
            }
            else if (Uri.TryCreate(message.Text, UriKind.Absolute, out Uri r))
            {
                RadioChangeRequest(this, new RadioChangeRequestEventArgs() { Uri = message.Text });
            }
            else
            {
                await _bot.SendTextMessageAsync(
                    message.Chat.Id,
                    "Can't udnerstand command");
            }
        }
        private void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Console.WriteLine("Received error: {0} — {1}",
                receiveErrorEventArgs.ApiRequestException.ErrorCode,
                receiveErrorEventArgs.ApiRequestException.Message);
        }
    }
}