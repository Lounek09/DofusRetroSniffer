using DofusRetroSniffer.Utils;

namespace DofusRetroSniffer;

public static class Program
{
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
