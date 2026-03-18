using Fish;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scriptables
{
    [CreateAssetMenu(fileName = "FishData", menuName = "Scriptable Objects/FishDataSO")]
    public class FishData : ScriptableObject
    {
        public FishInstance prefab;
        public Sprite sprite;
        [FormerlySerializedAs("fishType")] public Data.FishRarity fishRarity = Data.FishRarity.Common;
        
        [Tooltip(
            "Optional value applied on top of the base points awarded for catching this fish. Higher values increase the points awarded.")]
        public float pointsValue;
        [Tooltip("Optional applied on top of the rarity-based spawn chance. Higher values increase the likelihood of this fish spawning.")]
        public float spawnWeight;
    }
}