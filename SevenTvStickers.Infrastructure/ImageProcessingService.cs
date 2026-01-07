using Microsoft.Extensions.Logging;
using SevenTvStickers.Core.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace SevenTvStickers.Infrastructure
{
    public class ImageProcessingService : IImageProcessingService
    {
        private readonly ILogger<ImageProcessingService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public ImageProcessingService(ILogger<ImageProcessingService> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<(Stream Stream, bool IsAnimated)> ProcessEmoteAsync(string url)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var emoteData = await client.GetByteArrayAsync(url);

                var isAnimated = await IsAnimatedAsync(emoteData);

                if (isAnimated)
                {
                    using var inputStream = new MemoryStream(emoteData);
                    var outputStream = await ConvertWithFFmpegAsync(inputStream, "-i pipe:0 -vf \"scale=512:512:force_original_aspect_ratio=decrease,pad=512:512:(ow-iw)/2:(oh-ih)/2:color=0x00000000\" -c:v libvpx-vp9 -pix_fmt yuva420p -t 3 -an -f webm pipe:1");
                    return (outputStream, true);
                }
                else
                {
                    using var inputStream = new MemoryStream(emoteData);
                    var outputStream = await ConvertWithFFmpegAsync(inputStream, "-i pipe:0 -vf \"scale=512:512:force_original_aspect_ratio=decrease,pad=512:512:(ow-iw)/2:(oh-ih)/2:color=0x00000000\" -f png pipe:1");
                    return (outputStream, false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process emote from {Url}", url);
                throw;
            }
        }

        private async Task<bool> IsAnimatedAsync(byte[] data)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffprobe",
                    Arguments = "-v error -select_streams v:0 -count_frames -show_entries stream=nb_read_frames -of default=nokey=1:noprint_wrappers=1 pipe:0",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            process.Start();

            await process.StandardInput.BaseStream.WriteAsync(data, 0, data.Length);
            process.StandardInput.Close();

            var output = await process.StandardOutput.ReadToEndAsync();
            process.WaitForExit();

            return int.TryParse(output, out var frameCount) && frameCount > 1;
        }

        private async Task<Stream> ConvertWithFFmpegAsync(Stream inputStream, string arguments)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = arguments,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            process.Start();

            var outputStream = new MemoryStream();

            var inputTask = inputStream.CopyToAsync(process.StandardInput.BaseStream).ContinueWith(_ => process.StandardInput.Close());
            var outputTask = process.StandardOutput.BaseStream.CopyToAsync(outputStream);

            await Task.WhenAll(inputTask, outputTask);

            process.WaitForExit();

            outputStream.Position = 0;
            return outputStream;
        }
    }
}
