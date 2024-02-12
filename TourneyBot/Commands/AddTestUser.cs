using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace TourneyBot.Commands {
    public class AddTestUser {
        public static int testCount = 0;
        public static async Task Client_Ready() {
            var guild = Program.Client.GetGuild(Program.GuildId);
            var command = new SlashCommandBuilder();
            command.WithName("add-test-user");
            command.WithDescription("Add a test, dummy user to the tournament.");
            command.AddOption("count", ApplicationCommandOptionType.Integer, "Number of users to add", isRequired: true);
            
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
            if (command.Data.Name == "add-test-user") {
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
                
                
                int count = Convert.ToInt32(command.Data.Options.First().Value);
                await command.RespondAsync($"Adding users...");
                for (int i = 0; i < count; i++) {
                    testCount++;
                    await command.Channel.SendMessageAsync($"Adding test user {testCount}.");
                    TestUser user = new TestUser();
                    user.Username = $"DummyUser{testCount}";
                    Program.CurrentTournament.PlayerIDs.Add(user.Id);
                }
            }
        }
    }
}