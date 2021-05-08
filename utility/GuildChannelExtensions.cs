// System
using System;
using System.Threading.Tasks;

// Discord
using Discord;
using Discord.WebSocket;

namespace Mate.Utility {
    public static class GuildChannelExtensions
    {
        public static async Task SendActionLog(this SocketGuild Guild, string Message, Color EmbedColor) {
            EmbedBuilder _logEmbed = new EmbedBuilder {
                Title = $"Log - {DateTime.UtcNow.ToShortDateString()}:{DateTime.UtcNow.ToShortTimeString()}",
                Description = Message,
                Color = EmbedColor
            };
            SocketTextChannel targetChannel = Guild.GetTextChannel(GuildProfileManager.GuildProfiles[Guild.Id].LogChannelId);
            if (targetChannel != null) await targetChannel.SendMessageAsync(null, false, _logEmbed.Build());
            else await Guild.SystemChannel.SendMessageAsync("No log channel has been set, consider providing one via `config`", false, _logEmbed.Build());
        }
    }
}