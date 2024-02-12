using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using System.Text.Json;
using Discord.Rest;
using TourneyBot.Commands;
using TourneyBot.Listeners;
using TourneyBot.Scoreboard;

namespace TourneyBot {
    class Program {
        private static string token = "REDACTED";
        public static DiscordSocketClient Client;

        public static ulong GuildId = 0; // Replace with guild ID

        public static SerialScoreboard Scoreboard;
        
        public static Task Main(string[] args) => new Program().MainAsync();

        public static Tournament.Tournament CurrentTournament;

        public async Task MainAsync() {
            Client = new DiscordSocketClient();
            Client.Log += Log;
            SerialScoreboard.Load();
            // Commands
            await InitialiseCommands();
            Client.SlashCommandExecuted += Hello.CommandHandler;
            Client.SlashCommandExecuted += CreateTournament.CommandHandler;
            Client.SlashCommandExecuted += AddRound.CommandHandler;
            Client.SlashCommandExecuted += Announce.CommandHandler;
            Client.SlashCommandExecuted += StartTournament.CommandHandler;
            Client.SlashCommandExecuted += AddTestUser.CommandHandler;
            Client.SlashCommandExecuted += Clean.CommandHandler;
            Client.SlashCommandExecuted += Info.CommandHandler;
            Client.SlashCommandExecuted += Submit.CommandHandler;
            Client.SlashCommandExecuted += TopTen.CommandHandler;
            
            // Listeners
            Client.ModalSubmitted += ScoreSubmitListener.ModalListener;

            Client.Ready += async () => {
                var guild = Program.Client.GetGuild(Program.GuildId);
                bool exists = false;
                foreach (SocketRole role in guild.Roles) {
                    if (role.Name == "Tournament Manager") {
                        exists = true;
                        break;
                    }
                }

                if (!exists) {
                    RestRole role = await guild.CreateRoleAsync("Tournament Manager");
                    RestTextChannel channel = await guild.CreateTextChannelAsync("tourney-bot-commands");
                    await channel.AddPermissionOverwriteAsync(Client.CurrentUser, OverwritePermissions.AllowAll(channel));
                    await channel.AddPermissionOverwriteAsync(guild.EveryoneRole, OverwritePermissions.DenyAll(channel));
                    await channel.AddPermissionOverwriteAsync(role, OverwritePermissions.AllowAll(channel));
                    // Replace with user IDs
                    await guild.GetUser(0).AddRoleAsync(role);
                    await guild.GetUser(0).AddRoleAsync(role);
                    
                    Console.WriteLine("Role and command channel created");
                }
            };
            
            await Client.LoginAsync(TokenType.Bot, token);
            await Client.StartAsync();
            
            LoadFromJSON();
            
            

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private async Task InitialiseCommands() {
            // This is only needed to register slash commands, so only needs to run once per command
            
             //Client.Ready += Hello.Client_Ready;
             //Client.Ready += CreateTournament.Client_Ready;
             //Client.Ready += AddRound.Client_Ready;
             //Client.Ready += Announce.Client_Ready;
             //Client.Ready += StartTournament.Client_Ready;
            //Client.Ready += AddTestUser.Client_Ready;
             //Client.Ready += Clean.Client_Ready;
             //Client.Ready += Info.Client_Ready;
            //Client.Ready += Submit.Client_Ready;
            //Client.Ready += TopTen.Client_Ready;
        }
        
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }


        public static void SaveToJSON() {
            JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerOptions.Default);
            options.WriteIndented = true;
            string json = JsonSerializer.Serialize(CurrentTournament, options);
            try {
                File.WriteAllText("Tourney.json", json);
            }
            catch (Exception e) {
                File.Create("Tourney.json");
                File.WriteAllText("Tourney.json", json);
            }
        }

        public static void LoadFromJSON() {
            try {
                CurrentTournament = JsonSerializer.Deserialize<Tournament.Tournament>(File.ReadAllText("Tourney.json"));
            }
            catch (Exception e) {
                CurrentTournament = new Tournament.Tournament();
                SaveToJSON();
            }
            
        }
    }
}