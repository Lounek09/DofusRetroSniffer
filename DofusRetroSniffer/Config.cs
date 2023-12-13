using DofusRetroSniffer.Utils;

using System.Text.Json;

namespace DofusRetroSniffer;

public class Config
{
    public static readonly string PATH = Path.Join(Program.APP_PATH, "config.json");

    public List<string> Servers { get; set; }
    public ushort GamePort { get; set; }
    public string LocalIp { get; set; }

    public Config()
    {
        Servers = [];
        LocalIp = string.Empty;
    }

    public static Config Load()
    {
        var json = File.ReadAllText(PATH);
        return JsonSerializer.Deserialize<Config>(json)!;
    }
}
