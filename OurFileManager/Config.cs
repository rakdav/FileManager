using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurFileManager
{
    internal static class Config
    {
        public static ConsoleColor BackgroundColor = ConsoleColor.Black;
        public static ConsoleColor ForegroundColor = ConsoleColor.White;
        public static string SectionPath = @".";
        public static int StartupWindowWidth = 120; 
        public static int StartupWindowHeight = 30;

        public static int WindowWidth = StartupWindowWidth - 3;
        public static int QuarterWindowWidth = WindowWidth / 4;
        public static int HalfWindowWidth = WindowWidth / 2;
        public static int WindowHeight = StartupWindowHeight - 3;
        public static int FilesCountOneSection = StartupWindowHeight - 11;
        public static string ProgramLocation = Directory.GetCurrentDirectory();

        public static void ChangeColorScheme(ConsoleColor[] colors)
        {
            BackgroundColor = colors[0];
            ForegroundColor = colors[1];
        }

        public static void ChangeColorScheme(string str)
        {
            if (str.Length > 150)
                return;

            string[] colors;
            try
            {
                colors = str.Split(",.!-/?\\_ ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            }
            catch (Exception) { throw; }

            Enum.TryParse(colors[0], out BackgroundColor);
            Enum.TryParse(colors[1], out ForegroundColor);
        }

        public static void ChangeResolution(int w, int h)
        {
            try
            {
                StartupWindowWidth = w;
                StartupWindowHeight = h;
                WindowWidth = StartupWindowWidth - 3;
                QuarterWindowWidth = WindowWidth / 4;
                HalfWindowWidth = WindowWidth / 2;
                WindowHeight = StartupWindowHeight - 3;
                FilesCountOneSection = StartupWindowHeight - 11;
            }
            catch (Exception) { }
        }

        public static void SaveConfig()
        {
            try
            {
                using (var sw = File.CreateText($"{ProgramLocation}\\config.txt"))
                {
                    sw.Write(
                        BackgroundColor + " " + ForegroundColor
                        );
                    sw.Write(StartupWindowWidth + " " + StartupWindowHeight);
                }
            }
            catch (Exception) { }
        }
    }
}
