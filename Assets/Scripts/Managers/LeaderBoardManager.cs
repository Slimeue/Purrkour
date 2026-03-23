using System;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    public class LeaderboardManager : MonoBehaviour
    {
        public static LeaderboardManager Instance { get; private set; }

        private const string LeaderboardKey = "leaderboard_top10";
        private const int MaxEntries = 10;

        private Data.LeaderboardEntryList _leaderboard = new();

        public IReadOnlyList<int> Scores => _leaderboard.scores;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            GameManager.Instance.OnEndGame += SaveLeaderboard;
        }

        public void RecordRunScore(int score)
        {
            if (score <= 0)
            {
                return;
            }

            // If leaderboard not full → always add
            if (_leaderboard.scores.Count < MaxEntries)
            {
                _leaderboard.scores.Add(score);
            }
            else
            {
                // Find the lowest score (last element since sorted descending)
                int lowestScore = _leaderboard.scores[_leaderboard.scores.Count - 1];

                // Only insert if better than lowest
                if (score > lowestScore)
                {
                    _leaderboard.scores.Add(score);
                }
                else
                {
                    // ❌ Do not add if not high enough
                    return;
                }
            }

            // Sort descending
            _leaderboard.scores.Sort((a, b) => b.CompareTo(a));

            // Trim excess
            if (_leaderboard.scores.Count > MaxEntries)
            {
                _leaderboard.scores.RemoveRange(MaxEntries, _leaderboard.scores.Count - MaxEntries);
            }

            SaveLeaderboard();
        }

        public int GetScoreAt(int index)
        {
            if (index < 0 || index >= _leaderboard.scores.Count)
                return 0;

            return _leaderboard.scores[index];
        }

        public void ClearLeaderboard()
        {
            _leaderboard.scores.Clear();
            SaveLeaderboard();
        }

        private void SaveLeaderboard()
        {
            string json = JsonUtility.ToJson(_leaderboard);
            PlayerPrefs.SetString(LeaderboardKey, json);
            PlayerPrefs.Save();
        }

        public void LoadLeaderboard()
        {
            if (!PlayerPrefs.HasKey(LeaderboardKey))
            {
                _leaderboard = new Data.LeaderboardEntryList();
                return;
            }

            string json = PlayerPrefs.GetString(LeaderboardKey);

            if (string.IsNullOrEmpty(json))
            {
                _leaderboard = new Data.LeaderboardEntryList();
                return;
            }

            _leaderboard = JsonUtility.FromJson<Data.LeaderboardEntryList>(json);

            if (_leaderboard == null || _leaderboard.scores == null)
            {
                _leaderboard = new Data.LeaderboardEntryList();
            }

            // Safety sort in case saved data got messed up
            _leaderboard.scores.Sort((a, b) => b.CompareTo(a));

            if (_leaderboard.scores.Count > MaxEntries)
            {
                _leaderboard.scores.RemoveRange(MaxEntries, _leaderboard.scores.Count - MaxEntries);
            }
        }
    }
}