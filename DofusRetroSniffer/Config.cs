using DofusRetroSniffer.Utils;

using System.Text.Json;

namespace DofusRetroSniffer
{
    public class Config
    {
        public static readonly string PATH = $"{Program.APP_PATH}/config.json";

        public List<string> Servers { get; set; }
        public ushort GamePort { get; set; }
        public string LocalIp { get; set; }

        public Config()
        {
            Servers = new();
            LocalIp = string.Empty;
        }

        public static Config Load()
        {
            if (!File.Exists(PATH))
            {
                Logger.Crit("Config file not found at '{0}'", PATH);
                Console.ReadKey();
                Environment.Exit(0);
            }

            string json = File.ReadAllText(PATH);
            return JsonSerializer.Deserialize<Config>(json)!;
        }
    }
}
