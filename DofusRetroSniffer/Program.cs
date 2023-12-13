using DofusRetroSniffer.Utils;

using System.Reflection;

namespace DofusRetroSniffer;

public static class Program
{
    public static readonly string APP_PATH = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

    public static async Task Main()
    {
        try 
        {
            var config = Config.Load();

            var sniffer = new Sniffer(config);
            sniffer.Start();
        }
        catch (Exception e)
        {
            Logger.Crit(e);
        }

        await Task.Delay(-1);
    }
}
