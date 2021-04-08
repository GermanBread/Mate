// System
using System;
using System.Threading.Tasks;

// Discord
using Discord;

// Mate
using Mate.Variables;

namespace Mate.Utility
{
    public static class Logger
    {
        private static bool isPrintingToConsole = false;
        
        /// <summary>
        /// Sets a urgency threshold i.e. 1 = Only messages of urgency CRITICAL and ERROR, 0 = Only messages of urgency CRITICAL
        /// </summary>
        public static int MinimumPriority = 99;
        /// <summary>
        /// Logs your message. Attributes defined as a seperate parameter.
        /// </summary>
        public static Task Log(LogMessage log, LogAttributes attrs) {
            return baseLog(log, attrs);
        }
        /// <summary>
        /// Logs your message. Supports attributes.
        /// </summary>
        public static Task Log(ExtendedLogMessage log) {
            return baseLog(log.Message, log.Attributes);
        }
        /// <summary>
        /// Logs your message. The simple one.
        /// </summary>
        public static Task Log(LogMessage log) {
            return baseLog(log, null);
        }
        /// <summary>
        /// Logs your message. The simple one. This one doesn't store the log.
        /// </summary>
        public static Task SilentLog(LogMessage log) {
            return baseLog(log, new LogAttributes { IsSilent = true });
        }
        
        private static Task baseLog(LogMessage log, LogAttributes? attributes = null) {            
            // Add this log to the global array, if allowed
            if (!attributes.GetValueOrDefault().IsSilent) 
                GlobalVariables.Logs.Add(new ExtendedLogMessage(log, attributes));

            // Exit if the log is supressed
            if (attributes.GetValueOrDefault().IsSupressed | (int)log.Severity > MinimumPriority)
                return Task.CompletedTask;
            
            // Determine the color to use
            ConsoleColor timeColor = ConsoleColor.Blue;
            ConsoleColor severifyColor = ConsoleColor.Cyan;
            ConsoleColor sourceColor = ConsoleColor.Green;
            ConsoleColor messageColor = ConsoleColor.White;
            ConsoleColor exceptionColor = ConsoleColor.DarkRed;
            switch (log.Severity) {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    timeColor = ConsoleColor.Cyan;
                    severifyColor = ConsoleColor.Green;
                    sourceColor = ConsoleColor.Yellow;
                    messageColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    messageColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    timeColor = ConsoleColor.DarkBlue;
                    severifyColor = ConsoleColor.DarkCyan;
                    sourceColor = ConsoleColor.DarkGreen;
                    messageColor = ConsoleColor.DarkGray;
                    break;
            }
            // Log the message
            string buffer = new string(' ', 4 - (int)Math.Ceiling(log.Severity.ToString().Length / 2f));
            string buffer2 = new string(' ', 4 - (int)Math.Floor(log.Severity.ToString().Length / 2f));

            // Wait until the last print is done
            while (isPrintingToConsole);
            // Set a bool to prevent this method from being interrupted
            isPrintingToConsole = true;

            // Time
            DateTime.UtcNow.ToString("HH:mm:ss").Write(timeColor);
            // Buffer
            " | ".Write();
            buffer.Write();
            // Severity
            log.Severity.Write(severifyColor);
            // Buffer2
            buffer2.Write();
            if (attributes.GetValueOrDefault().IsSilent) "silent".Write(ConsoleColor.Gray);
            " | ".Write();
            // Source
            log.Source.Write(sourceColor);
            // Buffer
            " > ".Write(ConsoleColor.Magenta);
            // Message
            log.Message.WriteLine(messageColor);
            // Exception
            if (log.Exception != null) log.Exception.WriteLine(exceptionColor);

            isPrintingToConsole = false;
            return Task.CompletedTask;
        }
    }
}