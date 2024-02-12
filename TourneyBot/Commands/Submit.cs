using System;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using TourneyBot.Tournament;

namespace TourneyBot.Commands {
    public class Submit {
        public static async Task Client_Ready() {
            var guild = Program.Client.GetGuild(Program.GuildId);
            var command = new SlashCommandBuilder();
            command.WithName("submit");
            command.WithDescription("Submit scores for players in the room.");
            
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
            if (command.Data.Name == "submit") {
                var guild = Program.Client.GetGuild(Program.GuildId);
                bool canUse = false;
                foreach (SocketRole role in guild.GetUser(command.User.Id).Roles) {
                    if (role.Name == "Tournament Manager") {
                        canUse = true;
                        break;
                    }
                }
                if (!canUse) {
                    await command.RespondAsync("You don't have permission to do that. Get a tournament manager to check your evidence or review your scores.");
                    return;
                }
                
                
                
                if (!command.Channel.Name.StartsWith("room-")) {
                    await command.RespondAsync("This can only be ran in a tournament room channel.");
                    return;
                }
                
                ModalBuilder builder = new ModalBuilder()
                    .WithTitle("Enter scores")
                    .WithCustomId(command.Channel.Name);
                
                int roomNo = Convert.ToInt32(command.Channel.Name.Split('-')[1]) - 1;
                Round currentRound = Program.CurrentTournament.Rounds[Program.CurrentTournament.currentRound - 1];
                Room room = currentRound.Rooms[roomNo];
                
                ulong dummy = 0;
                foreach (ulong id in room.PlayerIDs) {
                    if (id.ToString().StartsWith("123456")) {
                        dummy++;
                        builder.AddTextInput($"Dummy {dummy}", Convert.ToString(id + dummy), style: TextInputStyle.Short, required: true, value: "0");
                    }
                    else {
                        SocketGuildUser player = guild.GetUser(id);
                        builder.AddTextInput(player.Username, Convert.ToString(id), style: TextInputStyle.Short, required: true, value: "0");
                    }
                }

                await command.RespondWithModalAsync(builder.Build());
            }
        }
    }
}