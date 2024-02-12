using System;

namespace TourneyBot {
    public class Utils {
        public static String ToUsefulLanguage(string s) {
            // yes this is shit code sue me
            switch (s) {
                case "EASY": return "Easy";
                case "NORMAL": return "Normal";
                case "HARD": return "Hard";
                case "FRANTIC": return "Frantic";
                case "NONE": return "No items";
                case "FIFTY": return "50cc";
                case "HUNDRED": return "100cc";
                case "HUNDRED_FIFTY": return "150cc";
                case "MIRROR": return "Mirror";
                case "TWO_HUNDRED": return "200cc";
                case "ALL": return "All tracks";
                case "RANDOM": return "Random tracks";
                case "NITRO": return "Original (Mushroom/Flower/Star/Crown)";
                case "RETRO": return "Retro (Shell/Banana/Leaf/Lightning)";
                case "WII_U_DLC": return "Wii U DLC (Egg/Crossing/Triforce/Bell)";
                case "SWITCH_DLC": return "Switch DLC";
                case "SPECIAL": return "Special Tracks (Crown/Lightning/Rainbow Road/Bowser's Castle)";
                case "KARTS": return "Karts";
                case "MOTORBIKES": return "Bikes";
                case "ANY": return "Any vehicle";
                default: return s;
            }
        }
    }
}