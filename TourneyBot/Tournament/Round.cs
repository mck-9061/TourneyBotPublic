using System;
using System.Collections.Generic;
using Discord;
using Discord.WebSocket;
using TourneyBot.Settings;

namespace TourneyBot.Tournament {
    [Serializable]
    public class Round {
        public AILevel AiLevel { get; set; }
        public Items Items { get; set; }
        public Speed Speed { get; set; }
        public Tracks Tracks { get; set; }
        public Vehicles Vehicles { get; set; }
        public int RaceCount { get; set; }

        public List<Room> Rooms { get; set; }
        public int RoomsLeft { get; set; }

        public Round() {
            Rooms = new List<Room>();
        }
    }
}