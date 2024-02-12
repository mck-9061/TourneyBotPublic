using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using TourneyBot.Settings;
using TourneyBot.Tournament;

namespace TourneyBot.Commands {
    public class AddRound {
        public static async Task Client_Ready() {
            var guild = Program.Client.GetGuild(Program.GuildId);
            var command = new SlashCommandBuilder()
                .WithName("addround")
                .WithDescription("Add a round to the current tournament")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("ai-level")
                    .WithDescription("Level of the computer's skill")
                    .WithRequired(true)
                    .AddChoice("Easy", "easy")
                    .AddChoice("Normal", "normal")
                    .AddChoice("Hard", "hard")
                    .WithType(ApplicationCommandOptionType.String))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("items")
                    .WithDescription("Item set")
                    .WithRequired(true)
                    .AddChoice("Normal", "normal")
                    .AddChoice("Frantic", "frantic")
                    .AddChoice("None", "none")
                    .WithType(ApplicationCommandOptionType.String))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("speed")
                    .WithDescription("Level of speed")
                    .WithRequired(true)
                    .AddChoice("50cc", "fifty")
                    .AddChoice("100cc", "hundred")
                    .AddChoice("150cc", "hundred_fifty")
                    .AddChoice("150cc Mirror", "mirror")
                    .AddChoice("200cc", "two_hundred")
                    .WithType(ApplicationCommandOptionType.String))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("tracks")
                    .WithDescription("Set of tracks")
                    .WithRequired(true)
                    .AddChoice("All", "all")
                    .AddChoice("Random", "random")
                    .AddChoice("Original (Mushroom/Flower/Star/Crown)", "nitro")
                    .AddChoice("Retro (Shell/Banana/Leaf/Lightning)", "retro")
                    .AddChoice("Wii U DLC (Egg/Crossing/Triforce/Bell)", "wii_u_dlc")
                    .AddChoice("Switch DLC", "switch_dlc")
                    .AddChoice("Special Tracks (Crown/Lightning/Rainbow Road/Bowser's Castle)", "special")
                    .WithType(ApplicationCommandOptionType.String))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("vehicles")
                    .WithDescription("Allowed vehicles")
                    .WithRequired(true)
                    .AddChoice("Karts", "karts")
                    .AddChoice("Bikes", "motorbikes")
                    .AddChoice("Any", "any")
                    .WithType(ApplicationCommandOptionType.String))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("race-count")
                    .WithDescription("Number of races")
                    .WithRequired(true)
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
            if (command.Data.Name == "addround") {
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
                    await command.RespondAsync("No tournament open!");
                    return;
                }
                if (Program.CurrentTournament.FinishedBuilding) {
                    await command.RespondAsync("Tournament cannot be edited!");
                }
                else {
                    AILevel.TryParse(((string) command.Data.Options.ToArray()[0].Value).ToUpper(), out AILevel level);

                    Items.TryParse(((string) command.Data.Options.ToArray()[1].Value).ToUpper(), out Items items);

                    Speed.TryParse(((string) command.Data.Options.ToArray()[2].Value).ToUpper(), out Speed speed);

                    Tracks.TryParse(((string) command.Data.Options.ToArray()[3].Value).ToUpper(), out Tracks tracks);

                    Vehicles.TryParse(((string) command.Data.Options.ToArray()[4].Value).ToUpper(), out Vehicles vehicles);

                    int raceCount = Convert.ToInt32(command.Data.Options.ToArray()[5].Value);
                    
                    Round round = new Round();
                    round.AiLevel = level;
                    round.Items = items;
                    round.Speed = speed;
                    round.Tracks = tracks;
                    round.Vehicles = vehicles;
                    round.RaceCount = raceCount;
                    
                    Program.CurrentTournament.Rounds.Add(round);

                    EmbedBuilder builder = new EmbedBuilder()
                        .WithTitle("Round added")
                        .AddField(new EmbedFieldBuilder()
                            .WithName("AI Level")
                            .WithValue(Utils.ToUsefulLanguage(level.ToString())))
                        .AddField(new EmbedFieldBuilder()
                            .WithName("Items")
                            .WithValue(Utils.ToUsefulLanguage(items.ToString())))
                        .AddField(new EmbedFieldBuilder()
                            .WithName("Speed")
                            .WithValue(Utils.ToUsefulLanguage(speed.ToString())))
                        .AddField(new EmbedFieldBuilder()
                            .WithName("Tracks")
                            .WithValue(Utils.ToUsefulLanguage(tracks.ToString())))
                        .AddField(new EmbedFieldBuilder()
                            .WithName("Vehicles")
                            .WithValue(Utils.ToUsefulLanguage(vehicles.ToString())))
                        .AddField(new EmbedFieldBuilder()
                            .WithName("Race count")
                            .WithValue(raceCount.ToString()))
                        .WithFooter("Round count: "+Convert.ToString(Program.CurrentTournament.Rounds.Count));
                        

                    await command.RespondAsync(embed: builder.Build());
                    Program.SaveToJSON();
                }
            }
        }
    }
}