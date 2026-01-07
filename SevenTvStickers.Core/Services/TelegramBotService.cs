using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using SevenTvStickers.Host;

namespace SevenTvStickers.Core.Services
{
    public class TelegramBotService : IHostedService
    {
        private readonly ILogger<TelegramBotService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ITelegramBotClient _botClient;

        public TelegramBotService(ILogger<TelegramBotService> logger, ITelegramBotClient botClient, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _botClient = botClient;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[] { UpdateType.Message }
            };

            _botClient.StartReceiving(HandleUpdateAsync, HandlePollingErrorAsync, receiverOptions, cancellationToken);
            _logger.LogInformation("Telegram Bot started.");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Telegram Bot stopped.");
            return Task.CompletedTask;
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type != UpdateType.Message || update.Message.WebAppData == null)
                return;

            var userId = update.Message.From.Id;
            var webAppData = update.Message.WebAppData;

            try
            {
                var emoteUrls = JsonSerializer.Deserialize<string[]>(webAppData.Data);
                if (emoteUrls == null || !emoteUrls.Any())
                {
                    await botClient.SendTextMessageAsync(userId, "No emotes selected.", cancellationToken: cancellationToken);
                    return;
                }

                using (var scope = _serviceProvider.CreateScope())
                {
                    var stickerService = scope.ServiceProvider.GetRequiredService<IStickerService>();
                    await stickerService.CreateStickerPackFromUrlsAsync(userId, emoteUrls);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing sticker pack request.");
                await botClient.SendTextMessageAsync(userId, $"An error occurred: {ex.Message}", cancellationToken: cancellationToken);
            }
        }

        private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Telegram Bot polling error.");
            return Task.CompletedTask;
        }
    }
}
