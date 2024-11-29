using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp
{
    /// <summary>
    /// Static class used for debugging and showing warnings.
    /// </summary>
    public static class RoLogger
    {
        private static bool disableLogging = false;

        /// <summary>
        /// Sends a debug message if the assembly was compiled in DEBUG configuration. Does nothing if the assembly was compiled otherwise.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="debugOverride">Color override.</param>
        public static void Debug(string message, ConsoleColor debugOverride = ConsoleColor.Cyan)
        {
#if DEBUG
            if (!disableLogging)
            {
                Console.ForegroundColor = debugOverride;
                Console.WriteLine($"[{DateTime.Now:O}] {message}");
                Console.ResetColor();
            }
#endif
        }

        /// <summary>
        /// Forcefully disables all log messages from RoSharp, including warnings.
        /// </summary>
        public static void DisableAllLogging() => disableLogging = true;

        /// <summary>
        /// Outputs a user-friendly warning message in the console.
        /// </summary>
        /// <param name="message">The warning text.</param>
        public static void Warn(string message)
        {
            if (!disableLogging)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[{DateTime.Now:O}] RoSharp Warning: {message}");
                Console.ResetColor();
            }
        }
    }
}
