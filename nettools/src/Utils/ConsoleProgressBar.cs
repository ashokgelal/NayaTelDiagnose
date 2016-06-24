using System;

namespace nettools.Utils
{

    internal static class ConsoleProgressBar
    {

        public static void DrawProgressBar(long current, long total, int barSize, double speed, double time, char progressChar = '|', char beginChar = '[', char endChar = ']')
        {
            Console.CursorVisible = false;

            int left = Console.CursorLeft;
            decimal perc = current / total;
            int chars = (int)Math.Floor(perc / (1 / barSize)) + 2;
            string p1 = string.Empty, p2 = string.Empty;

            for (int i = 0; i < chars; i++)
                p1 += progressChar;

            for (int i = 0; i < barSize - chars; i++)
                p2 += progressChar;

            Console.WriteLine(beginChar + " ");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(p1 + "=>");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write(p2);

            Console.WriteLine(" " + endChar);

            Console.ResetColor();
            Console.Write("\t{0}% @ {1} kb/s\remaining {0:hh\\:mm\\:ss}", (perc * 100).ToString("N2"), (speed / 1024).ToString("N2"), new TimeSpan((long)time));
            Console.CursorLeft = left;
        }

    }

}
