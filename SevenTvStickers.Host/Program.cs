using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SevenTvStickers.Core.Services;
using System.IO;
using System.Threading.Tasks;

namespace SevenTvStickers.Host
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHttpClient();

                    // Register TelegramBotClient as a singleton
                    services.AddSingleton<ITelegramBotClient>(sp =>
                    {
                        var options = sp.GetRequiredService<IOptions<TelegramBotOptions>>().Value;
                        return new TelegramBotClient(options.Token);
                    });

                    services.AddHostedService<TelegramBotService>();
                    services.AddTransient<IStickerService, StickerService>();
                    services.AddTransient<IImageProcessingService, ImageProcessingService>();

                    // Configure Telegram Bot options
                    services.Configure<TelegramBotOptions>(hostContext.Configuration.GetSection("TelegramBot"));
                });
    }

    public class TelegramBotOptions
    {
        public string Token { get; set; }
    }
}
