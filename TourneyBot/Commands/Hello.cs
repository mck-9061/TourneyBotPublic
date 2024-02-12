using System;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace TourneyBot.Commands {
    public class Hello {
        public static async Task Client_Ready() {
            var guild = Program.Client.GetGuild(Program.GuildId);
            var command = new SlashCommandBuilder();
            command.WithName("hello");
            command.WithDescription("Say hello!");
            
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
            if (command.Data.Name == "hello") {
                await command.RespondAsync("Hello world!");
            }
        }
    }
}