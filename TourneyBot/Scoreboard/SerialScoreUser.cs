using System;

namespace TourneyBot.Scoreboard {
    [Serializable]
    public class SerialScoreUser {
        public ulong DiscordId { get; set; }
        public int LifetimePoints { get; set; }
        public int RoomsWon { get; set; }
        public int TourneysWon { get; set; }

        public SerialScoreUser() {
            LifetimePoints = 0;
            RoomsWon = 0;
            TourneysWon = 0;
        }
    }
}