// System
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

// Discord
using Discord;
using Discord.Commands;
using Discord.WebSocket;

// Mate
using Mate.Extra;
using Mate.Utility;
using Mate.Variables;

namespace Mate.Modules
{
    [RequireContext(ContextType.Guild, ErrorMessage = "This command cannot be run here...")]
    [Name("Commands for moderation")]
    public class AdminGuildModule : ModuleBase<SocketCommandContext>
    {
        [RequireUserPermission(GuildPermission.Administrator, ErrorMessage = "You are not an administrator")]
        [Alias("conf", "config")]
        [Group("configure")]
        [Summary("Used for setting guild-specific settings")]
        public sealed class ConfigurationModule : ModuleBase<SocketCommandContext> {
            [Command]
            public async Task ChangeSettings() {
                EmbedBuilder _helpEmbed = new EmbedBuilder {
                    Title = "Usage",
                    Fields = new List<EmbedFieldBuilder> {
                        new EmbedFieldBuilder {
                            Name = "prefix",
                            Value = "The prefix this bot should use\nUsage: `configure prefix [new prefix]`"
                        },
                        new EmbedFieldBuilder {
                            Name = "log channel",
                            Value = "The channel where log messages should go to\nUsage: `configure log channel [channel mention or Id]`"
                        }
                    },
                    Color = Color.Green
                };
                await ReplyAsync(null, false, _helpEmbed.Build());
            }
            [Alias("pref", "pre")]
            [Command("prefix")]
            public async Task ChangePrefix(string prefix = null) {
                string currentPrefix = GuildProfileManager.GuildProfiles[Context.Guild.Id].BotPrefix;
                if (prefix == null) {
                    await ReplyAsync($"The current prefix is `{currentPrefix}`");
                    return;
                }
                if (currentPrefix == prefix) {
                    await ReplyAsync("That prefix is already set");
                    return;
                }
                await ReplyAsync($"Prefix set to `{prefix}`");
                GuildProfileManager.GuildProfiles[Context.Guild.Id].BotPrefix = prefix;
            }
            [Alias("log")]
            [Command("log channel")]
            public async Task ChangeLogChannel(SocketTextChannel channel = null) {
                ulong currentChannelId = GuildProfileManager.GuildProfiles[Context.Guild.Id].LogChannelId;
                if (channel == null) {
                    await ReplyAsync($"The current channel is {MentionUtils.MentionChannel(currentChannelId)}");
                    return;
                }
                if (currentChannelId == channel.Id) {
                    await ReplyAsync("That channel is already set");
                    return;
                }
                GuildProfileManager.GuildProfiles[Context.Guild.Id].LogChannelId = channel.Id;
                await ReplyAsync($"Channel set to {MentionUtils.MentionChannel(channel.Id)}");
                await Context.Guild.SendActionLog("I will now start logging here", new Color(0x00FFAA));
            }
        }

        [RequireUserPermission(GuildPermission.Administrator, ErrorMessage = "You are not an administrator")]
        [Alias("sup")]
        [Command("support")]
        public async Task SendSupportServerInvite() {
            await ReplyAsync("You got mail!");
            var _dm = await Context.User.GetOrCreateDMChannelAsync();
            await _dm.SendMessageAsync("Here is the invite to the support server [https://discord.gg/tQ7hbAds4w]");
        }

        // TODO: Move to UserModule.cs
        [RequireUserPermission(GuildPermission.Administrator, ErrorMessage = "You are not an administrator")]
        [Alias("inv", "bot invite")]
        [Command("invite")]
        public async Task SendBotInvite() {
            await ReplyAsync("You got mail!");
            var _dm = await Context.User.GetOrCreateDMChannelAsync();
            await _dm.SendMessageAsync("Here is the invite for this bot [https://discord.com/oauth2/authorize?client_id=797478563568812074&scope=bot]");
        }

        /*public sealed class UserManagementModule : ModuleBase<SocketCommandContext> {

            [RequireUserPermission(GuildPermission.BanMembers, ErrorMessage = "You lack permissions to ban")]
            [Command("ban")]
            [Summary("Ban user by mention")]
            [Remarks("This command only accepts one mention")]
            public async Task BanUser(SocketGuildUser user, [Remainder] string reason = "No reason provided") {
                if (user.Id == Context.Client.CurrentUser.Id) {
                    await ReplyAsync($"You cannot ban this bot");
                    return;
                }
                else if (user.Id == Context.User.Id) {
                    await ReplyAsync($"You cannot ban yourself");
                    return;
                }
                else if (user.Id == Context.Guild.Owner.Id) {
                    await ReplyAsync($"You cannot ban the owner");
                    return;
                }
                
                IDMChannel _dm = await user.GetOrCreateDMChannelAsync();

                await Context.Guild.SendActionLog($"Banned `{user} | {user.Id}`", new Color(0xFF5500));

                string _message = $"You have been banned from `{Context.Guild}` by `{Context.User}`.\nReason `{reason}`";
                await _dm.SendMessageAsync(_message);

                await user.BanAsync(0, reason);
            }
            
            [RequireUserPermission(GuildPermission.BanMembers, ErrorMessage = "You lack permissions to ban")]
            [Command("banall")]
            [Summary("Ban users by role mention")]
            [Remarks("This command only accepts one mention")]
            public async Task Banall(IRole role, string reason = "No reason provided") {
                var guild = Context.Guild;
                var users = Context.Guild.Users.Where(user
                => user.Roles.Contains(role));
                string usernames = "";
                users.ToList().ForEach(async user
                => {
                        try {
                            await user.BanAsync(reason: reason);
                            var _dm = await user.GetOrCreateDMChannelAsync();
                            await _dm.SendMessageAsync($"You have been kicked from {guild}.\nReason `{reason}`");
                            usernames += "`" + user.ToString() + "` `" + user.Id + "`\n";
                        } catch { }
                    }
                );
                await guild.SendActionLog("Mass-ban done\nBanned:\n" + usernames, Color.Green);
            }
            
            [RequireUserPermission(GuildPermission.KickMembers, ErrorMessage = "You lack permissions to kick")]
            [Command("kick")]
            [Summary("Kick user by mention")]
            [Remarks("This command only accepts one mention")]
            public async Task KickUser(SocketGuildUser user, [Remainder] string reason = "No reason provided") {
                if (user.Id == Context.Client.CurrentUser.Id) {
                    await ReplyAsync($"You cannot kick this bot");
                    return;
                }
                else if (user.Id == Context.User.Id) {
                    await ReplyAsync($"You cannot kick yourself");
                    return;
                }
                else if (user.Id == Context.Guild.Owner.Id) {
                    await ReplyAsync($"You cannot kick the owner");
                    return;
                }
                
                IDMChannel _dm = await user.GetOrCreateDMChannelAsync();

                await Context.Guild.SendActionLog($"Kicked `{user} | {user.Id}`", new Color(0xFFFF00));

                await _dm.SendMessageAsync($"You have been kicked from {Context.Guild}.\nReason `{reason}`");

                await user.KickAsync(reason);
            }
            
            [RequireUserPermission(GuildPermission.KickMembers, ErrorMessage = "You lack permissions to kick")]
            [Command("kickall")]
            [Summary("Kick users by role mention")]
            [Remarks("This command only accepts one mention")]
            public async Task Kickall(IRole role, string reason = "No reason provided") {
                var guild = Context.Guild;
                var users = Context.Guild.Users.Where(user
                => user.Roles.Contains(role));
                string usernames = "";
                users.ToList().ForEach(async user
                => {
                        try {
                            await user.KickAsync(reason);
                            var _dm = await user.GetOrCreateDMChannelAsync();
                            await _dm.SendMessageAsync($"You have been kicked from {guild}.\nReason `{reason}`");
                            usernames += "`" + user.ToString() + "` `" + user.Id + "`\n";
                        } catch { }
                    }
                );
                await guild.SendActionLog("Mass-kick done\nKicked:\n" + usernames, Color.Green);
            }
        }*/

        /*[RequireUserPermission(GuildPermission.Administrator, ErrorMessage = "You are not an administrator")]
        [Command("upload")]
        [Summary("Upload image URLs in attached file")]
        public async Task Upload() {
            // No attachments, return
            if (Context.Message.Attachments.Count == 0) {
                await ReplyAsync("You must attach a text file with URLs");
                return;
            }

            await ReplyAsync("This will take a while");
            
            // Uhh .. GC please?
            _ = new TaskFactory().StartNew(async () => {
                string tempAttachPath = Path.Combine(Path.Combine(Path.GetTempPath(), "MateTemp"), "Attachments");
                string tempImgPath = Path.Combine(Path.Combine(Path.GetTempPath(), "MateTemp"), "Images");

                Directory.CreateDirectory(tempAttachPath);
                Directory.CreateDirectory(tempImgPath);

                foreach (var attachment in Context.Message.Attachments) {
                    Downloader.Download(attachment.Url, Path.Combine(tempAttachPath, attachment.Filename));
                }
                
                // Compile a list of URLs
                List<string> URLs = new List<string>();
                foreach (var file in Directory.GetFiles(tempAttachPath)) {
                    foreach (var line in File.ReadAllLines(file)) {
                        string url = line.Trim();
                        if (url.StartsWith("http"))
                            URLs.Add(url);
                    }
                }

                // Now download them
                foreach (var url in URLs) {
                    string file = Path.Combine(tempImgPath, Path.GetFileName(url));
                    Downloader.Download(url, file);
                    await Context.Channel.SendFileAsync(file);

                    // To prevent being rate-limited
                    await Task.Delay(1000);
                }

                // Delete the directory afterwards
                Directory.Delete(tempAttachPath, true);
                Directory.Delete(tempImgPath, true);
            });
        }*/
    }
}