// System
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// Mate
using Mate.Core;
using Mate.Utility;
using Mate.Variables;

// Discord
using Discord;
using Discord.Commands;

namespace Mate.Extra
{
    public static class ConsoleCommands
    {
        [Summary("Shows instructions on how to use this command-line")]
        public static void Help() {
            "Help menu:".Write(ConsoleColor.Black, ConsoleColor.White);
            new string(' ', Console.WindowWidth - Console.CursorLeft).Write(background: ConsoleColor.White);
            
            "Usage:".WriteLine(ConsoleColor.Yellow);
            "BACKSPACE\t = clear input".WriteLine(ConsoleColor.Cyan);
            "UP/DOWN\t\t = scrub through history".WriteLine(ConsoleColor.Cyan);
            "ENTER\t\t = execute command".WriteLine(ConsoleColor.Cyan);

            "Handy shortcuts:".WriteLine(ConsoleColor.Yellow);
            "CONTROL + L\t = clear the console".WriteLine(ConsoleColor.Cyan);
            "CONTROL + D\t = log out (stop the console)".WriteLine(ConsoleColor.Cyan);
            "CONTROL + C\t = stop the bot, press again to kill".WriteLine(ConsoleColor.Cyan);
            
            "Available commands:".WriteLine(ConsoleColor.Yellow);
            foreach (var command in typeof(ConsoleCommands).GetMethods())
            {
                // Check if the method is a inbuilt one
                if (GlobalVariables.GenericMethodNames.Contains(command.Name)) continue;

                var commandName = command.Name;
                var summaryAttribute = (SummaryAttribute)command.GetCustomAttributes(typeof(SummaryAttribute), false).FirstOrDefault();
                var commandSummary = summaryAttribute.Text;
                var seperator = !string.IsNullOrEmpty(commandSummary) ? "Summary: " : "";
                
                commandName.WriteLine(ConsoleColor.Cyan);
                seperator.Write(ConsoleColor.Blue);
                commandSummary.Write();
                Console.WriteLine();
            }
        }
        [Summary("Stops the bot")]
        public static void Stop() {
            "Stopping bot".WriteLine(ConsoleColor.Yellow);
            _ = GlobalVariables.DiscordBot.Quit();
        }
        [Summary("Reboots the bot")]
        public static void Reboot() {
            "Rebooting bot".WriteLine(ConsoleColor.Yellow);
            _ = GlobalVariables.DiscordBot.Reboot();
        }
        [Summary("Stops the bot in an instant, bypasses every measure to perform a clean shutdown.")]
        public static void Abort() {
            "Killing bot".WriteLine(ConsoleColor.Yellow);
            _ = GlobalVariables.DiscordBot.Abort();
        }
        [Summary("Display the uptime")]
        public static void Uptime() {
            "Bot uptime".WriteLine(ConsoleColor.Yellow);
            "Up: ".Write(ConsoleColor.Cyan);
            Math.Truncate(GlobalVariables.Bootwatch.Elapsed.TotalDays).Write(ConsoleColor.Blue);
            " days, ".Write(ConsoleColor.Blue);
            GlobalVariables.Bootwatch.Elapsed.Hours.Write(ConsoleColor.Blue);
            " hours, ".Write(ConsoleColor.Blue);
            GlobalVariables.Bootwatch.Elapsed.Minutes.Write(ConsoleColor.Blue);
            " minutes, ".Write(ConsoleColor.Blue);
            GlobalVariables.Bootwatch.Elapsed.Seconds.Write(ConsoleColor.Blue);
            " seconds.".WriteLine(ConsoleColor.Blue);
        }
        [Summary("Clear the console")]
        public static void Clear() {
            "Clearing console".WriteLine(ConsoleColor.Yellow);
            Console.Clear();
        }
        [Summary("Stops this console session and allows logs to be shown again")]
        public static void Logout(CommandConsole console) {
            console.Close();
        }
    }
}