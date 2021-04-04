using MemeBot.Bots;
using MemeBot.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using Telegram.Bot;

namespace MemeBot
{
    public class MemeBotApp
    {
        private readonly ILogger _logger;
        private MemeReceiver memeReceiver;
        private readonly IConfiguration _config;
        private readonly AppConfig _appConfig;
        private TelegramBotClient telegramBotClient;
        public MemeBotApp(ILogger<MemeBotApp> logger, IConfiguration configuration)
        {
            _logger = logger;
            _config = configuration;
            _appConfig = _config.Get<AppConfig>();

        }
        internal void Run()
        {
            _logger.LogInformation("MemeReceiver Started at {dateTime}", DateTime.Now);
            telegramBotClient = new TelegramBotClient(_appConfig.Telegram.BotToken);

            telegramBotClient.SendTextMessageAsync(_appConfig.Telegram.Channel, "Memebot Started at " + DateTime.Now);

            memeReceiver = new MemeReceiver(telegramBotClient, _logger, _appConfig);

            memeReceiver.Start();

        }
        public void Stop()
        {
            telegramBotClient.SendTextMessageAsync(_appConfig.Telegram.Channel, "Memebot Stoped at " + DateTime.Now);
            _logger.LogInformation("MemeReceiver Ended at {dateTime}", DateTime.Now);
            memeReceiver.Stop();
        }

    }
}
