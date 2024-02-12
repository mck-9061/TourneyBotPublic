using System;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace TourneyBot.Commands {
    public class CreateTournament {
        public static async Task Client_Ready() {
            var guild = Program.Client.GetGuild(Program.GuildId);
            var command = new SlashCommandBuilder()
                .WithName("create")
                .WithDescription("Begin building a tournament");
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



            if (command.Data.Name == "create") {
                var guild = Program.Client.GetGuild(Program.GuildId);
                bool canUse = false;
                
                Console.WriteLine(guild.Id);
                Console.WriteLine(command.User.Id);
                
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
                
                
                
                string warning = "";
                if (Program.CurrentTournament != null) {
                    await command.RespondAsync("Tournament already exists - remove with /clean to start a new one");
                    return;
                }
                
                Program.CurrentTournament = new Tournament.Tournament();
                await command.RespondAsync("Tournament created! Run /addround to add rounds, then /announce when you're done.");
                Program.SaveToJSON();
            }
        }
    }
}