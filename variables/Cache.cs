using System.Diagnostics.Tracing;
// System
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

// Discord
using Discord;
using Discord.Commands;
using Discord.WebSocket;

// Mate
using Mate.Utility;

namespace Mate.Variables
{
    public static class Cache
    {
        public static Dictionary<GuildPermissions, Embed> EmbedCache { get; private set; } = new Dictionary<GuildPermissions, Embed>();
        public static Embed GetOrGenerateHelpEmbed(SocketGuildUser User, ContextType Context) {
            // Check if the embed already exists
            if (!EmbedCache.ContainsKey(User.GuildPermissions)) {
                // Log the action
                Logger.Log(new LogMessage(LogSeverity.Info, "Cache", $"Creating new help cache for user {User}, permission mask: {User.GuildPermissions}"));
                
                // If it does not, create one!
                EmbedBuilder builder = new EmbedBuilder {
                    Title = "List of commands",
                    Description = generateCommandField(User.GuildPermissions.ToList(), Context),
                    Color = Color.Teal
                };
                Embed built = builder.Build();
                
                // Then add the result to the cache
                EmbedCache.Add(
                    User.GuildPermissions,
                    built
                );

                // And return it
                return built;
            } else {
                Logger.Log(new LogMessage(LogSeverity.Info, "Cache", $"Recycled help cache for user {User}, permission mask: {User.GuildPermissions}"));
                return EmbedCache[User.GuildPermissions];
            }

            string generateCommandField(List<GuildPermission> Permissions, ContextType Context) {
                var _commands = new List<CommandInfo>();
                List<ModuleInfo> _modules = new List<ModuleInfo>();

                GlobalVariables.DiscordBot.Command.Commands.ToList().ForEach(command
                 => {
                    // Check if the permissions parameter is null (indicates a DM)
                    if (Permissions == null) {
                        _commands.Add(command);
                        return;
                    }
                    
                    // Exit if the command belongs to a group.
                    if (!string.IsNullOrEmpty(command.Module.Group)) return;
                    
                    // Exit if the command is only executable by the owner of the bot
                    if (command.Module.Preconditions.OfType<RequireOwnerAttribute>().Count() > 0) return;

                    // We need to check whether or not this command can run in this context.
                    var contextAttributes = command.Module.Preconditions.OfType<RequireContextAttribute>();

                    // Try to get the attribute, if this contains 0 elements, it does not exist.
                    var permissionAttributes = command.Preconditions.OfType<RequireUserPermissionAttribute>();
                    
                    // Check each attribute if the context type mismatches.
                    if (contextAttributes.Any(attribute
                     => attribute.Contexts != Context)) return;

                    // Now we can go check the permissions.
                    // If the command has no permission attributes, add it
                    if (permissionAttributes.Count() == 0) _commands.Add(command);
                    
                    // if the permissions match, add the command.
                    if (permissionAttributes.Any(attribute
                     => Permissions.Contains(attribute.GuildPermission.Value))) {
                        _commands.Add(command);
                    }
                });

                GlobalVariables.DiscordBot.Command.Modules.ToList().ForEach(module
                 => {
                    // Check if the command is only executable by the owner of the bot
                    if (module.Preconditions.OfType<RequireOwnerAttribute>().Count() > 0) return;
                    
                    // Check if this command can be run inside a guild.
                    // We need to check whether or not this command can run in this context.
                    var contextAttributes = module.Preconditions.OfType<RequireContextAttribute>();
                    
                    // Check each attribute if the context type mismatches.
                    if (contextAttributes.Any(attribute
                     => attribute.Contexts != Context)) return;

                    // Now check for permissions.
                    // Try to get the attribute, if this contains 0 elements, it does not exist.
                    var permissionAttributes = module.Preconditions.OfType<RequireUserPermissionAttribute>();

                    // Check if attributes were found.
                    try {
                        permissionAttributes.First();
                    } catch {
                        // If the permissions parameter is null, add this command to the list
                        if (Permissions == null) _modules.Add(module);
                        else return;
                    }

                    // If the permissions parameter is null, skip.
                    if (Permissions == null) return;
                    
                    // Now we can go check the permissions.
                    // if the permissions match, add the command.
                    if (permissionAttributes.Any(attribute
                     => Permissions.Contains(attribute.GuildPermission.Value))) {
                        _modules.Add(module);
                    }
                });

                // Get a list of all modules that are NOT submodules and DON'T belong to a group
                _modules.AddRange(GlobalVariables.DiscordBot.Command.Modules.Where(module
                 => !module.IsSubmodule && string.IsNullOrEmpty(module.Group)));
                
                
                // Add all commands in order
                string _generatedField = "";
                string _lastModuleName = "";
                _commands.Where(command
                 => !string.IsNullOrEmpty(command.Name) && string.IsNullOrEmpty(command.Module.Group)).ToList().ForEach(command
                     => {
                        string _commandParameters = "";
                        string _commandAliases = "";

                        // Add the command aprameters to the string
                        command.Parameters.ToList().ForEach(parameter
                         => _commandParameters += $" [{(parameter.IsOptional ? "(optional) " : "")}{parameter.Name}]");

                        command.Aliases.Where(alias => alias != command.Name).ToList().ForEach(alias
                         => _commandAliases += $" \"{alias}\"");
                        
                        // Now add the description
                        var guild = User.Guild;
                        if (_lastModuleName != command.Module.Name && !command.Module.IsSubmodule) {
                            _generatedField += $"\n__**{command.Module.Name}**__\n";
                            
                            string moduleAliases = "";

                            // Add all command groups in order
                            _modules.Where(module
                                 => !string.IsNullOrEmpty(module.Group)
                                     && module.IsSubmodule
                                         && module.Parent.Name == command.Module.Name).ToList().ForEach(module
                                             => {
                                            // Add the module aprameters to the string
                                            module.Aliases.Where(alias => alias != module.Name).ToList().ForEach(alias
                                             => moduleAliases += $" \"{alias}\"");

                                            _generatedField += $"**{module.Name}**"
                                                 + (!string.IsNullOrEmpty(moduleAliases) ? $" *Aliases:{moduleAliases}*\n" : "\n")
                                                     + (!string.IsNullOrEmpty(module.Summary) ? $"{module.Summary}\n" : "No description provided\n")
                                                         + (!string.IsNullOrEmpty(module.Remarks) ? $"Remarks: {module.Remarks}\n" : "");
                                }
                            );
                        }
                        _generatedField += $"**{command.Name}**"
                         +  (!string.IsNullOrEmpty(_commandAliases) ? $" *Aliases:{_commandAliases}*\n" : "\n")
                             + (!string.IsNullOrEmpty(command.Summary) ? $"{command.Summary}\n" : "No description provided\n")
                                 + (!string.IsNullOrEmpty(_commandParameters) ? $"Usage: `{command.Name}" + _commandParameters + "`\n" : "")
                                     + (!string.IsNullOrEmpty(command.Remarks) ? $"Remarks: {command.Remarks}\n" : "");

                        // Set the modules' name
                        _lastModuleName = command.Module.Name;
                    }
                );
                
                // Return the string and let the main function create the embed
                return _generatedField;
            }
        }
    }
}