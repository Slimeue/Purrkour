public static class Data
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }

    public enum FishRarity
    {
        Common,
        Uncommon,
        Rare,
        Legendary
    }
    
    public enum FishSectionPattern
    {
        None,
        Single,
        Line,
        Arc,
        RichLine
    } 
    
    public class SaveData
    {
        public int totalPoints;
        public int totalFishCaught;
        public int bestCatchPoints;
        public int bestCatchRarity;
        public int bestCatchSize;
    }
}