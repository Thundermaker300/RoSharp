using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp
{
    public static class RoLogger
    {
        public static void Debug(string message, ConsoleColor debugOverride = ConsoleColor.Cyan)
        {
#if DEBUG
            Console.ForegroundColor = debugOverride;
            Console.WriteLine($"[{DateTime.Now:O}] {message}");
            Console.ResetColor();
#endif
        }

        public static void Warn(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[{DateTime.Now:O}] RoSharp Warning: {message}");
            Console.ResetColor();
        }
    }
}
