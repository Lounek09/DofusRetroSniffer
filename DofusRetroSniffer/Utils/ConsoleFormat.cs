﻿using System.Drawing;
using System.Runtime.InteropServices;

namespace DofusRetroSniffer.Utils;

public static partial class ConsoleFormat
{
    private const int STD_OUTPUT_HANDLE = -11;
    private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial IntPtr GetStdHandle(int nStdHandle);

    static ConsoleFormat()
    {
        if (OperatingSystem.IsWindows())
        {
            var iStdOut = GetStdHandle(STD_OUTPUT_HANDLE);
            GetConsoleMode(iStdOut, out var outConsoleMode);
            SetConsoleMode(iStdOut, outConsoleMode | ENABLE_VIRTUAL_TERMINAL_PROCESSING);
        }
    }

    public enum Style : int
    {
        END = 0,
        BOLD = 1,
        FAINT = 2,
        ITALIC = 3,
        UNDERLINED = 4,
        INVERSE = 7,
        STRIKETHROUGH = 9,
        FOREGROUND = 38,
        BACKGROUND = 48
    }

    private static string GetEscapeCodes(Style style, Color? color = null)
    {
        switch (style)
        {
            case Style.END:
            case Style.BOLD:
            case Style.FAINT:
            case Style.ITALIC:
            case Style.UNDERLINED:
            case Style.INVERSE:
            case Style.STRIKETHROUGH:
                return $"\x1b[{(int)style}m";
            case Style.FOREGROUND:
            case Style.BACKGROUND:
                if (color is not null)
                    return $"\x1b[{(int)style};2;{color.Value.R};{color.Value.G};{color.Value.B}m";
                break;
        }

        return "";
    }

    private static string Format(this string text, Style style, Color? color = null)
    {
        var start = GetEscapeCodes(style, color);
        var end = GetEscapeCodes(Style.END);

        return start + text + (string.IsNullOrEmpty(start) || text.EndsWith(end) ? "" : end);
    }

    public static string SetColor(this string text, Color color)
    {
        return text.Format(Style.FOREGROUND, color);
    }

    public static string SetBgColor(this string text, Color color)
    {
        return text.Format(Style.BACKGROUND, color);
    }

    public static string SetStyle(this string text, Style style)
    {
        return text.Format(style);
    }
}
