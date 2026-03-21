using System.Collections.Generic;
using Obstacles;
using Platform;
using Scriptables;
using Tools;
using UnityEngine;

namespace Managers
{
    public class ObstacleSpawnManager : MonoBehaviour
    {
        public static ObstacleSpawnManager Instance { get; private set; }

        [Header("Obstacle Data")]
        [SerializeField] private List<ObstacleData> obstacleDataArray;

        [Header("Spawn")]
        [SerializeField] private Transform obstacleParent;
        [SerializeField] private float obstacleHeightOffset = 0f;
        [SerializeField] private float obstacleSectionChance = 0.55f;
        [SerializeField] private int maxObstaclesPerSection = 1;
        
        public List<ObstacleInstance> obstacleInstances = new List<ObstacleInstance>();

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
            if (obstacleDataArray == null)
                return;

            GameManager.Instance.OnRestartGame += ClearObstacles;

            foreach (var obstacleData in obstacleDataArray)
            {
                if (obstacleData == null || obstacleData.prefab == null)
                    continue;

                GenericObjectPool<ObstacleInstance>.Prewarm(obstacleData.prefab, 6);
            }
        }

        public void HandleSectionSpawn(SpawnSectionContext context)
        {
            if (context == null || context.Platform == null)
                return;

            if (Random.value > obstacleSectionChance)
                return;

            int obstacleCount = Random.Range(1, maxObstaclesPerSection + 1);

            for (int i = 0; i < obstacleCount; i++)
            {
                SpawnObstacle(context);
            }
        }

        private void SpawnObstacle(SpawnSectionContext context)
        {
            ObstacleData obstacleData = GetWeightedRandomObstacleData();
            if (obstacleData == null || obstacleData.prefab == null)
                return;

            List<int> validStartIndices = new();

            for (int i = 0; i < context.SlotXPositions.Count; i++)
            {
                if (context.CanReserveSlots(i, obstacleData.slotWidth, obstacleData.slotPadding))
                {
                    validStartIndices.Add(i);
                }
            }

            if (validStartIndices.Count == 0)
                return;

            int chosenStartIndex = validStartIndices[Random.Range(0, validStartIndices.Count)];
            context.ReserveSlots(chosenStartIndex, obstacleData.slotWidth, obstacleData.slotPadding);

            float startX = context.SlotXPositions[chosenStartIndex];
            int endIndex = Mathf.Min(chosenStartIndex + obstacleData.slotWidth - 1, context.SlotXPositions.Count - 1);
            float endX = context.SlotXPositions[endIndex];
            float centerX = (startX + endX) * 0.5f;

            PlatformInstance platform = context.Platform;
            Vector3 spawnPosition = new Vector3(
                centerX,
                platform.transform.position.y + platform.Height * 0.5f + obstacleHeightOffset,
                platform.transform.position.z
            );

            ObstacleInstance instance =
                GenericObjectPool<ObstacleInstance>.Get(obstacleData.prefab, obstacleParent, 16);

            if (instance == null)
                return;

            instance.Initialize(obstacleData, spawnPosition, platform);
            obstacleInstances.Add(instance);
        }

        private ObstacleData GetWeightedRandomObstacleData()
        {
            if (obstacleDataArray == null || obstacleDataArray.Count == 0)
                return null;

            float totalWeight = 0f;

            for (int i = 0; i < obstacleDataArray.Count; i++)
            {
                ObstacleData data = obstacleDataArray[i];
                if (data == null || data.prefab == null)
                    continue;

                totalWeight += Mathf.Max(0f, data.spawnWeight);
            }

            if (totalWeight <= 0f)
                return null;

            float roll = Random.Range(0f, totalWeight);
            float cumulative = 0f;

            for (int i = 0; i < obstacleDataArray.Count; i++)
            {
                ObstacleData data = obstacleDataArray[i];
                if (data == null || data.prefab == null)
                    continue;

                cumulative += Mathf.Max(0f, data.spawnWeight);

                if (roll <= cumulative)
                    return data;
            }

            return obstacleDataArray[^1];
        }
        
        public void ClearObstacles()
        {
            foreach (var obstacle in obstacleInstances)
            {
                if (obstacle != null)
                    GenericObjectPool<ObstacleInstance>.Release(obstacle);
            }

            obstacleInstances.Clear();
        }
    }
}
