using System.Collections.Generic;
using Scriptables;
using Sounds;
using Tools;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private SoundInstance soundInstancePrefab;
    [SerializeField] private Transform poolParent;
    [SerializeField] private int prewarmCount = 10;
    [SerializeField] private int requestsProcessedPerFrame = 3;
    [SerializeField] private SoundLibrary soundLibrary;

    private SoundInstance _bgmSoundInstance;

    // Add this field
    private readonly Dictionary<Data.AudioCategory, float> _categoryVolumes = new()
    {
        { Data.AudioCategory.SFX, 1f },
        { Data.AudioCategory.BGM, 1f },
        { Data.AudioCategory.UI, 1f },
        { Data.AudioCategory.Voice, 1f }
    };

    private readonly Dictionary<Data.AudioCategory, float> _cooldowns = new()
    {
        { Data.AudioCategory.SFX, 0.05f },
        { Data.AudioCategory.Music, 0f },
        { Data.AudioCategory.UI, 0.03f },
        { Data.AudioCategory.Voice, 0.2f }
    };

    private readonly Dictionary<string, float> _lastPlayed = new();

    private readonly Queue<Data.AudioRequest> _queue = new();
    public static AudioManager Instance { get; private set; }


    // ── Lifecycle ──────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        GenericObjectPool<SoundInstance>.Prewarm(
            soundInstancePrefab,
            prewarmCount,
            poolParent
        );
    }

    private void Update()
    {
        var processed = 0;

        while (_queue.Count > 0 && processed < requestsProcessedPerFrame)
        {
            var req = _queue.Dequeue();
            processed++;

            if (ShouldDiscard(req)) continue;
            if (IsDuplicate(req)) continue;

            Play(req);
        }
    }

    // ── Public API ─────────────────────────────────────────────
    public void Request(Data.SoundId id)
    {
        if (!soundLibrary.TryGet(id, out var entry))
        {
            Debug.LogWarning($"[AudioManager] SoundId not found: {id}");
            return;
        }

        Request(new Data.AudioRequest
        {
            SoundId = entry.Id,
            Clip = entry.Clip,
            Category = entry.Category,
            Priority = entry.Priority,
            Volume = entry.Volume,
            Pitch = entry.Pitch,
            MaxAge = entry.MaxAge,
            DedupeKey = id.ToString(), // auto dedup per SoundId
            isLoop = entry.isLoop
        });
    }

    public void Request(Data.AudioRequest request)
    {
        request.RequestedAt = Time.time;
        _queue.Enqueue(request);
    }

    public void PlayBgm(Data.SoundId soundId)
    {
        if (_bgmSoundInstance != null)
            _bgmSoundInstance.Stop();

        Request(soundId);
    }
    // ── Decision Logic ─────────────────────────────────────────

    // Add this method
    public void SetCategoryVolume(Data.AudioCategory category, float volume)
    {
        _categoryVolumes[category] = Mathf.Clamp01(volume);

        // If BGM is playing, update it live
        if (category == Data.AudioCategory.BGM && _bgmSoundInstance != null)
            _bgmSoundInstance.SetVolume(_categoryVolumes[category]);
    }

    private bool ShouldDiscard(Data.AudioRequest req)
    {
        return req.Clip == null ||
               Time.time - req.RequestedAt > req.MaxAge;
    }

    private bool IsDuplicate(Data.AudioRequest req)
    {
        if (string.IsNullOrEmpty(req.DedupeKey)) return false;

        var cooldown = _cooldowns.GetValueOrDefault(req.Category, 0.05f);

        return _lastPlayed.TryGetValue(req.DedupeKey, out var lastTime)
               && Time.time - lastTime < cooldown;
    }

    private void Play(Data.AudioRequest req)
    {
        var instance = GenericObjectPool<SoundInstance>.Get(
            soundInstancePrefab,
            poolParent
        );

        if (req.Category == Data.AudioCategory.BGM)
            _bgmSoundInstance = instance;

        // Scale clip volume by category volume
        var categoryVolume = _categoryVolumes.GetValueOrDefault(req.Category, 1f);
        req.Volume *= categoryVolume;

        instance.Play(req);

        if (!string.IsNullOrEmpty(req.DedupeKey))
            _lastPlayed[req.DedupeKey] = Time.time;
    }
}