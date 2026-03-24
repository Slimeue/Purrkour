using System.Collections.Generic;

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

    public enum CatsType
    {
        Black,
        Siamese,
        White,
        Blue,
        Orange,
        Calico,
    }

    public class LeaderboardEntryList
    {
        public List<int> scores = new();
    }
}