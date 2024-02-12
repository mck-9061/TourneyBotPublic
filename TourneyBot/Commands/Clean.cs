using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace TourneyBot.Commands {
    public class Clean {
        public static async Task Client_Ready() {
            var guild = Program.Client.GetGuild(Program.GuildId);
            var command = new SlashCommandBuilder();
            command.WithName("clean");
            command.WithDescription("Clean up from tests - reset tournament, remove roles and channels");
            
            try {
                await guild.CreateApplicationCommandAsync(command.Build());
            }
            catch(ApplicationCommandException exception)
            {
                var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
                Console.WriteLine(json);
            }
        }

        public static async Task CommandHandler(SocketSlashCommand command) {
            if (command.Data.Name == "clean") {
                var guild = Program.Client.GetGuild(Program.GuildId);
                bool canUse = false;
                foreach (SocketRole role in guild.GetUser(command.User.Id).Roles) {
                    if (role.Name == "Tournament Manager") {
                        canUse = true;
                        break;
                    }
                }
                if (!canUse) {
                    await command.RespondAsync("You don't have permission to do that.");
                    return;
                }
                if (command.Channel.Name != "tourney-bot-commands") {
                    await command.RespondAsync("Please run this command in the tourney-bot-commands channel.", ephemeral: true);
                    return;
                }
                
                
                await command.RespondAsync("Cleaning up...");
                Program.SaveToJSON();
                string identifier = DateTime.Now.ToLongTimeString().Replace(":", "") +
                                    DateTime.Now.ToShortDateString().Replace("/", "");
                File.Copy("Tourney.json", $"old/tourney-{identifier}.json");
                
                
                Program.CurrentTournament = null;
                Program.SaveToJSON();

                foreach (SocketRole role in guild.Roles) {
                    try {
                        if (role.Name.Contains("Tourney:")) await role.DeleteAsync();
                    }
                    catch (Exception e) {
                        await command.Channel.SendMessageAsync(
                            "Failed to delete a role. Please delete manually.");
                    }
                }

                foreach (SocketTextChannel channel in guild.TextChannels) {
                    if (channel.Name.StartsWith("room-")) await channel.DeleteAsync();
                }

                foreach (SocketCategoryChannel category in guild.CategoryChannels) {
                    if (category.Name == "tournament") await category.DeleteAsync();
                }

                await command.Channel.SendMessageAsync("Cleaned");
            }
        }
    }
}