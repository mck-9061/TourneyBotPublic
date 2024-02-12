using System;
using Discord;

namespace TourneyBot.Settings {
    [Serializable]
    public enum Tracks {
        ALL,
        RANDOM,
        NITRO,
        WII_U_DLC,
        RETRO,
        SPECIAL,
        SWITCH_DLC
    }
}