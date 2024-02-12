using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;
using TourneyBot.Settings;
using TourneyBot.Tournament;

namespace TourneyBot.Commands {
    public class StartTournament {
        public static async Task Client_Ready() {
            var guild = Program.Client.GetGuild(Program.GuildId);
            var command = new SlashCommandBuilder();
            command.WithName("start");
            command.WithDescription("Start the current tournament");
            
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
            if (command.Data.Name == "start") {
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
                
                
                
                if (!Program.CurrentTournament.FinishedBuilding) {
                    await command.RespondAsync("Tournament not announced!");
                    return;
                }
                await command.RespondAsync("Started!");

                RestCategoryChannel category = await guild.CreateCategoryChannelAsync("tournament");
                Program.CurrentTournament.CategoryId = category.Id;

                await category.ModifyAsync(x => x.Position = 4);
                
                IMessageChannel announcementChannel = (IMessageChannel)guild.GetChannel(Program.CurrentTournament.AnnouncementChannelId);

                IMessage message =
                    await announcementChannel.GetMessageAsync(Program.CurrentTournament.reactionMessageId);

                var reactorsAsync = message.GetReactionUsersAsync(new Emoji("✋"), 100);
                var reactors = await reactorsAsync.Flatten().ToArrayAsync();

                foreach (IUser user in reactors) {
                    if (user.IsBot) continue;
                    Program.CurrentTournament.PlayerIDs.Add(user.Id);
                }

                int roomCount = 0;
                int playerCount = Program.CurrentTournament.PlayerIDs.Count;
                int roundCount = Program.CurrentTournament.Rounds.Count;
                int i = 0;

                foreach (Round round in Program.CurrentTournament.Rounds) {
                    i++;
                    if (playerCount < 3) roomCount = 1;
                    else if (playerCount < 6) roomCount = 2;
                    else if (playerCount < 8) roomCount = 3;
                    else if (playerCount < 12) roomCount = 4;
                    else if (playerCount < 18) roomCount = 5;
                    else if (playerCount < 23) roomCount = 6;
                    else roomCount = 7;

                    for (int j = 0; j < roomCount; j++) {
                        Room room = new Room();
                        round.Rooms.Add(room);
                    }

                    if (i == roundCount) roomCount = 1;
                    if (i == roundCount - 1) roomCount = 2;
                    
                    playerCount = roomCount;
                }

                Round round1 = Program.CurrentTournament.Rounds[0];
                int roomNo = 0;
                // Sort players
                foreach (ulong id in Program.CurrentTournament.PlayerIDs) {
                    if (roomNo == round1.Rooms.Count) roomNo = 0;
                    Room room = round1.Rooms[roomNo];
                    room.PlayerIDs.Add(id);

                    roomNo++;
                }
                
                // Round 1 channels/roles
                int k = 0;
                foreach (Room room in round1.Rooms) {
                    RestTextChannel channel = await guild.CreateTextChannelAsync($"Room {k + 1}", tcp => tcp.CategoryId = category.Id);
                    RestRole role = await guild.CreateRoleAsync($"Tourney: Room {k + 1}");

                    await channel.AddPermissionOverwriteAsync(guild.EveryoneRole,
                        new OverwritePermissions(viewChannel: PermValue.Allow, sendMessages: PermValue.Deny));
                    
                    await channel.AddPermissionOverwriteAsync(role,
                        new OverwritePermissions(viewChannel: PermValue.Allow, sendMessages: PermValue.Allow));

                    foreach (ulong id in room.PlayerIDs) {
                        if (id.ToString().StartsWith("123456789")) continue;
                        IUser player = guild.GetUser(id);
                        await guild.GetUser(player.Id).AddRoleAsync(role);
                    }
                    k++;
                    Console.WriteLine("Channel and role made");
                }
                Console.WriteLine("All channels done, building message");
                
                EmbedBuilder embed = new EmbedBuilder().WithTitle("Round 1: Players").WithFooter("Developed by mck");;
                
                Console.WriteLine("Embed made");

                int l = 0;
                int dummy = 1;
                foreach (Room room in round1.Rooms) {
                    l++;
                    string players = "";
                    foreach (ulong id in room.PlayerIDs) {
                        if (id.ToString().StartsWith("123456789")) {
                            players = players + $"Dummy User {dummy}, ";
                            dummy++;
                            continue;
                        }
                        players = players + guild.GetUser(id).Username + ", ";
                    }

                    embed.AddField($"Room {l}", players);
                    Console.WriteLine("Field added");
                }

                round1.RoomsLeft = round1.Rooms.Count;
                Program.CurrentTournament.IsRunning = true;
                Program.CurrentTournament.RemainingPlayers = new List<ulong>(Program.CurrentTournament.PlayerIDs);
                await message.Channel.SendMessageAsync(embed: embed.Build());
                Program.SaveToJSON();
            }
        }
    }
}