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

#if DEBUG
        internal static bool debugLogs = true;
#else
        internal static bool debugLogs = false;
#endif


        /// <summary>
        /// Sends a debug message if the assembly was compiled in DEBUG configuration. Does nothing if the assembly was compiled otherwise.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="debugOverride">Color override.</param>
        public static void Debug(string message, ConsoleColor debugOverride = ConsoleColor.Cyan)
        {
            if (debugLogs && !disableLogging)
            {
                Console.ForegroundColor = debugOverride;
                Console.WriteLine(string.IsNullOrWhiteSpace(message) ? null : $"[ROSHARP] [{DateTime.Now:O}] {message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Forcefully disables all log messages from RoSharp, including warnings.
        /// </summary>
        public static void DisableAllLogging() => disableLogging = true;

        /// <summary>
        /// Toggles debug logs.
        /// </summary>
        public static void SetDebugLogs(bool value) => debugLogs = value;

        /// <summary>
        /// Outputs a user-friendly warning message in the console.
        /// </summary>
        /// <param name="message">The warning text.</param>
        public static void Warn(string message)
        {
            if (!disableLogging)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[ROSHARP] [{DateTime.Now:O}]: {message}");
                Console.ResetColor();
            }
        }
    }
}
