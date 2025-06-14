namespace DofusRetroSniffer;

/// <summary>
/// Represents the configuration for the Dofus Retro Sniffer application.
/// </summary>
public sealed class SnifferConfig
{
    /// <summary>
    /// Gets the list of server urls to listen to.
    /// </summary>
    public required IReadOnlyList<string> Servers { get; init; }

    /// <summary>
    /// Gets the game port to listen to.
    /// </summary>
    public required ushort GamePort { get; init; }

    /// <summary>
    /// Gets the local IP address to listen to.
    /// </summary>
    public required string LocalIp { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SnifferConfig"/> class.
    /// </summary>
    public SnifferConfig()
    {

    }
}
