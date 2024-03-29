// System
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

// Discord
using Discord;

// Mate
using Mate.Utility;
using Mate.Variables;

// CliWrap
using CliWrap;

// Begin scope
{
    // Start the stopwatch
    GlobalVariables.Bootwatch.Start();

    string[] bootParameters = args;

    if (bootParameters.Contains("-h") || bootParameters.Contains("--help")) {
        "Quick rundown of command-line switches:".WriteLine(ConsoleColor.Cyan);
        "-h | --help\t\t= the thing you are seeing right now".WriteLine(ConsoleColor.Green);
        "--noreboot\t\t= disables automatic daily rebooting".WriteLine(ConsoleColor.Green);
        "--disable-http-server\t= disable the HTTP server".WriteLine(ConsoleColor.Green);
        return;
    }

    // Splash
    "Discord bot by ".Write(ConsoleColor.DarkCyan);
    "GermanBread#9077".WriteLine(ConsoleColor.Cyan);

    string bitVersionString = Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit";
    string osVersionString = "unknown version";
    string osNameString = "unknown name";
    if (OperatingSystem.IsLinux()) {
        if (File.Exists("/proc/version")) {
            osVersionString = File.ReadAllText("/proc/version");
            osVersionString = osVersionString[14..];
            osVersionString = osVersionString[..osVersionString.IndexOf(' ')];
        }
        if (File.Exists("/etc/os-release")) {
            osNameString = File.ReadAllText("/etc/os-release");
            osNameString = Regex.Match(osNameString, "(PRETTY_NAME=\").*").Captures.First().Value[13..].TrimEnd('\"');
        }
        "Running on Linux ".Write(ConsoleColor.White);
        $"{osNameString} ".Write(ConsoleColor.Red);
        $"{bitVersionString} ".Write(ConsoleColor.White);
        $"{osVersionString}".WriteLine(ConsoleColor.Green);
    } else {
        Environment.OSVersion.WriteLine(ConsoleColor.White);
    }

    if (bootParameters.Contains("--helper-script")) {
        "Notice: Rebooting is handled by a shell script, as indicated by the ".Write(ConsoleColor.Blue);
        "--helper-script".Write(ConsoleColor.Cyan);
        " switch".WriteLine(ConsoleColor.Blue);
    }

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

    initDataDirectory();

    try {
        // Start the bot
        await GlobalVariables.DiscordBot.Start();

        if (GlobalVariables.Rebooting) "<<< Rebooting bot via script >>>".WriteLine(ConsoleColor.Green);
    }
    catch (Exception ex) {
        await Logger.Log(new LogMessage(LogSeverity.Critical, "Crash", "The bot crashed!", ex));
        Environment.Exit(1);
    }

    createLog();

    void initDataDirectory() {
        Directory.CreateDirectory(GlobalVariables.GuildProfilesPath);
        Directory.CreateDirectory(GlobalVariables.BaseLogPath);
        if (!File.Exists(GlobalVariables.TokenPath)) File.WriteAllText(GlobalVariables.TokenPath, "Token here");
    }
    void createLog() {
        string logFileDate = DateTime.UtcNow.ToShortDateString().Replace('/', '-') /* had to add this because else the log saving process crashes */;
        StreamWriter _logwriter = new(GlobalVariables.LogFilePath.Replace("%D", logFileDate), true);
        _logwriter.WriteLine($"Logfile created on {DateTime.UtcNow}");
        foreach (var element in GlobalVariables.Logs)
        {
            var _message = element.Message;
            _logwriter.WriteLine($"{DateTime.Now,-19} [{_message.Severity,8}] {_message.Source}: {_message.Message} {_message.Exception}");
        }
        _logwriter.Close(); // Write buffer to disk
        _logwriter.Dispose(); // Free up memory

        Logger.Log(new LogMessage(LogSeverity.Info, "Shutdown", "Log saved"));
    }

    Environment.Exit(GlobalVariables.Rebooting ? 2 : 0);
}