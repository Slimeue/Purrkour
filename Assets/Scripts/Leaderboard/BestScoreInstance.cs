using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Leaderboard
{
    public class BestScoreInstance : MonoBehaviour
    {
        [FormerlySerializedAs("_bestScore")] [SerializeField] private TextMeshProUGUI bestScore;

        public void SetBestScore(int score)
        {
            if (bestScore != null)
                bestScore.text = $"{score}";
        }
        
    }
}