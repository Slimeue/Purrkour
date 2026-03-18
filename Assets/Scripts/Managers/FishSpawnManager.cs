using System.Collections.Generic;
using Fish;
using Platform;
using Scriptables;
using Tools;
using UnityEngine;

namespace Managers
{
    public class FishSpawnManager : MonoBehaviour
    {
        public static FishSpawnManager Instance { get; private set; }

        private enum FishSpawnPattern
        {
            Single,
            Line,
            Arc,
            RichLine
        }

        [Header("Fish Data")]
        [SerializeField] private List<FishData> fishDataArray;

        [Header("General Spawn")]
        [SerializeField] private float fishGap = 2f;
        [SerializeField] private float fishHeightOffset = 1f;
        [SerializeField] private Transform fishParent;

        [Header("Fish Spawn Density")]
        [SerializeField] private int minSectionsBetweenFish = 3;
        [SerializeField] private int maxSectionsBetweenFish = 5;
        [SerializeField] private float fishSectionChance = 0.7f;

        [Header("Pattern Weights")]
        [SerializeField] private float singlePatternWeight = 20f;
        [SerializeField] private float linePatternWeight = 40f;
        [SerializeField] private float arcPatternWeight = 25f;
        [SerializeField] private float richLinePatternWeight = 15f;

        [Header("Pattern Settings")]
        [SerializeField] private float arcHeight = 2f;
        [SerializeField] private int minLineCount = 3;
        [SerializeField] private int maxLineCount = 6;
        [SerializeField] private int minRichLineCount = 6;
        [SerializeField] private int maxRichLineCount = 10;

        private int _sectionsSinceLastFish;
        private int _requiredSectionsBetweenFish;

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
            foreach (var fishData in fishDataArray)
            {
                if (fishData == null || fishData.prefab == null)
                    continue;

                GenericObjectPool<FishInstance>.Prewarm(fishData.prefab, 6);
            }

            RollNextCooldown();
            _sectionsSinceLastFish = 0;
        }

        public void HandleSectionSpawn(PlatformInstance platform)
        {
            if (platform == null)
                return;

            if (!ShouldSpawnFish())
                return;

            SpawnFish(platform);
        }

        public void SpawnFish(PlatformInstance platform)
        {
            if (platform == null)
                return;

            List<float> availableSlots = BuildAvailableSlots(platform);
            if (availableSlots.Count == 0)
                return;

            FishData fishData = GetWeightedRandomFishData();
            if (fishData == null || fishData.prefab == null)
                return;

            FishSpawnPattern pattern = GetRandomPattern();

            switch (pattern)
            {
                case FishSpawnPattern.Single:
                    SpawnSingle(platform, fishData, availableSlots);
                    break;

                case FishSpawnPattern.Line:
                    SpawnLine(platform, fishData, availableSlots);
                    break;

                case FishSpawnPattern.Arc:
                    SpawnArc(platform, fishData, availableSlots);
                    break;

                case FishSpawnPattern.RichLine:
                    SpawnRichLine(platform, fishData, availableSlots);
                    break;
            }
        }

        private void SpawnSingle(PlatformInstance platform, FishData fishData, List<float> availableSlots)
        {
            if (availableSlots.Count == 0)
                return;

            int randomSlotIndex = Random.Range(0, availableSlots.Count);
            float spawnX = availableSlots[randomSlotIndex];

            Vector3 spawnPosition = new Vector3(
                spawnX,
                platform.transform.position.y + platform.Height * 0.5f + fishHeightOffset,
                platform.transform.position.z
            );

            SpawnFishInstance(fishData, spawnPosition, platform);
        }

        private void SpawnLine(PlatformInstance platform, FishData fishData, List<float> availableSlots)
        {
            if (availableSlots.Count == 0)
                return;

            int count = Mathf.Min(Random.Range(minLineCount, maxLineCount + 1), availableSlots.Count);

            // Centered contiguous selection
            availableSlots.Sort();
            int maxStart = Mathf.Max(0, availableSlots.Count - count);
            int startIndex = Random.Range(0, maxStart + 1);

            for (int i = 0; i < count; i++)
            {
                float spawnX = availableSlots[startIndex + i];

                Vector3 spawnPosition = new Vector3(
                    spawnX,
                    platform.transform.position.y + platform.Height * 0.5f + fishHeightOffset,
                    platform.transform.position.z
                );

                SpawnFishInstance(fishData, spawnPosition, platform);
            }
        }

        private void SpawnArc(PlatformInstance platform, FishData fishData, List<float> availableSlots)
        {
            if (availableSlots.Count == 0)
                return;

            int count = Mathf.Min(Random.Range(minLineCount, maxLineCount + 1), availableSlots.Count);

            availableSlots.Sort();
            int maxStart = Mathf.Max(0, availableSlots.Count - count);
            int startIndex = Random.Range(0, maxStart + 1);

            float baseY = platform.transform.position.y + platform.Height * 0.5f + fishHeightOffset;

            for (int i = 0; i < count; i++)
            {
                float spawnX = availableSlots[startIndex + i];

                float t = count == 1 ? 0f : Mathf.Lerp(-1f, 1f, i / (float)(count - 1));
                float yOffset = arcHeight * (1f - (t * t));

                Vector3 spawnPosition = new Vector3(
                    spawnX,
                    baseY + yOffset,
                    platform.transform.position.z
                );

                SpawnFishInstance(fishData, spawnPosition, platform);
            }
        }

        private void SpawnRichLine(PlatformInstance platform, FishData fishData, List<float> availableSlots)
        {
            if (availableSlots.Count == 0)
                return;

            int count = Mathf.Min(Random.Range(minRichLineCount, maxRichLineCount + 1), availableSlots.Count);

            availableSlots.Sort();
            int maxStart = Mathf.Max(0, availableSlots.Count - count);
            int startIndex = Random.Range(0, maxStart + 1);

            for (int i = 0; i < count; i++)
            {
                float spawnX = availableSlots[startIndex + i];

                Vector3 spawnPosition = new Vector3(
                    spawnX,
                    platform.transform.position.y + platform.Height * 0.5f + fishHeightOffset,
                    platform.transform.position.z
                );

                SpawnFishInstance(fishData, spawnPosition, platform);
            }
        }

        private void SpawnFishInstance(FishData fishData, Vector3 spawnPosition, PlatformInstance platform)
        {
            FishInstance fishInstance = GenericObjectPool<FishInstance>.Get(fishData.prefab, fishParent, 16);
            if (fishInstance == null)
                return;

            fishInstance.Initialize(fishData, spawnPosition, platform);
        }

        private List<float> BuildAvailableSlots(PlatformInstance platform)
        {
            List<float> slots = new();

            float left = platform.LeftEdge;
            float right = platform.RightEdge;

            float edgePadding = 0.5f;

            float startX = left + edgePadding;
            float endX = right - edgePadding;

            if (endX <= startX)
                return slots;

            for (float x = startX; x <= endX; x += fishGap)
            {
                slots.Add(x);
            }

            return slots;
        }

        private FishSpawnPattern GetRandomPattern()
        {
            float totalWeight = singlePatternWeight + linePatternWeight + arcPatternWeight + richLinePatternWeight;
            if (totalWeight <= 0f)
                return FishSpawnPattern.Single;

            float roll = Random.Range(0f, totalWeight);
            float cumulative = 0f;

            cumulative += singlePatternWeight;
            if (roll <= cumulative)
                return FishSpawnPattern.Single;

            cumulative += linePatternWeight;
            if (roll <= cumulative)
                return FishSpawnPattern.Line;

            cumulative += arcPatternWeight;
            if (roll <= cumulative)
                return FishSpawnPattern.Arc;

            return FishSpawnPattern.RichLine;
        }

        public bool ShouldSpawnFish()
        {
            _sectionsSinceLastFish++;

            if (_sectionsSinceLastFish < _requiredSectionsBetweenFish)
                return false;

            if (Random.value > fishSectionChance)
                return false;

            _sectionsSinceLastFish = 0;
            RollNextCooldown();

            return true;
        }

        private void RollNextCooldown()
        {
            _requiredSectionsBetweenFish = Random.Range(
                minSectionsBetweenFish,
                maxSectionsBetweenFish + 1
            );
        }

        private FishData GetWeightedRandomFishData()
        {
            if (fishDataArray == null || fishDataArray.Count == 0)
                return null;

            float totalWeight = 0f;

            for (int i = 0; i < fishDataArray.Count; i++)
            {
                FishData fishData = fishDataArray[i];
                if (fishData == null || fishData.prefab == null)
                    continue;

                totalWeight += GetFishSpawnWeight(fishData);
            }

            if (totalWeight <= 0f)
                return null;

            float roll = Random.Range(0f, totalWeight);
            float cumulative = 0f;

            for (int i = 0; i < fishDataArray.Count; i++)
            {
                FishData fishData = fishDataArray[i];
                if (fishData == null || fishData.prefab == null)
                    continue;

                cumulative += GetFishSpawnWeight(fishData);

                if (roll <= cumulative)
                    return fishData;
            }

            return fishDataArray[^1];
        }

        public float GetFishSpawnCount(FishData fishData)
        {
            if (fishData == null)
                return 0f;

            return GetBaseCountByRarity(fishData.fishRarity);
        }

        private float GetBaseCountByRarity(Data.FishRarity fishDataFishRarity)
        {
            return fishDataFishRarity switch
            {
                Data.FishRarity.Common => 6f,
                Data.FishRarity.Uncommon => 4f,
                Data.FishRarity.Rare => 3f,
                Data.FishRarity.Legendary => 1f,
                _ => 0f
            };
        }

        public float GetFishSpawnWeight(FishData fishData)
        {
            if (fishData == null)
                return 0f;

            return GetBaseWeightByRarity(fishData.fishRarity) + fishData.spawnWeight;
        }

        private float GetBaseWeightByRarity(Data.FishRarity fishRarity)
        {
            return fishRarity switch
            {
                Data.FishRarity.Common => 50f,
                Data.FishRarity.Uncommon => 30f,
                Data.FishRarity.Rare => 15f,
                Data.FishRarity.Legendary => 5f,
                _ => 0f
            };
        }
    }
}
