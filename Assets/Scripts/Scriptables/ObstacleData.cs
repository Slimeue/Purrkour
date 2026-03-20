using Obstacles;
using UnityEngine;

namespace Scriptables
{
    [CreateAssetMenu(fileName = "ObstacleData", menuName = "Scriptable Objects/ObstacleDataSO")]
    public class ObstacleData : ScriptableObject
    {
        public string id;
        public ObstacleInstance prefab;

        [Header("Spawn")]
        public float spawnWeight = 1f;
        [Tooltip("Damage dealt to player on collision.")]
        public int damage = 1;

        [Tooltip("How many slots this obstacle occupies.")]
        public int slotWidth = 1;

        [Tooltip("Extra blocked slots left/right so fish do not spawn too close.")]
        public int slotPadding = 1;
    }
}
