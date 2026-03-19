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
            RichLine,
            ArcBetweenPlatforms
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
        [SerializeField] private float arcBetweenPlatformsWeight = 30f;

        [Header("Pattern Settings")]
        [SerializeField] private float arcHeight = 2f;
        [SerializeField] private int minLineCount = 3;
        [SerializeField] private int maxLineCount = 6;
        [SerializeField] private int minRichLineCount = 6;
        [SerializeField] private int maxRichLineCount = 10;

        [Header("Between Platforms")]
        [SerializeField] private float gapEdgePadding = 0.5f;
        [SerializeField] private float rightPlatformEdgePadding = 2f;
        [SerializeField] private float leftPlatformEdgePadding = -2f;
        [SerializeField] private int minGapArcCount = 3;
        [SerializeField] private int maxGapArcCount = 6;

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
            if (fishDataArray != null)
            {
                foreach (var fishData in fishDataArray)
                {
                    if (fishData == null || fishData.prefab == null)
                        continue;

                    GenericObjectPool<FishInstance>.Prewarm(fishData.prefab, 6);
                }
            }

            RollNextCooldown();
            _sectionsSinceLastFish = 0;
        }

        public void HandleSectionSpawn(SpawnSectionContext context, PlatformInstance previousPlatform = null)
        {
            if (context == null || context.Platform == null)
                return;

            if (!ShouldSpawnFish())
                return;

            SpawnFish(context, previousPlatform);
        }

        public void SpawnFish(SpawnSectionContext context, PlatformInstance previousPlatform = null)
        {
            if (context == null || context.Platform == null)
                return;

            List<int> freeSlotIndices = context.GetFreeSlotIndices();
            if (freeSlotIndices.Count == 0)
                return;

            FishData fishData = GetWeightedRandomFishData();
            if (fishData == null || fishData.prefab == null)
                return;

            FishSpawnPattern pattern = GetRandomPattern();

            switch (pattern)
            {
                case FishSpawnPattern.Single:
                    SpawnSingle(context, fishData);
                    break;

                case FishSpawnPattern.Line:
                    SpawnLine(context, fishData);
                    break;

                case FishSpawnPattern.Arc:
                    SpawnArc(context, fishData);
                    break;

                case FishSpawnPattern.RichLine:
                    SpawnRichLine(context, fishData);
                    break;

                case FishSpawnPattern.ArcBetweenPlatforms:
                    if (previousPlatform != null)
                        SpawnFishArcBetweenPlatforms(previousPlatform, context.Platform);
                    else
                        SpawnArc(context, fishData);
                    break;
            }
        }

        public void SpawnFishArcBetweenPlatforms(PlatformInstance leftPlatform, PlatformInstance rightPlatform)
        {
            if (leftPlatform == null || rightPlatform == null)
                return;

            FishData fishData = GetWeightedRandomFishData();
            if (fishData == null || fishData.prefab == null)
                return;

            List<float> availableSlots = BuildAvailableSlotsBetweenPlatforms(leftPlatform, rightPlatform);
            if (availableSlots.Count == 0)
                return;

            int count = Mathf.Min(Random.Range(minGapArcCount, maxGapArcCount + 1), availableSlots.Count);
            if (count <= 0)
                return;

            availableSlots.Sort();

            float leftTopY = leftPlatform.transform.position.y + leftPlatform.Height * 0.5f;
            float rightTopY = rightPlatform.transform.position.y + rightPlatform.Height * 0.5f;
            float baseY = Mathf.Max(leftTopY, rightTopY) + fishHeightOffset;

            int maxStart = Mathf.Max(0, availableSlots.Count - count);
            int startIndex = Random.Range(0, maxStart + 1);

            for (int i = 0; i < count; i++)
            {
                float spawnX = availableSlots[startIndex + i];
                float t = count == 1 ? 0f : Mathf.Lerp(-1f, 1f, i / (float)(count - 1));
                float yOffset = arcHeight * (1f - (t * t));

                Vector3 spawnPosition = new Vector3(
                    spawnX,
                    baseY + yOffset,
                    leftPlatform.transform.position.z
                );

                SpawnFishInstance(fishData, spawnPosition, leftPlatform);
            }
        }

        private void SpawnSingle(SpawnSectionContext context, FishData fishData)
        {
            List<int> freeSlots = context.GetFreeSlotIndices();
            if (freeSlots.Count == 0)
                return;

            int chosenIndex = freeSlots[Random.Range(0, freeSlots.Count)];
            context.ReserveSlots(chosenIndex, 1, 0);

            float spawnX = context.SlotXPositions[chosenIndex];
            SpawnFishInstance(fishData, BuildSpawnPosition(context.Platform, spawnX, 0f), context.Platform);
        }

        private void SpawnLine(SpawnSectionContext context, FishData fishData)
        {
            int count = GetLineCount(context.GetFreeSlotIndices().Count);
            if (count <= 0)
                return;

            SpawnContiguousLine(context, fishData, count, false);
        }

        private void SpawnArc(SpawnSectionContext context, FishData fishData)
        {
            int count = GetLineCount(context.GetFreeSlotIndices().Count);
            if (count <= 0)
                return;

            SpawnContiguousLine(context, fishData, count, true);
        }

        private void SpawnRichLine(SpawnSectionContext context, FishData fishData)
        {
            int count = Mathf.Min(Random.Range(minRichLineCount, maxRichLineCount + 1), context.GetFreeSlotIndices().Count);
            if (count <= 0)
                return;

            SpawnContiguousLine(context, fishData, count, false);
        }

        private void SpawnContiguousLine(SpawnSectionContext context, FishData fishData, int count, bool useArc)
        {
            List<int> freeSlots = context.GetFreeSlotIndices();
            if (freeSlots.Count == 0 || count <= 0)
                return;

            freeSlots.Sort();
            List<int> contiguous = FindContiguousFreeRun(freeSlots, count);

            if (contiguous == null || contiguous.Count == 0)
                return;

            for (int i = 0; i < contiguous.Count; i++)
            {
                int slotIndex = contiguous[i];
                context.ReserveSlots(slotIndex, 1, 0);

                float spawnX = context.SlotXPositions[slotIndex];
                float yOffset = 0f;

                if (useArc && contiguous.Count > 1)
                {
                    float t = Mathf.Lerp(-1f, 1f, i / (float)(contiguous.Count - 1));
                    yOffset = arcHeight * (1f - (t * t));
                }

                SpawnFishInstance(
                    fishData,
                    BuildSpawnPosition(context.Platform, spawnX, yOffset),
                    context.Platform
                );
            }
        }

        private List<int> FindContiguousFreeRun(List<int> sortedFreeSlots, int requiredCount)
        {
            if (sortedFreeSlots == null || sortedFreeSlots.Count == 0 || requiredCount <= 0)
                return null;

            List<List<int>> runs = new();
            List<int> currentRun = new() { sortedFreeSlots[0] };

            for (int i = 1; i < sortedFreeSlots.Count; i++)
            {
                if (sortedFreeSlots[i] == sortedFreeSlots[i - 1] + 1)
                {
                    currentRun.Add(sortedFreeSlots[i]);
                }
                else
                {
                    if (currentRun.Count >= requiredCount)
                        runs.Add(new List<int>(currentRun));

                    currentRun.Clear();
                    currentRun.Add(sortedFreeSlots[i]);
                }
            }

            if (currentRun.Count >= requiredCount)
                runs.Add(new List<int>(currentRun));

            if (runs.Count == 0)
                return null;

            List<int> chosenRun = runs[Random.Range(0, runs.Count)];
            int maxStart = chosenRun.Count - requiredCount;
            int start = Random.Range(0, maxStart + 1);

            return chosenRun.GetRange(start, requiredCount);
        }

        private Vector3 BuildSpawnPosition(PlatformInstance platform, float spawnX, float extraY)
        {
            return new Vector3(
                spawnX,
                platform.transform.position.y + platform.Height * 0.5f + fishHeightOffset + extraY,
                platform.transform.position.z
            );
        }

        private void SpawnFishInstance(FishData fishData, Vector3 spawnPosition, PlatformInstance platform)
        {
            FishInstance fishInstance = GenericObjectPool<FishInstance>.Get(fishData.prefab, fishParent, 16);
            if (fishInstance == null)
                return;

            fishInstance.Initialize(fishData, spawnPosition, platform);
        }

        public List<float> BuildSlotsForPlatform(PlatformInstance platform)
        {
            List<float> slots = new();

            if (platform == null)
                return slots;

            float left = platform.LeftEdge + gapEdgePadding;
            float right = platform.RightEdge - gapEdgePadding;

            if (right <= left)
                return slots;

            for (float x = left; x <= right; x += fishGap)
            {
                slots.Add(x);
            }

            return slots;
        }

        private List<float> BuildAvailableSlotsBetweenPlatforms(PlatformInstance leftPlatform, PlatformInstance rightPlatform)
        {
            List<float> slots = new();

            float startX = leftPlatform.RightEdge - leftPlatformEdgePadding;
            float endX = rightPlatform.LeftEdge + rightPlatformEdgePadding;

            if (endX <= startX)
                return slots;

            for (float x = startX; x <= endX; x += fishGap)
            {
                slots.Add(x);
            }

            return slots;
        }

        private int GetLineCount(int availableSlotCount)
        {
            return Mathf.Min(Random.Range(minLineCount, maxLineCount + 1), availableSlotCount);
        }

        private FishSpawnPattern GetRandomPattern()
        {
            float totalWeight =
                singlePatternWeight +
                linePatternWeight +
                arcPatternWeight +
                richLinePatternWeight +
                arcBetweenPlatformsWeight;

            if (totalWeight <= 0f)
                return FishSpawnPattern.Single;

            float roll = Random.Range(0f, totalWeight);
            float cumulative = 0f;

            cumulative += singlePatternWeight;
            if (roll <= cumulative) return FishSpawnPattern.Single;

            cumulative += linePatternWeight;
            if (roll <= cumulative) return FishSpawnPattern.Line;

            cumulative += arcPatternWeight;
            if (roll <= cumulative) return FishSpawnPattern.Arc;

            cumulative += arcBetweenPlatformsWeight;
            if (roll <= cumulative) return FishSpawnPattern.ArcBetweenPlatforms;

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
