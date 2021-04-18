// System
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Threading.Tasks;

// Discord
using Discord;
using Discord.Commands;
using Discord.WebSocket;

// Mate
using Mate.Utility;
using Mate.Variables;

namespace Mate.Core
{
    public abstract class BotBase
    {
        public DiscordSocketClient Client { get; protected set; }
        public CommandService Command;
        protected IServiceProvider Service = null;
        private CancellationTokenSource BotCancelTokenSource = new CancellationTokenSource();
        private CancellationToken BotCancelToken;
        private CancellationTokenSource BackupCancelTokenSource = new CancellationTokenSource();
        private CancellationToken BackupCancelToken;
        // We assume that the child classes will contain a "Run" and "Stop" method
        private MethodBase[] ChildMethods = typeof(Bot).GetRuntimeMethods().ToArray();
        private bool IsMakingBackup = false;
        
        /// <summary>
        /// Starts the bot.
        /// </summary>
        public async Task Start() {           
            await Logger.Log(new LogMessage(LogSeverity.Info, "Startup", $"Initializing variables"));
            await InitVariables();

            await Logger.Log(new LogMessage(LogSeverity.Info, "Startup", $"Subscribing logs"));
            Client.Log += Logger.Log;
            Command.Log += Logger.Log;

            await Logger.Log(new LogMessage(LogSeverity.Info, "Startup", $"Loading guild profiles"));
            await GuildProfileManager.LoadGuildProfiles();
            if (GuildProfileManager.GuildProfiles.Count == 1) await Logger.Log(new LogMessage(LogSeverity.Info, "Profile manager", $"Loaded one guild profile"));
            else await Logger.Log(new LogMessage(LogSeverity.Info, "Startup", $"Loaded {GuildProfileManager.GuildProfiles.Count} guild profiles"));

            await Logger.Log(new LogMessage(LogSeverity.Info, "Startup", $"Logging in"));
            await Client.LoginAsync(TokenType.Bot, File.ReadAllText(GlobalVariables.TokenPath).Trim()); // using Trim() is useful if you use text editors that like to append empty lines (looking at you, Kate)
            await Client.StartAsync();

            await Logger.Log(new LogMessage(LogSeverity.Info, "Startup", $"Downloading memberlists"));
            await Client.DownloadUsersAsync(Client.Guilds);

            await Logger.Log(new LogMessage(LogSeverity.Info, "Startup", $"Starting backup task"));
            _ = PeriodicBackupTask(2);

            await InvokeChildMethod("Init");

            // As soon as the client signals ready, we complete the init process
            Client.Ready += () => {
                // To prevent this method from firing the invoke again
                if (GlobalVariables.StartupComplete) return Task.CompletedTask;
                GlobalVariables.StartupComplete = true;
                
                // Call the run method from children
                InvokeChildMethod("Run").Wait();

                GlobalVariables.StartupTime = GlobalVariables.Uptime; // Set the startup time
                $"<<< Startup completed in {GlobalVariables.StartupTime} milliseconds >>>".WriteLine(ConsoleColor.Green);
                
                return Task.CompletedTask;
            };
            
            // Code resposible for keeping the bot alive
            try {
                await Task.Delay(-1, BotCancelToken);
            } catch (TaskCanceledException) {
                GlobalVariables.ShutdownTime = GlobalVariables.Uptime - GlobalVariables.ShutdownTime; // Now correct the uptime
                if (!GlobalVariables.ShuttingDown) $"<<< Shutdown completed in {GlobalVariables.ShutdownTime} milliseconds >>>".WriteLine(ConsoleColor.Red); // Clean shutdown
                else "<<< Shutdown aborted >>>".WriteLine(ConsoleColor.Red); // Dirty shutdown
            }
        }

        /// <summary>
        /// Stops the bot. DO NOT AWAIT!
        /// </summary>
        public async Task Quit() {
            // Reset the filter
            Logger.MinimumPriority = 99;
            
            GlobalVariables.ShuttingDown = true;

            "<<< Shutting down >>>".WriteLine(ConsoleColor.Red);
            
            if (IsMakingBackup) await Logger.Log(new LogMessage(LogSeverity.Warning, "Shutdown", "A backup task is currently running, waiting for it to finish. Press Control + C again to abort"));
            while (IsMakingBackup);
            
            GlobalVariables.ShutdownTime = GlobalVariables.Uptime; // A hack, but it works

            await Logger.Log(new LogMessage(LogSeverity.Info, "Shutdown", "Stopping backup task"));
            BackupCancelTokenSource.Cancel();
            
            await Logger.Log(new LogMessage(LogSeverity.Info, "Shutdown", "Logging out"));
            await Client.StopAsync();
            await Client.LogoutAsync();
            //Client.Dispose();

            await Logger.Log(new LogMessage(LogSeverity.Info, "Shutdown", "Unsubscribing logs"));
            Client.Log -= Logger.Log;
            Command.Log -= Logger.Log;

            // Call the exit method
            await InvokeChildMethod("Stop");

            await Logger.Log(new LogMessage(LogSeverity.Info, "Shutdown", "Saving guild profiles"));
            await GuildProfileManager.SaveGuildProfiles();

            GlobalVariables.ShuttingDown = false; // Now before we stop the main thread, we want to reset this variable. It indicates a dirty shutdown

            await Logger.Log(new LogMessage(LogSeverity.Info, "Shutdown", "Stopping main thread"));
            BotCancelTokenSource.Cancel();
        }

        /// <summary>
        /// Stops the bot in an instant. DO NOT AWAIT
        /// </summary>
        public async Task Abort() {
            Logger.MinimumPriority = 99;
            
            GlobalVariables.ShuttingDown = true;
            
            "<<< Aborting operation >>>".WriteLine(ConsoleColor.Red);
            
            await Logger.Log(new LogMessage(LogSeverity.Info, "Abort", "Stopping main thread"));
            BotCancelTokenSource.Cancel();
        }
        
        private async Task InitVariables() {
            // Extract the token
            BotCancelToken = BotCancelTokenSource.Token;
            
            // Extract the token
            BackupCancelToken = BotCancelTokenSource.Token;

            // Subscribe a cancel handler
            Console.CancelKeyPress += (object _, ConsoleCancelEventArgs args) => {
                Logger.Log(new LogMessage(LogSeverity.Info, "Console", "ControlC was recieved"));
                args.Cancel = true;
                if(!GlobalVariables.ShuttingDown) _ = Quit(); // We want a clean shutdown
                else BotCancelTokenSource.Cancel(); // If ControlC is pressed again while shutting down, kill the main thread
            };
            
            Client = new DiscordSocketClient(new DiscordSocketConfig {
                LogLevel = LogSeverity.Info,
                ExclusiveBulkDelete = false // To get rid of that warning
            });
            
            Command = new CommandService(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Info,
                CaseSensitiveCommands = false
            });

            await Task.Delay(0);
        }

        private async Task InvokeChildMethod(string name) {
            try {
                await Logger.Log(new LogMessage(LogSeverity.Info, "Invoke", $"Invoking child method \"{name}\""));
                ChildMethods.First(x => x.Name.Equals(name)).Invoke(this, null);
            } catch (InvalidOperationException) {
            } catch (TargetInvocationException ex) {
                await Logger.Log(new LogMessage(LogSeverity.Error, "Invoke", $"Invoked child method \"{name}\" threw exception", ex.InnerException));
            } catch (Exception ex) {
                await Logger.Log(new LogMessage(LogSeverity.Error, "Invoke", $"Failed to invoke child method \"{name}\"", ex));
            }
        }
        
        /// <summary>
        /// This task will make periodic backups of the guild parameters. DO NOT AWAIT
        /// </summary>
        /// <param name="frequency">How many times per hour this should run</param>
        private async Task PeriodicBackupTask(int frequency) {
            try {
                // Loop until this task is cancelled
                while (true /* no need to check if the token got cancelled, the Exception will throw us out of the loop */) {
                    await Task.Delay(360000 / frequency, BackupCancelToken);
                    await Logger.Log(new LogMessage(LogSeverity.Info, "Backup", "Saving guild profiles"));
                    IsMakingBackup = true;
                    await GuildProfileManager.SaveGuildProfiles();
                    IsMakingBackup = false;
                    await Logger.Log(new LogMessage(LogSeverity.Info, "Backup", "Backup complete"));
                }
            } catch (TaskCanceledException) { }
        }
    }
}
