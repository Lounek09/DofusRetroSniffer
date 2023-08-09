using System.Drawing;

namespace DofusRetroSniffer.Utils
{
    public static class Logger
    {
        private enum Level
        {
            CRIT,
            ERROR,
            INFO,
            INCOMING,
            OUTGOING
        }

        private static void Log(string message, Level level, DateTime? date = null)
        {
            date ??= DateTime.Now;

            string dateFormat = $"[{date:HH:mm:ss:ffff}]".SetColor(Color.Goldenrod);
            string levelFormat = $"[{level,5}]";

            switch (level)
            {
                case Level.CRIT:
                    levelFormat = levelFormat.SetStyle(ConsoleFormat.Style.BOLD).SetBgColor(Color.LightGoldenrodYellow).SetColor(Color.Red);
                    break;
                case Level.ERROR:
                    levelFormat = levelFormat.SetColor(Color.IndianRed);
                    break;
                case Level.INFO:
                    levelFormat = levelFormat.SetColor(Color.SteelBlue);
                    break;
                case Level.INCOMING:
                    levelFormat = " <--".SetColor(Color.White);
                    message = message.SetColor(Color.LightSlateGray);
                    break;
                case Level.OUTGOING:
                    levelFormat = " -->".SetColor(Color.White);
                    message = message.SetColor(Color.LimeGreen);
                    break;
            }

            Console.WriteLine(dateFormat + levelFormat + " " + message);
        }


        public static void Crit(string message)
        {
            Log(message, Level.CRIT);
        }

        public static void Crit(string message, params object[] args)
        {
            Log(string.Format(message, args), Level.CRIT);
        }

        public static void Crit(Exception exception)
        {
            Log($"{exception.Message}\n{exception.StackTrace}", Level.CRIT);
        }

        public static void Error(string message)
        {
            Log(message, Level.ERROR);
        }

        public static void Error(string message, params object[] args)
        {
            Log(string.Format(message, args), Level.ERROR);
        }

        public static void Error(Exception exception)
        {
            Log($"{exception.Message}\n{exception.StackTrace}", Level.ERROR);
        }

        public static void Info(string message)
        {
            Log(message, Level.INFO);
        }

        public static void Info(string message, params object[] args)
        {
            Log(string.Format(message, args), Level.INFO);
        }

        public static void Packet(string message, bool isIncoming, DateTime date)
        {
            Log(message, isIncoming ? Level.INCOMING : Level.OUTGOING, date);
        }
    }
}
