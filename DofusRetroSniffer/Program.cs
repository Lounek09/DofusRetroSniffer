using DofusRetroSniffer.Utils;

using System.Reflection;

namespace DofusRetroSniffer
{
    public static class Program
    {
        public static readonly string APP_PATH = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

        public static Sniffer? Sniffer { get; private set; }

        public static async Task Main()
        {
            Config config = Config.Load();

            try 
            {
                Sniffer = new(config);
            }
            catch (NullReferenceException e)
            {
                Logger.Crit(e);
            }

            await Task.Delay(-1);
        }
    }
}
