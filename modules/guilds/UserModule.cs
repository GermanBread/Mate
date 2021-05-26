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
using Mate.Utility;
using Mate.Variables;

namespace Mate.Modules
{
    [RequireContext(ContextType.Guild, ErrorMessage = "This command cannot be run here...")]
    [Name("Commands that everyone can run")]
    public class UserGuildModule : ModuleBase<SocketCommandContext>
    {
        public List<Task> tasks = new();
        
        [Command("help", true)]
        [Summary("Shows this menu")]
        public async Task ShowHelp() {
            // Send the embed
            await ReplyAsync(null, false, Cache.GetOrGenerateHelpEmbed(Context.User as SocketGuildUser, ContextType.Guild));
        }
        
        [Command("ping")]
        [Summary("Measure the lag this bot is experiencing")]
        public async Task Ping() {
            await ReplyAsync($"{Context.User.Mention} Pong!\n{Context.Client.Latency}ms latency");
        }

        [Alias("inv", "bot invite")]
        [Command("invite")]
        public async Task SendBotInvite() {
            await ReplyAsync("You got mail!");
            var _dm = await Context.User.GetOrCreateDMChannelAsync();
            await _dm.SendMessageAsync("Here is the invite for this bot [https://discord.com/oauth2/authorize?client_id=797478563568812074&scope=bot]");
        }

        /*[Command("wget")]
        [Summary("Compile a list of URLs for x images in this channel")]
        public async Task CreateWgetList(int limit) {
            // Account for bot response and the user typing the command
            limit += 2;
            
            // First we dispose any unused task
            tasks.ForEach(task
             => { if (task.IsCompleted) task.Dispose(); });
            
            // Delete the original message
            await ReplyAsync("Compiling list, this will take a while");
            
            // Spawn a task to prevent it from blocking the main thread
            var task = new TaskFactory().StartNew(async () => {
                var attachmentUrls = new List<string>();
                var messages = Context.Channel.GetMessagesAsync(limit);
                
                // Get all attachments
                await messages.AnyAwaitAsync(ProcessList);
                
                // If the bot found attachments, compile them into list
                if (attachmentUrls.Count > 0)
                    await messages.AllAwaitAsync((_) => {
                        StringBuilder output = new StringBuilder();
                        foreach (var item in attachmentUrls)
                        {
                            output.AppendLine(item);
                        }
                        
                        // Create a temporary file
                        string file = Path.GetTempFileName() + ".txt";
                        
                        // Write the list to it
                        File.WriteAllText(file, output.ToString());
                        
                        // Attach the file
                        Context.Channel.SendFileAsync(file, Context.User.Mention + $" I found around {attachmentUrls.Count} images");
                        
                        // Delete the file
                        File.Delete(file);
                        return new ValueTask<bool>();
                    });
                // Note: invisible curly brace
                else {
                    await ReplyAsync("I didn't find any images");
                }

                ValueTask<bool> ProcessList(IReadOnlyCollection<IMessage> messageCollection) {
                    // Iterate through each message
                    messageCollection.ToList().ForEach(message
                    
                    // Locate attachments
                     => {
                        message.Attachments.ToList().ForEach(attachment
                       
                        // Add the urls to the list
                         => {
                            attachmentUrls.Add(attachment.Url);
                        });
                    });

                    messageCollection.ToList().ForEach(message
                    
                    // Same with URLs
                     => {
                        // Only trust Discord URLs
                        if (message.Content.StartsWith("https://cdn.discordapp.com/attachments/"))
                        attachmentUrls.Add(message.Content);
                    });
                    
                    return new ValueTask<bool>(true);
                }
            });

            // Add this task to the array to be disposed later
            tasks.Add(task);
        }*/
    }
}