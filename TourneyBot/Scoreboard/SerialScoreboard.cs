using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace TourneyBot.Scoreboard {
    [Serializable]
    public class SerialScoreboard {
        public List<SerialScoreUser> Players { get; set; }
        

        public static void Load() {
            try {
                Program.Scoreboard = JsonSerializer.Deserialize<SerialScoreboard>(File.ReadAllText("Scoreboard.json"));
            }
            catch (Exception e) {
                Program.Scoreboard = new SerialScoreboard();
                Program.Scoreboard.Players = new List<SerialScoreUser>();
                Program.Scoreboard.Save();
            }
        }

        public void Save() {
            JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerOptions.Default);
            options.WriteIndented = true;
            string json = JsonSerializer.Serialize(Program.Scoreboard, options);
            File.WriteAllText("Scoreboard.json", json);
        }

        public SerialScoreUser GetPlayer(ulong id) {
            foreach (SerialScoreUser user in Players) {
                if (id == user.DiscordId) return user;
            }
            
            SerialScoreUser newUser = new SerialScoreUser();
            newUser.DiscordId = id;

            Players.Add(newUser);
            return newUser;
        }

        public Dictionary<SerialScoreUser, int> TopTenScores() {
            Dictionary<SerialScoreUser, int> dict = new Dictionary<SerialScoreUser, int>();

            foreach (SerialScoreUser user in Players) {
                dict[user] = user.LifetimePoints;
            }

            var sortedDict = from entry in dict orderby entry.Value descending select entry;
            var topTen = sortedDict.Take(10);

            return topTen.ToDictionary(pair => pair.Key, pair => pair.Value);
        }
        
        public Dictionary<SerialScoreUser, int> TopTenRooms() {
            Dictionary<SerialScoreUser, int> dict = new Dictionary<SerialScoreUser, int>();

            foreach (SerialScoreUser user in Players) {
                dict[user] = user.RoomsWon;
            }

            var sortedDict = from entry in dict orderby entry.Value descending select entry;
            var topTen = sortedDict.Take(10);

            return topTen.ToDictionary(pair => pair.Key, pair => pair.Value);
        }
        
        public Dictionary<SerialScoreUser, int> TopTenTourneys() {
            Dictionary<SerialScoreUser, int> dict = new Dictionary<SerialScoreUser, int>();

            foreach (SerialScoreUser user in Players) {
                dict[user] = user.TourneysWon;
            }

            var sortedDict = from entry in dict orderby entry.Value descending select entry;
            var topTen = sortedDict.Take(10);

            return topTen.ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }
}