using System.Collections.Generic;
using Platform;
using UnityEngine;

namespace Managers
{
    public class SpawnManager : MonoBehaviour
    {
        public static SpawnManager Instance;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void HandleSectionSpawn(PlatformInstance nextPlatform, PlatformInstance previousPlatform)
        {
            if (nextPlatform == null)
                return;

            if (FishSpawnManager.Instance == null || ObstacleSpawnManager.Instance == null)
                return;

            List<float> slotXPositions = FishSpawnManager.Instance.BuildSlotsForPlatform(nextPlatform);
            if (slotXPositions == null || slotXPositions.Count == 0)
                return;

            SpawnSectionContext context = new SpawnSectionContext(nextPlatform, slotXPositions);

            // Obstacles first, so fish can avoid them.
            ObstacleSpawnManager.Instance.HandleSectionSpawn(context);

            // Fish second, using free slots only.
            FishSpawnManager.Instance.HandleSectionSpawn(context, previousPlatform);
        }
    }
}