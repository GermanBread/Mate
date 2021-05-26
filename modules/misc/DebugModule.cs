// System
using System;
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
    [RequireOwner(ErrorMessage = "Hey! Don't touch this!")]
    public class DebugModule : ModuleBase<SocketCommandContext>
    {
        [Command("dumpcache")]
        [Alias("dc")]
        public async Task DumpCache() {
            Cache.EmbedCache.Keys.ToList().ForEach(key
             => ReplyAsync($"Dumping embed cache: Key: `{key}`", false, Cache.EmbedCache[key]));
            await Task.Delay(0);
        }
        [Command("log")]
        public async Task Log([Remainder] string message) {
            await Context.Guild.SendActionLog(message, Color.Magenta);
        }
        [Command("uldisk")]
        public async Task UlFiles(string path) {
            await ReplyAsync("This will take a while");
            var files = System.IO.Directory.GetFiles(path);
            _ = new TaskFactory().StartNew(async ()
             => {
                foreach (var file in files)
                {
                    await Context.Channel.SendFileAsync(file);

                    // To prevent being rate-limited
                    await Task.Delay(1000);
                }
                await ReplyAsync("Done");
            });
        }
        [Command("stop")]
        public async Task StopBot() {
            var _stopmsg = await ReplyAsync("<a:windows_loading:755873787807006872> Stopping");
            _ = GlobalVariables.DiscordBot.Quit();
            _ = _stopmsg.ModifyAsync(x => x.Content = "✅ Stopped");
        }
        [Command("restart")]
        public async Task RestartBot() {
            await ReplyAsync("<a:windows_loading:755873787807006872> Rebooting");
            _ = GlobalVariables.DiscordBot.Reboot();
        }
    }
}