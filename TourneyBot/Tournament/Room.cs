using System;
using System.Collections.Generic;
using Discord;

namespace TourneyBot.Tournament {
    [Serializable]
    public class Room {
        public List<ulong> PlayerIDs { get; set; }
        public ulong WinnerId { get; set; }

        public Room() {
            PlayerIDs = new List<ulong>();
        }
    }
}