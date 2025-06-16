using System.Runtime.InteropServices;
using System.Text;

namespace DofusRetroSniffer.Utils;

/// <summary>
/// Provides logging functionality for network packets with color formatting.
/// </summary>
public static partial class PacketLogger
{
    private const string AnsiColorIncoming = "\x1b[34m"; // Blue
    private const string AnsiColorOutgoing = "\x1b[32m"; // Green
    private const string AnsiReset = "\x1b[m";

    private const string ArrowIncoming = "<--";
    private const string ArrowOutgoing = "-->";

    /// <summary>
    /// Writes a formatted log entry for a network packet to the console.
    /// </summary>
    /// <param name="message">The packet message to log.</param>
    /// <param name="isIncoming">Whether the packet is incoming or outgoing.</param>
    /// <param name="date">The date and time when the packet was captured.</param>
    public static void Write(ReadOnlySpan<char> message, bool isIncoming, DateTime date)
    {
        var dateFormat = date.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var arrow = isIncoming ? ArrowIncoming : ArrowOutgoing;
        var ansiColor = isIncoming ? AnsiColorIncoming : AnsiColorOutgoing;

        StringBuilder logBuilder = new(dateFormat.Length + 1 + arrow.Length + 1 + ansiColor.Length + message.Length + AnsiReset.Length);

        logBuilder.Append(dateFormat);
        logBuilder.Append(' ').Append(arrow).Append(' ');
        logBuilder.Append(ansiColor).Append(message).Append(AnsiReset);

        Console.WriteLine(logBuilder);
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
