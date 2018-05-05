using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Serilog.Core;
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
        Update
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
        private readonly Logger _log;

        public TelegramBot(string key, StationManager stationManager, Logger log)
        {
            _stationManager = stationManager;
            _bot = new TelegramBotClient(key);
            _log = log;
        }
        public void Start()
        {
            var me = _bot.GetMeAsync().Result;

            _bot.OnMessage += BotOnMessageReceived;
            _bot.OnReceiveError += BotOnReceiveError;
            _bot.OnCallbackQuery += BotOnCallbackQueryReceived;

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
            {
                CommandRequest(this, new CommandRequestEventArgs { Command = BotCommand.VolumeUp });
                await _bot.SendTextMessageAsync(message.Chat.Id, "Volume up.");
            }
            else if (message.Text == "/down")
            {
                CommandRequest(this, new CommandRequestEventArgs { Command = BotCommand.VolumeDown });
                await _bot.SendTextMessageAsync(message.Chat.Id, "Volume down.");
            }
            else if (message.Text == "/start")
            {
                CommandRequest(this, new CommandRequestEventArgs { Command = BotCommand.Start });
                await _bot.SendTextMessageAsync(message.Chat.Id, "Start to play.");
            }
            else if (message.Text == "/stop")
            {
                CommandRequest(this, new CommandRequestEventArgs { Command = BotCommand.Stop });
                await _bot.SendTextMessageAsync(message.Chat.Id, "Stop plaing.");
            }
            else if (message.Text == "/pause")
            {
                CommandRequest(this, new CommandRequestEventArgs { Command = BotCommand.Pause });
                await _bot.SendTextMessageAsync(message.Chat.Id, "Pause playing.");
            }
            else if (message.Text == "/update")
            {
                CommandRequest(this, new CommandRequestEventArgs { Command = BotCommand.Update });
                await _bot.SendTextMessageAsync(message.Chat.Id, "Updating station list.");
            }
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

                await _bot.SendTextMessageAsync(message.Chat.Id, "Stations (please use /update command to get latests):", replyMarkup: stationButtons);
            }
            else if (Uri.TryCreate(message.Text, UriKind.Absolute, out Uri r))
            {
                RadioChangeRequest(this, new RadioChangeRequestEventArgs() { Uri = message.Text });
                await _bot.SendTextMessageAsync(message.Chat.Id, "Start playing entered uri.");
            }
            else
            {
                await _bot.SendTextMessageAsync(
                    message.Chat.Id,
                    "Can't udnerstand command");
            }
        }

        private async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            var callbackQuery = callbackQueryEventArgs.CallbackQuery;
            RadioChangeRequest(this, new RadioChangeRequestEventArgs() { Uri = callbackQuery.Data });
            await _bot.AnswerCallbackQueryAsync(callbackQuery.Id, $"Plaing {callbackQuery.Data}");
        }

        private void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            _log.Error("Received error: {0} — {1}",
                receiveErrorEventArgs.ApiRequestException.ErrorCode,
                receiveErrorEventArgs.ApiRequestException.Message);
        }
    }
}