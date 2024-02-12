using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using TourneyBot.Scoreboard;
using TourneyBot.Tournament;

namespace TourneyBot.Listeners {
    public class ScoreSubmitListener {
        public static async Task ModalListener(SocketModal modal) {
            List<SocketMessageComponentData> components = modal.Data.Components.ToList();
            
            
            int roomNo = Convert.ToInt32(modal.Channel.Name.Split('-')[1]) - 1;
            Round currentRound = Program.CurrentTournament.Rounds[Program.CurrentTournament.currentRound - 1];
            Room room = currentRound.Rooms[roomNo];

            string winner = "";
            int highest = 0;

            try {
                foreach (SocketMessageComponentData data in components) {
                    if (Convert.ToInt32(data.Value) > highest) {
                        winner = data.CustomId;
                        highest = Convert.ToInt32(data.Value);
                    }

                    SerialScoreUser user = Program.Scoreboard.GetPlayer(Convert.ToUInt64(data.CustomId));
                    user.LifetimePoints += Convert.ToInt32(data.Value);
                    Program.Scoreboard.Save();
                }
                
                SerialScoreUser userW = Program.Scoreboard.GetPlayer(Convert.ToUInt64(winner));
                userW.RoomsWon++;
                Program.Scoreboard.Save();
                
                foreach (SocketMessageComponentData data in components) {
                    if (data.CustomId != winner) {
                        if (data.CustomId.StartsWith("123456"))
                            Program.CurrentTournament.RemainingPlayers.Remove(123456789);
                        else Program.CurrentTournament.RemainingPlayers.Remove(Convert.ToUInt64(data.CustomId));
                    }
                }
                
                var guild = Program.Client.GetGuild(Program.GuildId);

                if (winner.StartsWith("123456")) {
                    await modal.RespondAsync("Submitted, removing channel...");
                    await ((SocketTextChannel) guild.GetChannel(Program.CurrentTournament.AnnouncementChannelId))
                        .SendMessageAsync($"A dummy is the winner of room {modal.Data.CustomId.Split('-')[1]} for round {Program.CurrentTournament.currentRound}!");
                }
                else {
                    IUser user = guild.GetUser(Convert.ToUInt64(winner));
                    await modal.RespondAsync("Submitted, removing channel...");
                
                    await ((SocketTextChannel) guild.GetChannel(Program.CurrentTournament.AnnouncementChannelId))
                        .SendMessageAsync($"{user.Username} is the winner of room {modal.Data.CustomId.Split('-')[1]} for round {Program.CurrentTournament.currentRound}!");
                }
                
                await ((SocketTextChannel) modal.Channel).DeleteAsync();
                room.WinnerId = Convert.ToUInt64(winner);
                currentRound.RoomsLeft--;
                Program.SaveToJSON();

                if (currentRound.RoomsLeft == 0) {
                    Program.CurrentTournament.currentRound++;
                    // Begin the next round
                    if (Program.CurrentTournament.RemainingPlayers.Count == 1 || Program.CurrentTournament.currentRound > Program.CurrentTournament.Rounds.Count) {
                        ulong winnerId = Program.CurrentTournament.RemainingPlayers[0];
                        SocketUser user = guild.GetUser(winnerId);
                        // The tournament is over
                        await ((SocketTextChannel) guild.GetChannel(Program.CurrentTournament.AnnouncementChannelId))
                            .SendMessageAsync($"{user.Username} wins the tournament!");
                        
                        SerialScoreUser userT = Program.Scoreboard.GetPlayer(winnerId);
                        userT.TourneysWon++;
                        Program.Scoreboard.Save();
                        
                        // Clean tournament
                        Program.SaveToJSON();
                        string identifier = DateTime.Now.ToLongTimeString().Replace(":", "") +
                                            DateTime.Now.ToShortDateString().Replace("/", "");
                        File.Copy("Tourney.json", $"old/tourney-{identifier}.json");
                
                
                        Program.CurrentTournament = null;
                        Program.SaveToJSON();

                        foreach (SocketRole role in guild.Roles) {
                            try {
                                if (role.Name.Contains("Tourney:")) await role.DeleteAsync();
                            }
                            catch (Exception e) {
                                await modal.Channel.SendMessageAsync(
                                    "Failed to delete a role. Please delete manually.");
                            }
                        }

                        foreach (SocketCategoryChannel category in guild.CategoryChannels) {
                            if (category.Name == "tournament") await category.DeleteAsync();
                        }
                        
                    }
                    else {
                        foreach (SocketRole role in guild.Roles) {
                            try {
                                if (role.Name.Contains("Tourney:")) await role.DeleteAsync();
                            }
                            catch (Exception e) {
                                await modal.Channel.SendMessageAsync(
                                    "Failed to delete a role. Please delete manually.");
                            }
                        }
                        
                        Round round = Program.CurrentTournament.Rounds[Program.CurrentTournament.currentRound-1];
                        int _roomNo = 0;
                        // Sort players
                        foreach (ulong id in Program.CurrentTournament.RemainingPlayers) {
                            if (_roomNo == round.Rooms.Count) _roomNo = 0;
                            Room _room = round.Rooms[_roomNo];
                            _room.PlayerIDs.Add(id);
                            Console.WriteLine($"Added {id} to room {_roomNo}");
                            _roomNo++;
                        }
                        
                        // Round channels/roles
                        int k = 0;
                        foreach (Room _room in round.Rooms) {
                            RestTextChannel channel = await guild.CreateTextChannelAsync($"Room {k + 1}", tcp => tcp.CategoryId = Program.CurrentTournament.CategoryId);
                            RestRole role = await guild.CreateRoleAsync($"Tourney: Room {k + 1}");

                            await channel.AddPermissionOverwriteAsync(guild.EveryoneRole,
                                new OverwritePermissions(viewChannel: PermValue.Allow, sendMessages: PermValue.Deny));
                            
                            await channel.AddPermissionOverwriteAsync(role,
                                new OverwritePermissions(viewChannel: PermValue.Allow, sendMessages: PermValue.Allow));

                            foreach (ulong id in _room.PlayerIDs) {
                                if (id.ToString().StartsWith("123456789")) continue;
                                IUser player = guild.GetUser(id);
                                await guild.GetUser(player.Id).AddRoleAsync(role);
                            }
                            k++;
                            Console.WriteLine("Channel and role made");
                        }
                        Console.WriteLine("All channels done, building message");
                        
                        EmbedBuilder embed = new EmbedBuilder().WithTitle($"Round {Program.CurrentTournament.currentRound}: Players").WithFooter("Developed by mck");;
                        
                        Console.WriteLine("Embed made");

                        int l = 0;
                        int dummy = 1;
                        foreach (Room _room in round.Rooms) {
                            l++;
                            string players = "";
                            foreach (ulong id in _room.PlayerIDs) {
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

                        round.RoomsLeft = round.Rooms.Count;
                        await guild.GetTextChannel(Program.CurrentTournament.AnnouncementChannelId).SendMessageAsync(embed: embed.Build());
                        Program.SaveToJSON();
                    }
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
            
        }
    }
}