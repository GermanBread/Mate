using System;
// System
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

// Discord
using Discord;

// Mate
using Mate.Core;
using Mate.Utility;

namespace Mate.Variables {
    public static class GlobalVariables
    {
        /// <summary>
        /// All past occurances or events that have happened.
        /// </summary>
        public static List<ExtendedLogMessage> Logs { get; } = new List<ExtendedLogMessage>();
        /// <summary>
        /// A collection of generic method names. Can be used as a blacklist when using reflection.
        /// </summary>
        /// <value></value>
        public static List<string> GenericMethodNames { get => new() {
            "ToString",
            "Equals",
            "GetHashCode",
            "GetType"
        }; }
        /// <summary>
        /// The bot itself
        /// </summary>
        public static Bot DiscordBot { get; } = new Bot();
        /// <summary>
        /// This stopwatch is started as soon as the Main() method is called.
        /// </summary>
        public static Stopwatch Bootwatch { get; } = new Stopwatch();
        /// <summary>
        /// The base directory for everything
        /// </summary>
        public static string TrunkPath { get; } = "data";
        /// <summary>
        /// Base directory where the webserver accesses it's HTML files from
        /// </summary>
        /// <value></value>
        public static string HtmlPath { get; } = "html";
        /// <summary>
        /// Directory where image assets reside in (does not have anything to do with the webserver)
        /// </summary>
        public static string AssetPath { get; } = "assets";
        /// <summary>
        /// Base direcory where all configuraion files reside in
        /// </summary>
        public static string BaseConfigPath { get; } = Path.Combine(TrunkPath, "config");
        /// <summary>
        /// The base directory where all log files get stored
        /// </summary>
        public static string BaseLogPath { get; } = Path.Combine(TrunkPath, "logs");
        /// <summary>
        /// The path to the bot's login token.
        /// </summary>
        public static string TokenPath { get; } = Path.Join(BaseConfigPath, "token.txt");
        /// <summary>
        /// The path for the logfiles. %D gets substituted with the date when it was written to disk.
        /// </summary>
        public static string LogFilePath { get; } = Path.Combine(BaseLogPath, "%D_log.txt");
        /// <summary>
        /// The path where the guild profiles get stored.
        /// </summary>
        public static string GuildProfilesPath { get; } = Path.Combine(BaseConfigPath, "profiles");
        /// <summary>
        /// Current bot uptime
        /// </summary>
        /// <value>Uptime in milliseconds</value>
        public static long Uptime { get { return Bootwatch.ElapsedMilliseconds; } }
        /// <summary>
        /// Time it took for the bot to start
        /// </summary>
        /// <value>Time in milliseconds</value>
        public static long StartupTime { get; set; }
        /// <summary>
        /// Time it took to shut down. This value is unset while the bot is running. During shutdown it is set to the current uptime and is corrected afterwards.
        /// </summary>
        /// <value>Time in milliseconds</value>
        public static long ShutdownTime { get; set; }
        /// <summary>
        /// If not null, will restrict command usage to one guild
        /// </summary>
        /// <value></value>
        public static ulong? MaintenanceModeGuild { get; set; }
        /// <summary>
        /// Whether or not the bot is currently shutting down.
        /// </summary>
        /// <value>True WHILE the bot is shutting down (also set to true while the bot is in the shutdown phase during reboot). False after shutdown succeeded. 
        /// If the bot wasn't shut down properly this value indicates a "dirty" shutdown (by having the value true)</value>
        public static bool ShuttingDown { get; set; } = false;
        /// <summary>
        /// Whether or not the bot is set to reboot.
        /// </summary>
        public static bool Rebooting { get; set; } = false;
        /// <summary>
        /// Whether or not the bot has started up. This variable is used to prevent the "ready" event from triggering the start() method twice.
        /// </summary>
        /// <value>True if startup completed. False if otherwise.</value>
        public static bool StartupComplete { get; set; } = false;
    }
}