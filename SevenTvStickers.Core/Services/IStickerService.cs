using System.Collections.Generic;
using System.Threading.Tasks;

namespace SevenTvStickers.Core.Services
{
    public interface IStickerService
    {
        Task CreateStickerPackFromUrlsAsync(long userId, IEnumerable<string> emoteUrls);
    }
}
