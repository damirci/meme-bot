using MemeBot.Config;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace MemeBot.Bots
{
    public class MemeReceiver
    {
        private readonly TelegramBotClient _telegramBotClient;
        private readonly ILogger _logger;
        private readonly AppConfig _appConfig;


        public MemeReceiver(TelegramBotClient telegramBotClient, ILogger logger, AppConfig appConfig)
        {
            _telegramBotClient = telegramBotClient;
            _logger = logger;
            _appConfig = appConfig;

            _telegramBotClient.OnMessage += TelegramBotClient_OnMessage;
            _telegramBotClient.OnReceiveError += TelegramBotClient_OnReceiveError;
            _telegramBotClient.OnReceiveGeneralError += TelegramBotClient_OnReceiveGeneralError;

        }

        public void Start()
        {
            var bot = _telegramBotClient.GetMeAsync().GetAwaiter().GetResult();
            _logger.LogInformation($"I am MemeReceiver bot number {bot.Id} and my name is {bot.FirstName}.");

            _telegramBotClient.StartReceiving();
        }

        public void Stop()
        {
            _telegramBotClient.StartReceiving();
        }

        private void TelegramBotClient_OnReceiveGeneralError(object sender, ReceiveGeneralErrorEventArgs e)
        {
            _logger.LogError(e.Exception.ToString());
        }

        private void TelegramBotClient_OnReceiveError(object sender, ReceiveErrorEventArgs e)
        {
            _logger.LogError(e.ApiRequestException.ToString());
        }

        private void TelegramBotClient_OnMessage(object sender, MessageEventArgs e)
        {
            var msg = e.Message;

            _logger.LogInformation($"Message received from user {msg.From.Id}:{msg.From.Username} with Id: { msg.MessageId}");
            try
            {
                StoreMessage(msg);

                switch (e.Message.Type)
                {
                    case Telegram.Bot.Types.Enums.MessageType.Text:
                        HandleTextMessage(msg);
                        break;
                    case Telegram.Bot.Types.Enums.MessageType.Photo:
                        HandlePhotoMessage(msg);
                        break;
                    case Telegram.Bot.Types.Enums.MessageType.Video:
                        HandleVideoMessage(msg);
                        break;
                    default:
                        if (msg.Animation != null)
                            HandleAnimationMessage(msg);
                        else
                            HandleUnknownMessage(msg);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message.ToString());
            }
        }

        private void StoreMessage(Message message)
        {

        }

        private void HandleTextMessage(Message message)
        {
            _logger.LogInformation("Message: " + message.Text);
            if (message.Text.Equals("/start", StringComparison.InvariantCultureIgnoreCase))
            {
                _telegramBotClient.SendTextMessageAsync(message.Chat.Id, Texts.WelcomeMessage);
            }
            else if (message.Text.Equals(Texts.NoThanks, StringComparison.InvariantCultureIgnoreCase) ||
                message.Text.Equals(Texts.YesPlease, StringComparison.InvariantCultureIgnoreCase))
            {
                _telegramBotClient.SendTextMessageAsync(message.Chat.Id, Texts.Thanks);

                _telegramBotClient.SendTextMessageAsync(_appConfig.Telegram.Channel, $"{message.Text} @{message.From.Username}");
            }
            else
            {
                _telegramBotClient.SendTextMessageAsync(message.Chat.Id, Texts.PleaseUploadPhoto);
            }
        }

        private void HandleIdQuestion(long chatId)
        {
            ReplyKeyboardMarkup rkm = new string[] { Texts.YesPlease, Texts.NoThanks };
            rkm.ResizeKeyboard = true;
            _telegramBotClient.SendTextMessageAsync(chatId, Texts.ThanksAndFeedback,
                replyMarkup: rkm);
        }

        private void HandlePhotoMessage(Message message)
        {
            var fileId = message.Photo.ElementAtOrDefault(0)?.FileId;
            _telegramBotClient.SendPhotoAsync(_appConfig.Telegram.Channel, fileId, $"@{message.From.Username}");
            HandleIdQuestion(message.Chat.Id);
        }

        private void HandleAnimationMessage(Message message)
        {
            var fileId = message.Animation.FileId;
            _telegramBotClient.SendAnimationAsync(_appConfig.Telegram.Channel, fileId, caption: $"@{message.From.Username}");
            HandleIdQuestion(message.Chat.Id);
        }
        private void HandleVideoMessage(Message message)
        {
            var fileId = message.Video.FileId;
            _telegramBotClient.SendVideoAsync(_appConfig.Telegram.Channel, fileId, caption: $"@{message.From.Username}");
            HandleIdQuestion(message.Chat.Id);
        }

        private void HandleUnknownMessage(Message message)
        {
            _telegramBotClient.SendTextMessageAsync(message.Chat.Id, Texts.PleaseUploadPhoto);
        }
    }
}
