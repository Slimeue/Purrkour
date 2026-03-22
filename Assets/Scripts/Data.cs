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
    
    public enum ObstacleType 
    {
        Normal,
        Ceiling
    }
    
    public class SaveData
    {
        public int totalPoints;
        public int totalFishCaught;
        public int bestCatchPoints;
    }

    public class Ranking
    {
        public int higherstPoints;
    }
}