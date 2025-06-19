using DofusRetroSniffer.Network.Packets;

using Serilog;

using System.Runtime.InteropServices;

namespace DofusRetroSniffer.Utils;

/// <summary>
/// Provides methods for logging network packets to the console with color coding.
/// </summary>
public interface IPacketLogger
{
    /// <summary>
    /// Writes a formatted log entry for a network packet.
    /// </summary>
    /// <param name="rawDofusPacket">The raw Dofus packet to log.</param>
    void Write(RawDofusPacket rawDofusPacket);
}

public partial class PacketLogger : IPacketLogger
{
    public const string Incoming = "INCOMING";
    public const string Outgoing = "OUTGOING";

    private const string AnsiColorIncoming = "\x1b[34m"; // Blue
    private const string AnsiColorOutgoing = "\x1b[32m"; // Green
    private const string AnsiReset = "\x1b[m";

    private const string ArrowIncoming = "<---";
    private const string ArrowOutgoing = "--->";

    private readonly ILogger _logger;

    public PacketLogger()
    {
        _logger = Log.ForContext<PacketLogger>();
    }

    public void Write(RawDofusPacket rawDofusPacket)
    {
        var isIncoming = rawDofusPacket.IsIncoming;
        var direction = isIncoming ? ArrowIncoming : ArrowOutgoing;
        var ansiColor = isIncoming ? AnsiColorIncoming : AnsiColorOutgoing;

        _logger
            .ForContext("PacketTimestamp", rawDofusPacket.DateCaptured)
            .ForContext("Direction", direction)
            .Information("{ColorStart}{RawData}{ColorEnd}", ansiColor, rawDofusPacket.RawData, AnsiReset);
    }

    #region Windows Console Ansi Support

    private const int StdOutputHandle = -11;
    private const uint EnableVirtualTerminalProcessing = 0x0004;

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial IntPtr GetStdHandle(int nStdHandle);

    static PacketLogger()
    {
        if (OperatingSystem.IsWindows())
        {
            var stdOutHandle = GetStdHandle(StdOutputHandle);
            if (GetConsoleMode(stdOutHandle, out var consoleMode))
            {
                SetConsoleMode(stdOutHandle, consoleMode | EnableVirtualTerminalProcessing);
            }
        }
    }

    #endregion
}
