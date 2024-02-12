using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using TourneyBot.Tournament;

namespace TourneyBot.Commands {
    public class Info {
        public static async Task Client_Ready() {
            var guild = Program.Client.GetGuild(Program.GuildId);
            var command = new SlashCommandBuilder();
            command.WithName("info");
            command.WithDescription("Get info on currently running tournament");
            
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
            if (command.Data.Name == "info") {
                if (Program.CurrentTournament == null) {
                    await command.RespondAsync("No tournament running");
                    return;
                }
                var guild = Program.Client.GetGuild(Program.GuildId);
                EmbedBuilder embed = new EmbedBuilder().WithTitle("Tournament").WithFooter("Developed by mck");;
                int i = 0;

                foreach (Round round in Program.CurrentTournament.Rounds) {
                    i++;
                    
                    embed.AddField($"Round {i}", $"{Utils.ToUsefulLanguage(round.AiLevel.ToString())} AI, " +
                                                 $"{Utils.ToUsefulLanguage(round.Items.ToString())}, " +
                                                 $"{Utils.ToUsefulLanguage(round.Speed.ToString())}, " +
                                                 $"{Utils.ToUsefulLanguage(round.Tracks.ToString())}, " +
                                                 $"{Utils.ToUsefulLanguage(round.Vehicles.ToString())}, " +
                                                 $"{round.RaceCount} races" 
                        );
                }

                if (!Program.CurrentTournament.FinishedBuilding)
                    embed.Footer = new EmbedFooterBuilder().WithText("Tournament not yet open");
                else embed.Footer = new EmbedFooterBuilder().WithText("Tournament open for entries");
                if (Program.CurrentTournament.IsRunning) {
                    EmbedFieldBuilder field = new EmbedFieldBuilder().WithName("Remaining players");

                    foreach (ulong id in Program.CurrentTournament.RemainingPlayers) {
                        if (id.ToString().StartsWith("1234567")) field.Value += "Dummy, ";
                        else field.Value += guild.GetUser(id).Username + ", ";
                    }

                    embed.AddField(field);
                    embed.Footer = new EmbedFooterBuilder().WithText("Tournament running");
                }

                await command.RespondAsync(embed: embed.Build());
            }
        }
    }
}