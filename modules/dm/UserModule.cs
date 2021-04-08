// System
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

// Discord
using Discord;
using Discord.Commands;
using Discord.WebSocket;

// Mate
using Mate.Utility;
using Mate.Variables;

namespace Mate.Modules
{
    [RequireContext(ContextType.DM, ErrorMessage = "This command cannot be run here...")]
    public class UserDMModule : ModuleBase<SocketCommandContext>
    {
        [Command("help", true)]
        [Alias("h")]
        [Summary("Help command in DMs")]
        public async Task ShowDMHelp() {
            // Notify the user that the bot is doing something
            var _typing = Context.Channel.EnterTypingState();

            // Build an embed
            EmbedBuilder _embed = new EmbedBuilder {
                Title = "Command list\nRun these without a prefix i.e. `help` instead of `<help`",
                Color = Color.Teal
            };
            
            // Commands for everyone
            List<string> commands = new List<string> {
                "`help` - Shows this menu",
            };
            
            // Commands list
            string _generated = "";
            foreach (var item in commands) {
                _generated += $"\n{item}";
            }
            _embed.Description = _generated;
            
            // Build the embed
            Embed _builtEmbed = _embed.Build();
            
            _typing.Dispose(); // Stop typing status

            // Send the embed
            await ReplyAsync(null, false, _builtEmbed);
        }
        [Command("close")]
        [Summary("Closes the DM with the bot")]
        public async Task closedm() {
            await Context.User.GetOrCreateDMChannelAsync().Result.CloseAsync();
        }
    }
}