using DofusRetroSniffer.Extensions;
using DofusRetroSniffer.Utils;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Serilog;

namespace DofusRetroSniffer;

public static class Program
{
    public static async Task Main()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", false)
            .Build();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(config)
            .CreateLogger();

        try
        {
            var snifferConfig = config.GetSection("Sniffer").Get<SnifferConfig>()
                ?? throw new InvalidOperationException("Invalid configuration: 'Sniffer' section is missing or malformed.");

            ServiceCollection services = new();

            services.AddSingleton(snifferConfig);
            services.AddSingleton<ISniffer, Sniffer>();
            services.AddSingleton<IPacketLogger, PacketLogger>();

            var provider = services.BuildServiceProvider();

            provider.StartSniffer();

            await Task.Delay(-1);
        }
        catch (Exception e)
        {
            Log.Fatal(e, "Fatal error.");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
