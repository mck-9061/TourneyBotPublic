using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using TourneyBot.Scoreboard;

namespace TourneyBot.Commands {
    public class TopTen {
        public static async Task Client_Ready() {
            var guild = Program.Client.GetGuild(Program.GuildId);
            var command = new SlashCommandBuilder();
            command.WithName("top-ten");
            command.WithDescription("Get the top ten players.")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("type")
                    .WithDescription("Which variable to consider")
                    .WithRequired(true)
                    .AddChoice("Lifetime Points", 0)
                    .AddChoice("Room wins", 1)
                    .AddChoice("Tournament wins", 2)
                    .WithType(ApplicationCommandOptionType.Integer));
            
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
            var guild = Program.Client.GetGuild(Program.GuildId);
            if (command.Data.Name == "top-ten") {
                Dictionary<SerialScoreUser, int> topTen = null;
                string identifier = "";
                switch (Convert.ToInt32(command.Data.Options.First().Value)) {
                    case 0:
                        identifier = "Lifetime Scores";
                        topTen = Program.Scoreboard.TopTenScores();
                        break;
                    case 1:
                        identifier = "Room Wins";
                        topTen = Program.Scoreboard.TopTenRooms();
                        break;
                    case 2:
                        identifier = "Tournament Wins";
                        topTen = Program.Scoreboard.TopTenTourneys();
                        break;
                    default:
                        break;
                }

                if (topTen == null) {
                    await command.RespondAsync("Something went wrong!");
                    return;
                }
                
                EmbedBuilder builder = new EmbedBuilder().WithTitle($"Top Ten {identifier}").WithFooter("Developed by mck");;
                int i = 0;
                string title = "";
                foreach (SerialScoreUser user in topTen.Keys) {
                    i++;
                    string name = "";
                    if (user.DiscordId.ToString().StartsWith("1234567")) name = "Dummy";
                    else {
                        name = guild.GetUser(user.DiscordId).Username;
                    }

                    if (title == "") title = $"{i}) {name}: {topTen[user]}";
                    else {
                        builder.AddField(title, $"{i}) {name}: {topTen[user]}");
                        title = "";
                    }
                }
                if (title != "") builder.AddField(title, ".");

                await command.RespondAsync(embed: builder.Build());
            }
        }
    }
}