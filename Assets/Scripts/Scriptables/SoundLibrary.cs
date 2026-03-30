using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scriptables
{
    [CreateAssetMenu(menuName = "Sounds/Sound Library", fileName = "SoundLibrary")]
    public class SoundLibrary : ScriptableObject
    {
        [Serializable]
        public class SoundEntry
        {
            public Data.SoundId Id;
            public AudioClip Clip;
            public Data.AudioCategory Category = Data.AudioCategory.SFX;
            public Data.AudioPriority Priority = Data.AudioPriority.Normal;
            [Range(0f, 1f)] public float Volume = 1f;
            [Range(0.5f, 2f)] public float Pitch = 1f;
            public float MaxAge = 0.1f;
            public bool isLoop = false;
        }

        [SerializeField] private List<SoundEntry> _entries = new();

        private Dictionary<Data.SoundId, SoundEntry> _lookup;

        private void OnEnable() => BuildLookup();

        private void BuildLookup()
        {
            _lookup = new Dictionary<Data.SoundId, SoundEntry>();
            foreach (var entry in _entries)
            {
                if (!_lookup.TryAdd(entry.Id, entry))
                    Debug.LogWarning($"[SoundLibrary] Duplicate SoundId: {entry.Id}");
            }
        }

        public bool TryGet(Data.SoundId id, out SoundEntry entry)
        {
            if (_lookup == null) BuildLookup();
            return _lookup.TryGetValue(id, out entry);
        }
    }
}