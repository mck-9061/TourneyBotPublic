using System;
using System.Collections.Generic;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using TourneyBot.Settings;

namespace TourneyBot.Tournament {
    [Serializable]
    public class Tournament {
        public List<Round> Rounds { get; set; }
        public List<ulong> PlayerIDs { get; set; }
        public List<ulong> RemainingPlayers { get; set; }
        public bool FinishedBuilding { get; set; }
        public bool IsRunning { get; set; }
        public ulong reactionMessageId { get; set; }
        public ulong AnnouncementChannelId { get; set; }
        public int currentRound { get; set; }
        public ulong CategoryId { get; set; }

        public Tournament() {
            Rounds = new List<Round>();
            PlayerIDs = new List<ulong>();
            FinishedBuilding = false;
            currentRound = 1;
        }
    }
}