using System.IO;
using System.Threading.Tasks;

namespace SevenTvStickers.Core.Services
{
    public interface IImageProcessingService
    {
        Task<(Stream Stream, bool IsAnimated)> ProcessEmoteAsync(string url);
    }
}
