using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using SevenTvStickers.Host;

namespace SevenTvStickers.Core.Services
{
    public class StickerService : IStickerService
    {
        private readonly ILogger<StickerService> _logger;
        private readonly IImageProcessingService _imageProcessingService;
        private readonly ITelegramBotClient _botClient;

        public StickerService(ILogger<StickerService> logger, IImageProcessingService imageProcessingService, ITelegramBotClient botClient)
        {
            _logger = logger;
            _imageProcessingService = imageProcessingService;
            _botClient = botClient;
        }

        public async Task CreateStickerPackFromUrlsAsync(long userId, IEnumerable<string> emoteUrls)
        {
            var packName = $"emotes_{Guid.NewGuid().ToString().Substring(0, 8)}_by_7tv_bot";
            var packTitle = "7TV Emotes Pack";

            try
            {
                var processedEmotes = new List<(Stream Stream, bool IsAnimated)>();
                foreach (var url in emoteUrls)
                {
                    processedEmotes.Add(await _imageProcessingService.ProcessEmoteAsync(url));
                }

                if (processedEmotes.Select(e => e.IsAnimated).Distinct().Count() > 1)
                {
                    await _botClient.SendTextMessageAsync(userId, "Mixed sticker types are not allowed. Please select either all static or all animated emotes.");
                    return;
                }

                var firstEmote = processedEmotes.First();
                var isAnimated = firstEmote.IsAnimated;

                if (isAnimated)
                {
                    await _botClient.CreateNewVideoStickerSetAsync(userId, packName, packTitle, new InputMedia(firstEmote.Stream, "first.webm"), new[] { "🚀" });
                }
                else
                {
                    await _botClient.CreateNewStickerSetAsync(userId, packName, packTitle, new InputMedia(firstEmote.Stream, "first.png"), "🚀");
                }

                foreach (var emote in processedEmotes.Skip(1))
                {
                    if (isAnimated)
                    {
                        await _botClient.AddVideoStickerToSetAsync(userId, packName, new InputMedia(emote.Stream, "sticker.webm"), new[] { "🚀" });
                    }
                    else
                    {
                        await _botClient.AddStickerToSetAsync(userId, packName, new InputMedia(emote.Stream, "sticker.png"), "🚀");
                    }
                }

                await _botClient.SendTextMessageAsync(userId, $"Sticker pack created: https://t.me/addstickers/{packName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating sticker pack.");
                await _botClient.SendTextMessageAsync(userId, $"Failed to create sticker pack: {ex.Message}");
            }
        }
    }
}
