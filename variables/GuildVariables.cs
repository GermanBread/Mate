// System
using System.Text.Json.Serialization;

// Discord
using Discord.WebSocket;

namespace Mate.Variables {
    /// <summary>
    /// This class holds all information related to a guild
    /// </summary>
    public class GuildVariables
    {
        /// <summary>
        /// This is the bot prefix
        /// </summary>
        [JsonPropertyName("BotPrefix")]
        public string BotPrefix { get; set; } = "<";
        /// <summary>
        /// This is the channel where log messages go to
        /// </summary>
        [JsonPropertyName("LogChannel")]
        public ulong LogChannelId { get; set; } = 0;
    }
}