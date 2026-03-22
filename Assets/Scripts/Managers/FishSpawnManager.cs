using System.Collections.Generic;
using System.Linq;
using Fish;
using Platform;
using Scriptables;
using Tools;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Managers
{
    public class FishSpawnManager : MonoBehaviour
    {
        [Header("Fish Data")] [SerializeField] private List<FishData> fishDataArray;

        [Header("General Spawn")] [SerializeField]
        private float fishGap = 2f;

        [SerializeField] private float fishHeightOffset = 1f;
        [SerializeField] private Transform fishParent;

        [Header("Fish Spawn Density")] [SerializeField]
        private int minSectionsBetweenFish = 3;

        [SerializeField] private int maxSectionsBetweenFish = 5;
        [SerializeField] private float fishSectionChance = 0.7f;

        [Header("Pattern Weights")] [SerializeField]
        private float singlePatternWeight = 20f;

        [SerializeField] private float linePatternWeight = 40f;
        [SerializeField] private float arcPatternWeight = 25f;
        [SerializeField] private float richLinePatternWeight = 15f;
        [SerializeField] private float arcBetweenPlatformsWeight = 30f;

        [Header("Pattern Settings")] [SerializeField]
        private float arcHeight = 2f;

        [SerializeField] private int minLineCount = 3;
        [SerializeField] private int maxLineCount = 6;
        [SerializeField] private int minRichLineCount = 6;
        [SerializeField] private int maxRichLineCount = 10;

        [Header("Between Platforms")] [SerializeField]
        private float gapEdgePadding = 0.5f;

        [SerializeField] private float rightPlatformEdgePadding = 2f;
        [SerializeField] private float leftPlatformEdgePadding = -2f;
        [SerializeField] private int minGapArcCount = 3;
        [SerializeField] private int maxGapArcCount = 6;

        [Header("Debug")] [SerializeField] private bool debugDrawSlots = true;

        [SerializeField] private float debugSlotRadius = 0.15f;
        private readonly List<(PlatformInstance left, PlatformInstance right, Vector3 localPos)> _debugGapSlots = new();
        private readonly Dictionary<PlatformInstance, List<Vector3>> _debugPlatformSlots = new();
        private int _requiredSectionsBetweenFish;

        private int _sectionsSinceLastFish;
        
        [FormerlySerializedAs("_activeFishInstances")] public List<FishInstance> activeFishInstances = new();
        
        public static FishSpawnManager Instance { get; private set; }

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
                foreach (var fishData in fishDataArray)
                {
                    if (fishData == null || fishData.prefab == null)
                        continue;

                    GenericObjectPool<FishInstance>.Prewarm(fishData.prefab, 6);
                }

            RollNextCooldown();
            _sectionsSinceLastFish = 0;
            GameManager.Instance.OnRestartGame += Restart;
        }

        private void Update()
        {

            if (DebuggerManager.Instance.showLogs)
            {
                
                if (Camera.main == null) return;

                var camX = Camera.main.transform.position.x;

                foreach (var pair in _debugPlatformSlots.ToList())
                {
                    var platform = pair.Key;

                    if (platform == null)
                    {
                        _debugPlatformSlots.Remove(platform);
                        continue;
                    }

                    if (platform.transform.position.x < camX - 10f) // buffer
                        _debugPlatformSlots.Remove(platform);
                }
            }
            
        }

        private void OnDrawGizmos()
        {
            if (!debugDrawSlots)
                return;

            // PLATFORM SLOTS
            Gizmos.color = Color.cyan;

            foreach (var slot in _debugPlatformSlots)
            {
                var hasList = _debugPlatformSlots.TryGetValue(slot.Key, out var list);

                if(!hasList)
                    continue;
                
                for (var i = 0; i < list.Count; i++)
                {
                    var localPos = list[i];


                    var worldPos = slot.Key.transform.position + localPos;

                    Gizmos.DrawSphere(worldPos, debugSlotRadius);
                    Gizmos.DrawLine(worldPos, worldPos + Vector3.up * 0.3f);

#if UNITY_EDITOR
                    Handles.Label(worldPos + Vector3.up * 0.4f, $"P{i}");
#endif
                }
            }

            // GAP SLOTS
            Gizmos.color = Color.magenta;

            for (var i = 0; i < _debugGapSlots.Count; i++)
            {
                var (leftPlatform, rightPlatform, localPos) = _debugGapSlots[i];

                if (leftPlatform == null)
                    continue;

                var worldPos = leftPlatform.transform.position + localPos;

                Gizmos.DrawSphere(worldPos, debugSlotRadius);
                Gizmos.DrawLine(worldPos, worldPos + Vector3.up * 0.3f);

#if UNITY_EDITOR
                Handles.Label(worldPos + Vector3.up * 0.4f, $"G{i}");
#endif
            }
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
            if (context == null || context.Platform == null || context.Platform.CurrentPieceData.isCeiling)
                return;

            var freeSlotIndices = context.GetFreeSlotIndices();
            if (freeSlotIndices.Count == 0)
                return;

            var fishData = GetWeightedRandomFishData();
            if (fishData == null || fishData.prefab == null)
                return;
            
            _sectionsSinceLastFish = 0;

            var pattern = GetRandomPattern();

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

            var fishData = GetWeightedRandomFishData();
            if (fishData == null || fishData.prefab == null)
                return;

            var availableSlots = BuildAvailableSlotsBetweenPlatforms(leftPlatform, rightPlatform);
            if (availableSlots.Count == 0)
                return;

            var count = Mathf.Min(Random.Range(minGapArcCount, maxGapArcCount + 1), availableSlots.Count);
            if (count <= 0)
                return;

            availableSlots.Sort();

            var leftTopY = leftPlatform.transform.position.y + leftPlatform.Height * 0.5f;
            var rightTopY = rightPlatform.transform.position.y + rightPlatform.Height * 0.5f;
            var baseY = Mathf.Max(leftTopY, rightTopY) + fishHeightOffset;

            var maxStart = Mathf.Max(0, availableSlots.Count - count);
            var startIndex = Random.Range(0, maxStart + 1);

            for (var i = 0; i < count; i++)
            {
                var spawnX = availableSlots[startIndex + i];
                var t = count == 1 ? 0f : Mathf.Lerp(-1f, 1f, i / (float)(count - 1));
                var yOffset = arcHeight * (1f - t * t);

                var spawnPosition = new Vector3(
                    spawnX,
                    baseY + yOffset,
                    leftPlatform.transform.position.z
                );

                SpawnFishInstance(fishData, spawnPosition, leftPlatform);
            }
        }

        private void SpawnSingle(SpawnSectionContext context, FishData fishData)
        {
            var freeSlots = context.GetFreeSlotIndices();
            if (freeSlots.Count == 0)
                return;

            var chosenIndex = freeSlots[Random.Range(0, freeSlots.Count)];
            context.ReserveSlots(chosenIndex, 1);

            var spawnX = context.SlotXPositions[chosenIndex];
            SpawnFishInstance(fishData, BuildSpawnPosition(context.Platform, spawnX, 0f), context.Platform);
        }

        private void SpawnLine(SpawnSectionContext context, FishData fishData)
        {
            var count = GetLineCount(context.GetFreeSlotIndices().Count);
            if (count <= 0)
                return;

            SpawnContiguousLine(context, fishData, count, false);
        }

        private void SpawnArc(SpawnSectionContext context, FishData fishData)
        {
            var count = GetLineCount(context.GetFreeSlotIndices().Count);
            if (count <= 0)
                return;

            SpawnContiguousLine(context, fishData, count, true);
        }

        private void SpawnRichLine(SpawnSectionContext context, FishData fishData)
        {
            var count = Mathf.Min(Random.Range(minRichLineCount, maxRichLineCount + 1),
                context.GetFreeSlotIndices().Count);
            if (count <= 0)
                return;

            SpawnContiguousLine(context, fishData, count, false);
        }

        private void SpawnContiguousLine(SpawnSectionContext context, FishData fishData, int count, bool useArc)
        {
            var freeSlots = context.GetFreeSlotIndices();
            if (freeSlots.Count == 0 || count <= 0)
                return;

            freeSlots.Sort();
            var contiguous = FindContiguousFreeRun(freeSlots, count);

            if (contiguous == null || contiguous.Count == 0)
                return;

            for (var i = 0; i < contiguous.Count; i++)
            {
                var slotIndex = contiguous[i];
                context.ReserveSlots(slotIndex, 1);

                var spawnX = context.SlotXPositions[slotIndex];
                var yOffset = 0f;

                if (useArc && contiguous.Count > 1)
                {
                    var t = Mathf.Lerp(-1f, 1f, i / (float)(contiguous.Count - 1));
                    yOffset = arcHeight * (1f - t * t);
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

            for (var i = 1; i < sortedFreeSlots.Count; i++)
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

            if (currentRun.Count >= requiredCount)
                runs.Add(new List<int>(currentRun));

            if (runs.Count == 0)
                return null;

            var chosenRun = runs[Random.Range(0, runs.Count)];
            var maxStart = chosenRun.Count - requiredCount;
            var start = Random.Range(0, maxStart + 1);

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
            var fishInstance = GenericObjectPool<FishInstance>.Get(fishData.prefab, fishParent);
            if (fishInstance == null)
                return;

            fishInstance.Initialize(fishData, spawnPosition, platform);
            activeFishInstances.Add(fishInstance);
        }

        public List<float> BuildSlotsForPlatform(PlatformInstance platform)
        {
            List<float> slots = new();

            if (platform == null)
                return slots;

            // _debugPlatformSlots.Clear();

            var left = platform.LeftEdge + gapEdgePadding;
            var right = platform.RightEdge - gapEdgePadding;

            if (right <= left)
                return slots;

            var y = platform.Height * 0.5f + fishHeightOffset;

            List<Vector3> localList = new();

            for (var x = left; x <= right; x += fishGap)
            {
                slots.Add(x);

                var localX = x - platform.transform.position.x;
                var localPos = new Vector3(localX, y, 0f);
                
                localList.Add(localPos);

            }
            _debugPlatformSlots[platform] = localList;
            
            return slots;
        }

        private List<float> BuildAvailableSlotsBetweenPlatforms(PlatformInstance leftPlatform,
            PlatformInstance rightPlatform)
        {
            List<float> slots = new();

            if (leftPlatform == null || rightPlatform == null)
                return slots;

            // _debugGapSlots.Clear();

            var startX = leftPlatform.RightEdge - leftPlatformEdgePadding;
            var endX = rightPlatform.LeftEdge + rightPlatformEdgePadding;

            if (endX <= startX)
                return slots;

            var leftTopY = leftPlatform.Height * 0.5f;
            var rightTopY = rightPlatform.Height * 0.5f;
            var baseY = Mathf.Max(leftTopY, rightTopY) + fishHeightOffset;

            for (var x = startX; x <= endX; x += fishGap)
            {
                slots.Add(x);

                var localX = x - leftPlatform.transform.position.x;
                var localPos = new Vector3(localX, baseY, 0f);

                _debugGapSlots.Add((leftPlatform, rightPlatform, localPos));
            }

            return slots;
        }

        private int GetLineCount(int availableSlotCount)
        {
            return Mathf.Min(Random.Range(minLineCount, maxLineCount + 1), availableSlotCount);
        }

        private FishSpawnPattern GetRandomPattern()
        {
            var totalWeight =
                singlePatternWeight +
                linePatternWeight +
                arcPatternWeight +
                richLinePatternWeight +
                arcBetweenPlatformsWeight;

            if (totalWeight <= 0f)
                return FishSpawnPattern.Single;

            var roll = Random.Range(0f, totalWeight);
            var cumulative = 0f;

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

            // Commented this to ensure fish always spawn after cooldown, but can re-enable for extra randomness if needed
            // if (Random.value > fishSectionChance)
            //     return false;

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

            var totalWeight = 0f;

            for (var i = 0; i < fishDataArray.Count; i++)
            {
                var fishData = fishDataArray[i];
                if (fishData == null || fishData.prefab == null)
                    continue;

                totalWeight += GetFishSpawnWeight(fishData);
            }

            if (totalWeight <= 0f)
                return null;

            var roll = Random.Range(0f, totalWeight);
            var cumulative = 0f;

            for (var i = 0; i < fishDataArray.Count; i++)
            {
                var fishData = fishDataArray[i];
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

        private enum FishSpawnPattern
        {
            Single,
            Line,
            Arc,
            RichLine,
            ArcBetweenPlatforms
        }
        
        private void Restart()
        {
            foreach (var fish in activeFishInstances)
            {
                if (fish != null)
                    GenericObjectPool<FishInstance>.Release(fish);
            }
            activeFishInstances.Clear();
        }
    }
}