// System
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json.Serialization;

// Discord
using Discord;
using Discord.WebSocket;

// Mate
using Mate.Variables;

namespace Mate.Utility
{
    public static class GuildProfileManager
    {
        /// <summary>
        /// This method creates a profile for your guild of choice and saves it to the global dictionary.
        /// </summary>
        /// <param name="Guild">The guild you want to add.</param>
        public static Task CreateGuildProfile(SocketGuild Guild) {
            if (!GuildProfiles.ContainsKey(Guild.Id)) {
                GuildProfiles.Add(Guild.Id, new GuildVariables());
                Logger.Log(new LogMessage(LogSeverity.Info, "Profile manager", $"Created new profile for guild {Guild.Name}"));
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// This method saves the guild profiles to disk.
        /// </summary>
        public static async Task SaveGuildProfiles() {
            string jsonPath = GlobalVariables.GuildProfilesPath;

            // Create the directory if it does not exist
            Directory.CreateDirectory(jsonPath);

            // First, overwrite the backup files
            foreach (KeyValuePair<ulong, GuildVariables> pair in GuildProfiles)
            {
                // Rinse and repeat
                File.WriteAllText(Path.Join(jsonPath, pair.Key + ".json~"), JsonSerializer.Serialize(new GuildProfile(pair)));
            }

            // Repeat that for the primary bank
            // Create a file to mark this bank as "dirty"
            File.Create(Path.Join(jsonPath, "primary dirty")).Dispose();
            foreach (KeyValuePair<ulong, GuildVariables> pair in GuildProfiles)
            {
                // Rinse and repeat
                File.WriteAllText(Path.Join(jsonPath, pair.Key + ".json"), JsonSerializer.Serialize(new GuildProfile(pair)));
            }

            // Remove the dirty indicator
            File.Delete(Path.Join(jsonPath, "primary dirty"));

            await Task.Delay(0);
        }
        /// <summary>
        /// This method loads the profiles from disk
        /// </summary>
        public static async Task LoadGuildProfiles() {
            // If no path was provided, set it to the global variable.
            string jsonPath = GlobalVariables.GuildProfilesPath;
            // Create the directory if it does not exist.
            Directory.CreateDirectory(jsonPath);
            
            string fileExtension = ".json";
            
            // Check if the dirty indicator exists.
            // If that's the case, append a ~ to the file extension
            if (File.Exists(Path.Join(jsonPath, "primary dirty"))) {
                fileExtension = ".json~";
                await Logger.Log(new LogMessage(LogSeverity.Warning, "Profile manager", "Primary bank has been marked as dirty, loading secondary bank"));
            }

            // Get a list of all files in the profiles folder and filter them
            var _jsonFiles = Directory.GetFiles(jsonPath)
                .Where(file => file.EndsWith(fileExtension));
            foreach (var jsonFile in _jsonFiles)
            {
                try {
                    // Try reading the JSON
                    var data = JsonSerializer.Deserialize<GuildProfile>(File.ReadAllText(jsonFile));
                    GuildProfiles.Add(data.GuildId, data.GuildVars);
                } catch (Exception) {
                    // If the above fails, try a backup file instead
                    try {
                        await Logger.Log(new LogMessage(LogSeverity.Warning, "Profile manager", "File in primary bank seemed to be corrupt; loading backup instead..."));
                        var data = JsonSerializer.Deserialize<GuildProfile>(File.ReadAllText(jsonFile + "~"));
                        GuildProfiles.Add(data.GuildId, data.GuildVars);
                    } catch (Exception ex) {
                        // If that fails, throw an error and continue
                        await Logger.Log(new LogMessage(LogSeverity.Error, "Profile manager", "Failed to load guild profile!", ex));
                        continue;
                    }
                }
            }
        }
        private struct GuildProfile {
            // Accept a seperate key and value as input
            public GuildProfile(ulong GId, GuildVariables GVars) {
                GuildId = GId;
                GuildVars = GVars;
            }
            // Convert the pair to a seperate key and value
            public GuildProfile(KeyValuePair<ulong, GuildVariables> KPair) {
                GuildId = KPair.Key;
                GuildVars = KPair.Value;
            }
            /// <summary>
            /// The Guild Id
            /// </summary>
            [JsonPropertyName("GuildId")]
            public ulong GuildId { get; set; }
            
            /// <summary>
            /// This variable contains guild-specific variables
            /// </summary>
            [JsonPropertyName("GuildVariables")]
            public GuildVariables GuildVars { get; set; }
            
            /// <summary>
            /// This is not a real variable, it pretends to be one. 
            /// When you write to it, the pair gets split into a key and the value internally.
            /// When you read, this variable will contain a joined key and value.
            /// </summary>
            [JsonIgnore]
            public KeyValuePair<ulong, GuildVariables> GuildPair { 
                get => new(GuildId, GuildVars);
                set { GuildId = value.Key; GuildVars = value.Value; }
            }
        }
        /// <summary>
        /// Information related to guilds get stored here
        /// </summary>
        /// <typeparam name="ulong">The Id of the guild</typeparam>
        /// <returns>Guild preferences</returns>
        public static Dictionary<ulong, GuildVariables> GuildProfiles { get; set; } = new Dictionary<ulong, GuildVariables>();
    }
}