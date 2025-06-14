using Microsoft.Extensions.Configuration;

using Serilog;

using SharpPcap.LibPcap;

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

            var device = FindDevice(snifferConfig.LocalIp);

            Sniffer sniffer = new(snifferConfig, device);
            sniffer.Listen();

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

    /// <summary>
    /// Finds a network device associated with the specified IP address.
    /// </summary>
    /// <param name="ip">The IP address to search for.</param>
    /// <returns>The <see cref="LibPcapLiveDevice"/> instance corresponding to the specified IP address.</returns>
    /// <exception cref="NullReferenceException">Thrown if no device is found that matches the specified IP address.</exception>
    private static LibPcapLiveDevice FindDevice(string ip)
    {
        foreach (var device in LibPcapLiveDeviceList.Instance)
        {
            foreach (var address in device.Addresses)
            {
                if (ip.Equals(address.Addr?.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return device;
                }
            }
        }

        throw new NullReferenceException($"Unable to find the device with IP '{ip}' to listen to");
    }
}
