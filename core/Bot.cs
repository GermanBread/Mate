// System
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

// Discord
using Discord;
using Discord.Commands;
using Discord.WebSocket;

// Mate
using Mate.Extra;
using Mate.Modules;
using Mate.Utility;
using Mate.Variables;

namespace Mate.Core
{
    public sealed partial class Bot : BotBase
    {
        // Startup:
        // Init = Called after the core bot has initialized
        // Run = Called after the client fires the "ready" event
        // Shutdown:
        // Stop = Called before guild profiles get saved
        // Note:
        // Any unhandled exception will terminate the execution of the method

        private Webserver server;
        private CommandConsole console;
        private CancellationTokenSource statusUpdater;
        private CancellationToken statusUpdaterToken;

        void Init() {
            Logger.Log(new LogMessage(LogSeverity.Info, "Startup", "Initializing commands"));
            InitCommands().Wait();
            Logger.Log(new LogMessage(LogSeverity.Info, "Startup", "Starting Webserver"));
            
            server = new Webserver(new string[] {
                "localhost"
            }, 8080);
            // *ahem* Because this was blocking the main thread from starting, the GC should take care of that eventually...
            Task.Run(() => server.StartAsync());

            Logger.Log(new LogMessage(LogSeverity.Info, "Startup", "Subscribing events"));
            Client.JoinedGuild += HandleGuildJoin;
            Client.MessageReceived += HandleCommands;
        }
        void Run() {
            // Set a custom status
            Logger.Log(new LogMessage(LogSeverity.Info, "Startup", "Setting status and activity"));
            Client.SetStatusAsync(GlobalVariables.MaintenanceModeGuild.HasValue ? UserStatus.Invisible : UserStatus.AFK);
            statusUpdater = new CancellationTokenSource();
            statusUpdaterToken = statusUpdater.Token;
            _ = UpdateStatus();

            // This blocked startup, so I decided to create a task for it. Horrible code if this were written in C/C++
            console = new CommandConsole(typeof(ConsoleCommands));
            Task.Run(() => {
                while (true)
                {
                    // Check if a console session already exists
                    if (!console.isAlive) {
                        ConsoleKeyInfo key = Console.ReadKey();
                        // Check for the key combination
                        if (key.Modifiers == ConsoleModifiers.Control & key.Key == ConsoleKey.E)
                            // Run asyncronously
                            _ = console.Start();
                    }
                }
            });

            // Start a task that restarts the bot (prevents OOM)
            Task.Run(() => {
                Thread.Sleep(TimeSpan.FromDays(1));
                _ = Reboot();
            });
        }
        void Stop() {
            Logger.Log(new LogMessage(LogSeverity.Info, "Shutdown", "Unsubscribing events"));
            Client.JoinedGuild -= HandleGuildJoin;
            Client.MessageReceived -= HandleCommands;
            
            Logger.Log(new LogMessage(LogSeverity.Info, "Shutdown", "Stopping webserver"));
            server.StopAsync().Wait();
            
            Logger.Log(new LogMessage(LogSeverity.Info, "Shutdown", "Stopping webserver"));
            console.Close();

            Logger.Log(new LogMessage(LogSeverity.Info, "Shutdown", "Stopping status task"));
            statusUpdater.Cancel();
        }
        
        private async Task HandleGuildJoin(SocketGuild guild) {
            if (!GuildProfileManager.GuildProfiles.ContainsKey(guild.Id)) {
            await GuildProfileManager.CreateGuildProfile(guild);
            await guild.SystemChannel.SendMessageAsync(@"Hello and thank you for inviting me!
By default my prefix is `<`. It should be changed when needed.
A list of commands can be obtained via the help command (i.e. with `<help`).
If you have any questions or concerns, join the support server (only Administrators can obtain the invite link)."); // Why do I have to remove the tabs preceeding each line?
            } else {
                await guild.SystemChannel.SendMessageAsync(@"Thank you for inviting me ... again!
The configuration for this guild hasn't been deleted and the bot will behave like you're used to!"); // Why do I have to remove the tabs preceeding each line?
            }
        }
        private async Task UpdateStatus() {
            // Note: Idrc if this task crashes
            while (true)
            {
                await Client.SetActivityAsync(new Game("eggs", ActivityType.Watching, ActivityProperties.None));
                await Task.Delay(5000, statusUpdaterToken);
                // Gotta credit the creator of this project, right?
                await Client.SetActivityAsync(new Game("GermanBread#9077", ActivityType.Watching, ActivityProperties.None));
                await Task.Delay(5000, statusUpdaterToken);
                await Client.SetActivityAsync(new Game($"ping: {Client.Latency}ms", ActivityType.Watching, ActivityProperties.None));
                await Task.Delay(5000, statusUpdaterToken);
                await Client.SetActivityAsync(new Game($"out for \"<\"", ActivityType.Watching, ActivityProperties.None));
                await Task.Delay(5000, statusUpdaterToken);

                GC.Collect();
            }
        }
    }
}