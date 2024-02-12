using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;
using TourneyBot.Tournament;

namespace TourneyBot.Commands {
    public class Announce {
        public static async Task Client_Ready() {
            var guild = Program.Client.GetGuild(Program.GuildId);
            var command = new SlashCommandBuilder()
                .WithName("announce")
                .WithDescription("Announce the current tournament")
                .AddOption("channel", ApplicationCommandOptionType.Channel, "The channel to announce in",
                    isRequired: true)
                .AddOption("announce-to", ApplicationCommandOptionType.Role, "(Optional) The role to mention",
                    isRequired: false);
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
            if (command.Data.Name == "announce") {
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
                
                
                
                if (Program.CurrentTournament == null) {
                    await command.RespondAsync("No tournament ready!");
                    return;
                }
                
                if (Program.CurrentTournament.FinishedBuilding) {
                    await command.RespondAsync("Already announced!");
                    return;
                }
                
                await command.RespondAsync("Announcing!");
                var channel = (SocketTextChannel) command.Data.Options.First().Value;
                await channel.SendMessageAsync("New tournament!");

                if (command.Data.Options.Count > 1) {
                    SocketRole role = (SocketRole) command.Data.Options.ToArray()[1].Value;
                    await channel.SendMessageAsync($"<@&{role.Id}>");
                }

                int i = 0;
                foreach (Round round in Program.CurrentTournament.Rounds) {
                    i++;
                    EmbedBuilder builder = new EmbedBuilder()
                        .WithTitle($"Round {i}")
                        .AddField(new EmbedFieldBuilder()
                            .WithName("AI Level")
                            .WithValue(Utils.ToUsefulLanguage(round.AiLevel.ToString())))
                        .AddField(new EmbedFieldBuilder()
                            .WithName("Items")
                            .WithValue(Utils.ToUsefulLanguage(round.Items.ToString())))
                        .AddField(new EmbedFieldBuilder()
                            .WithName("Speed")
                            .WithValue(Utils.ToUsefulLanguage(round.Speed.ToString())))
                        .AddField(new EmbedFieldBuilder()
                            .WithName("Tracks")
                            .WithValue(Utils.ToUsefulLanguage(round.Tracks.ToString())))
                        .AddField(new EmbedFieldBuilder()
                            .WithName("Vehicles")
                            .WithValue(Utils.ToUsefulLanguage(round.Vehicles.ToString())))
                        .AddField(new EmbedFieldBuilder()
                            .WithName("Race count")
                            .WithValue(round.RaceCount.ToString()))
                        .WithFooter("Developed by mck");
                    await channel.SendMessageAsync(embed: builder.Build());
                }

                Program.CurrentTournament.FinishedBuilding = true;
                RestUserMessage message = await channel.SendMessageAsync("React to this message to join!");
                await message.AddReactionAsync(new Emoji("✋"));
                Program.CurrentTournament.reactionMessageId = message.Id;
                Program.CurrentTournament.AnnouncementChannelId = message.Channel.Id;
                Program.SaveToJSON();
            }
        }
    }
}