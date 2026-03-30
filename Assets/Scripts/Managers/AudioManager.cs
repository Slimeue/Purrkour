using System.Collections.Generic;
using Scriptables;
using UnityEngine;
using Sounds;
using Tools;
using UI;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private SoundInstance soundInstancePrefab;
    [SerializeField] private Transform poolParent;
    [SerializeField] private int prewarmCount = 10;
    [SerializeField] private int requestsProcessedPerFrame = 3;
    [SerializeField] private SoundLibrary soundLibrary;

    private SoundInstance _bgmSoundInstance;

    private Queue<Data.AudioRequest> _queue = new();
    private Dictionary<string, float> _lastPlayed = new();

    private Dictionary<Data.AudioCategory, float> _cooldowns = new()
    {
        { Data.AudioCategory.SFX, 0.05f },
        { Data.AudioCategory.Music, 0f },
        { Data.AudioCategory.UI, 0.03f },
        { Data.AudioCategory.Voice, 0.2f },
    };

    // ── Lifecycle ──────────────────────────────────────────────

    void Awake()
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

    void Update()
    {
        int processed = 0;

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
            isLoop = entry.isLoop,
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

    private bool ShouldDiscard(Data.AudioRequest req)
    {
        return req.Clip == null ||
               (Time.time - req.RequestedAt) > req.MaxAge;
    }

    private bool IsDuplicate(Data.AudioRequest req)
    {
        if (string.IsNullOrEmpty(req.DedupeKey)) return false;

        float cooldown = _cooldowns.GetValueOrDefault(req.Category, 0.05f);

        return _lastPlayed.TryGetValue(req.DedupeKey, out float lastTime)
               && (Time.time - lastTime) < cooldown;
    }

    private void Play(Data.AudioRequest req)
    {
        // Get a SoundInstance from the pool
        var instance = GenericObjectPool<SoundInstance>.Get(
            soundInstancePrefab,
            poolParent
        );

        if (req.Category == Data.AudioCategory.BGM)
        {
            _bgmSoundInstance = instance;
        }
        
        instance.Play(req);

        if (!string.IsNullOrEmpty(req.DedupeKey))
            _lastPlayed[req.DedupeKey] = Time.time;
    }
}