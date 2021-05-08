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
using Discord.Commands.Builders;

// Mate
using Mate.Extra;
using Mate.Modules;
using Mate.Utility;
using Mate.Variables;

namespace Mate.Core
{
    public sealed partial class Bot : BotBase
    {
        private async Task InitCommands() {
            // Load the commands
            //await Command.AddModulesAsync(Assembly.GetEntryAssembly(), Service);
            
            // Guilds
            await Command.AddModuleAsync(typeof(UserGuildModule), Service);
            await Command.AddModuleAsync(typeof(AdminGuildModule), Service);
            await Command.AddModuleAsync(typeof(FunModule), Service);

            // DMs
            await Command.AddModuleAsync(typeof(UserDMModule), Service);
            
            // Debugging
            await Command.AddModuleAsync(typeof(DebugModule), Service);

            //await Logger.Log(new LogMessage(LogSeverity.Warning, "Command loader", "Creating commands from file"));

            if (Command.Commands.Count() == 0) await Logger.Log(new LogMessage(LogSeverity.Warning, "Command loader", "No commands have been found"));
            else if (Command.Commands.Count() == 1) await Logger.Log(new LogMessage(LogSeverity.Info, "Command loader", "1 command has been loaded"));
            else await Logger.Log(new LogMessage(LogSeverity.Info, "Command loader", $"{Command.Commands.Count()} commands have been loaded"));
        }

        private async Task HandleCommands(SocketMessage message) {
            SocketUserMessage _message = message as SocketUserMessage; // Convert the argument
            if (_message == null) return; // Exit if the message is a system message
            if (_message.Author.IsBot) return; // Exit if the message is from a bot
            if (_message.Author.IsWebhook) return; // Exit if the message is from a webhook

            SocketCommandContext context = new SocketCommandContext(Client, _message);

            int _pos = 0;
            string prefix;
            // Note: Exception handling looks cleaner here...
            try {
                prefix = GuildProfileManager.GuildProfiles[context.Guild.Id].BotPrefix;
            } catch (KeyNotFoundException) {
                prefix = "<";
                await GuildProfileManager.CreateGuildProfile(context.Guild);
            } catch (NullReferenceException) {
                // This means that we're most likely in a DM
                prefix = "<";
            }
            if (_message.HasStringPrefix(prefix, ref _pos)) {
                // Exit if it's a mention (those start with <), and because we're dealing with a substring, we don't need to check for <
                string _substr = _message.Content.Substring(_pos);
                if (_substr.StartsWith("@")
                 || _substr.StartsWith("#")
                  || _substr.StartsWith(":")
                   || _substr.StartsWith("a:")
                    || _substr.StartsWith("http"))
                    return;
                
                // Before we execute the command, check if maintenance mode is enabled
                if (GlobalVariables.MaintenanceModeGuild.HasValue) {
                    if (GlobalVariables.MaintenanceModeGuild.Value != context.Guild.Id) {
                        return;
                    }
                }
                
                // Now execute the command
                var _result = await Command.ExecuteAsync(context, _pos, Service); // Run the command
                
                
                // Post-execution handling
                await Logger.Log(new LogMessage(LogSeverity.Info, "Command handler", $"Executed \"{_message.Content.Substring(_pos)}\" for user {context.User}"));
                if (_result.IsSuccess) return;
                else await Logger.Log(new LogMessage(LogSeverity.Warning, "Command handler", $"Previous command errored with reason \"{_result.ErrorReason}\""));
                
                switch (_result.Error)
                {
                    case CommandError.UnknownCommand:
                        await _message.Channel.SendMessageAsync($"This command does not exist");
                        await _message.Channel.SendMessageAsync("https://tenor.com/view/bruh-moai-intense-transitions-reddit-chungus-gif-20326848");
                        await _message.Channel.SendMessageAsync($"`{prefix}help` exists y'know?");
                        break;
                    case CommandError.BadArgCount:
                        await _message.Channel.SendMessageAsync(_result.ErrorReason.Replace(" text", null));
                        break;
                    case CommandError.UnmetPrecondition:
                        // Replacing string...
                        /*string _missingPermission = _result.ErrorReason.Replace("User requires guild permission ", "").Replace("Bot requires guild permission ", "").Replace(".", "");
                        string _errorMessage;
                        if (_result.ErrorReason.StartsWith("Command must be used in")) _errorMessage = _result.ErrorReason;
                        else _errorMessage= _result.ErrorReason.StartsWith("User") ? $"You need the {_missingPermission} permission to run this command" : $"This bot cannot execute this command without the {_missingPermission} permission.";
                        await _message.Channel.SendMessageAsync(_errorMessage);*/
                        await _message.Channel.SendMessageAsync(_result.ErrorReason);
                        break;
                    default:
                        await _message.Channel.SendMessageAsync($"Oh snap, something went wrong");
                        break;
                }
            }
        }
    }
}