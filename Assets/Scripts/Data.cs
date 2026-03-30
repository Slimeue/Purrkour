using System.Collections.Generic;
using UnityEngine;

public static class Data
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver,
        Intro,
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
        Orange,
        Black,
        Siamese,
        White,
        Blue,
        Calico,
    }

    public class LeaderboardEntryList
    {
        public List<int> scores = new();
    }

    public enum AudioPriority
    {
        Low,
        Normal,
        High,
        Critical
    }

    public enum AudioCategory
    {
        SFX,
        Music,
        UI,
        Voice,
        BGM
    }

    public class AudioRequest
    {
        public SoundId SoundId;
        public AudioClip Clip;
        public AudioCategory Category;
        public AudioPriority Priority;
        public float Volume = 1f;
        public float Pitch = 1f;
        public float RequestedAt; // Time.time when queued
        public string DedupeKey; // e.g. "coin", "zombie_groan" — same key = deduplicated
        public float MaxAge; // discard if still queued after this many seconds
        public bool isLoop = false;
    }

    public enum SoundId
    {
        // Coins
        CoinCollectNormal,
        CoinCollectRare,

        // Player
        PlayerJump,
        PlayerDeath,
        CharacterSelection,

        // UI
        GameScene,
        ButtonClick,
        GameOver,
        Intro,
        MainMenu,
        Tutorial,
        CharacterChoose,
    }
}