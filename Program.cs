// System
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

// Discord
using Discord;
using Discord.Commands;
using Discord.WebSocket;

// Mate
using Mate.Utility;
using Mate.Variables;

// Start the stopwatch
GlobalVariables.Bootwatch.Start();

string[] bootParameters = Environment.GetCommandLineArgs();

// Splash
"Discord bot-init by ".Write(ConsoleColor.DarkCyan);
"GermanBread#9077".WriteLine(ConsoleColor.Cyan);

if (bootParameters.Contains("m")) "<<< Booting maintenance mode >>>".WriteLine(ConsoleColor.Yellow);
else "<<< Starting bot >>>".WriteLine(ConsoleColor.Green);

if (bootParameters.Contains("m")) {
    // Pause the stopwatch
    GlobalVariables.Bootwatch.Stop();

    "Please enter a valid guild ID".WriteLine();
    ulong value;
    while (!ulong.TryParse(Console.ReadLine(), System.Globalization.NumberStyles.Number, null, out value)) {
        GlobalVariables.MaintenanceModeGuild = value;
    }
    GlobalVariables.MaintenanceModeGuild = value;
    $"Booting with guild ID \"{GlobalVariables.MaintenanceModeGuild}\"".WriteLine();

    // Resume the stopwatch
    GlobalVariables.Bootwatch.Start();
}

InitDataDirectory();

try {
    // Start the bot
    await GlobalVariables.DiscordBot.Start();
}
catch (Exception ex) {
    await Logger.Log(new LogMessage(LogSeverity.Critical, "Crash", "The bot crashed!", ex));
}

CreateLog();

// Methods
void InitDataDirectory() {
    Directory.CreateDirectory(GlobalVariables.GuildProfilesPath);
    Directory.CreateDirectory(GlobalVariables.BaseLogPath);
    if (!File.Exists(GlobalVariables.TokenPath)) File.WriteAllText(GlobalVariables.TokenPath,"Token here");
}

void CreateLog() {
    string logFileDate = DateTime.UtcNow.ToShortDateString().Replace('/', '-') /* had to add this because else the log saving process crashes */;
    StreamWriter _logwriter = new StreamWriter(GlobalVariables.LogFilePath.Replace("%D", logFileDate), true);
    _logwriter.WriteLine($"Logfile created on {DateTime.UtcNow}");
    foreach (var element in GlobalVariables.Logs)
    {
        var _message = element.Message;
        _logwriter.WriteLine($"{DateTime.Now,-19} [{_message.Severity,8}] {_message.Source}: {_message.Message} {_message.Exception}");
    }
    _logwriter.Close(); // Write buffer to disk
    _logwriter.Dispose(); // Free up memory

    Logger.Log(new LogMessage(LogSeverity.Info, "Shutdown", $"Log saved"));
}