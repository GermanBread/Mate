// System
using System;
using System.IO;
using System.Linq;
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

namespace Mate.Modules
{
    [Name("Fun stuff")]
    public partial class FunModule : ModuleBase<SocketCommandContext>
    {
        [Command("tada")]
        [Summary("🎉")]
        public async Task Tada() {
            await ReplyAsync("🎉🎉🎉🎉🎉\n🎉🎉🎉🎉🎉\n🎉🎉🎉🎉🎉\n🎉🎉🎉🎉🎉\n🎉🎉🎉🎉🎉");
        }
        [Command("flip a coin")]
        [Alias("coinflip", "coin")]
        [Summary("Flips a coin!")]
        public async Task CoinFlip() {
            string _result = new Random().Next(2) == 0 ? "heads" : "tails";
            await ReplyAsync($"{Context.User.Mention} you got {_result}!");
        }
        [Command("astronomia")]
        [Alias("astro")]
        [Summary("Astronimia remix")]
        public async Task Astronomia() {
            await ReplyAsync("https://cdn.discordapp.com/attachments/729787119886663800/819628336136978482/dans.webm");
        }
        [Command("pinged")]
        [Summary("So you got pinged on Discord, eh?")]
        public async Task Pinged() {
            await ReplyAsync("https://cdn.discordapp.com/attachments/729216637504585790/824618225881841704/A_sad_strange_little_man_-_discord_Ping-1354512342122430467.mp4");
        }
    }
}