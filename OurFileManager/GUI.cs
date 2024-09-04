using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurFileManager
{
    internal static class GUI
    {
        public static void DrawHorizontalLine()
        {
            string buf = "";
            buf += "│";
            for (int i = 0; i < Config.WindowWidth; i++)
            {
                buf += "─";
            }
            buf += "│";
            Console.WriteLine(buf);
        }
    }
}
