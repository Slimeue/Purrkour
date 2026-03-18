using System.Collections.Generic;
using Fish;
using Managers;
using Platform;
using Tools;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core
{
    public class PlatformGenerator : MonoBehaviour
    {
        [Header("Data")] [SerializeField] private List<PlatformData> platformDatas;

        [Header("Generation Settings")] [SerializeField]
        private int initialSpawnCount = 8;

        [SerializeField] private float startY = -2f;
        [SerializeField] private float minGap = 1f;
        [SerializeField] private float maxGap = 2.5f;
        [SerializeField] private float maxStepUp = 1.5f;
        [SerializeField] private float maxStepDown = 2f;
        [SerializeField] private float minY = -3f;
        [SerializeField] private float maxY = 2f;

        [SerializeField] private float generateAheadDistance = 25f;
        [SerializeField] private float despawnBehindDistance = 20f;

        [Header("Parent")] [SerializeField] private Transform platformParent;

        [Header("Debug")] [SerializeField] private bool showDebugLogs;

        private readonly List<PlatformInstance> _activePlatforms = new();

        private Camera _mainCamera;
        private float _lastPlatformY;

        private void Awake()
        {
            _mainCamera = Camera.main;

            if (platformParent == null)
                platformParent = transform;
        }

        private void Start()
        {
            if (platformDatas == null || platformDatas.Count == 0)
            {
                DebuggerManager.Instance.Warn("PlatformGenerator: platformDatas is null or empty", gameObject);
                enabled = false;
                return;
            }

            InitializeGeneration();
        }

        private void Update()
        {
            GenerateAheadIfNeeded();
            CleanupOldPlatforms();
        }

        private void InitializeGeneration()
        {
            _lastPlatformY = startY;

            for (int i = 0; i < initialSpawnCount; i++)
            {
                SpawnNextPlatformSection(i == 0);
            }
        }

        private void GenerateAheadIfNeeded()
        {
            float targetRightX = GetCameraRightEdgeX() + generateAheadDistance;

            while (GetRightmostProgressionEdge() < targetRightX)
            {
                SpawnNextPlatformSection(false);
            }
        }

        private void CleanupOldPlatforms()
        {
            float despawnX = GetCameraLeftEdgeX() - despawnBehindDistance;

            for (int i = _activePlatforms.Count - 1; i >= 0; i--)
            {
                PlatformInstance platformInstance = _activePlatforms[i];

                if (platformInstance == null)
                {
                    _activePlatforms.RemoveAt(i);
                    continue;
                }

                if (!platformInstance.IsOffScreenLeft(despawnX))
                    continue;

                _activePlatforms.RemoveAt(i);
                GenericObjectPool<PlatformInstance>.Release(platformInstance);
            }
        }

        private void SpawnNextPlatformSection(bool isFirst)
        {
            PlatformData chosenData = isFirst ? platformDatas[0] : GetWeightRandomPlatformData();

            if (!IsValidPlatformData(chosenData))
            {
                DebuggerManager.Instance.Warn("PlatformGenerator: No valid PlatformData found to spawn", gameObject);
                return;
            }

            float sectionGap = isFirst ? 0f : Random.Range(minGap, maxGap);
            float targetY = isFirst ? startY : GetNextPlatformY(chosenData);

            float sectionStartX = isFirst
                ? GetCameraLeftEdgeX()
                : GetRightmostProgressionEdge() + sectionGap;

            float currentLeftEdgeX = sectionStartX;

            float sectionRightEdge = sectionStartX;

            for (int i = 0; i < chosenData.pieces.Length; i++)
            {
                PlatformPieceData piece = chosenData.pieces[i];

                if (piece == null || piece.prefab == null)
                {
                    DebuggerManager.Instance.Warn(
                        $"PlatformGenerator: PlatformData '{chosenData.name}' has a null piece or prefab at index {i}",
                        gameObject);
                    continue;
                }

                float centerX = isFirst ? 0f : currentLeftEdgeX + (piece.width * 0.5f);

                float pieceY = targetY;
                if (piece.isCeiling)
                {
                    pieceY += piece.verticalOffset;
                }

                Vector3 pieceSpawnPosition = new Vector3(centerX, pieceY, 0f);

                PlatformInstance newPlatformInstance =
                    GenericObjectPool<PlatformInstance>.Get(piece.prefab, platformParent, 16);

                if (newPlatformInstance == null)
                {
                    DebuggerManager.Instance.Warn("PlatformGenerator: No valid PlatformInstance found", gameObject);
                    continue;
                }

                newPlatformInstance.Initialize(chosenData, piece, pieceSpawnPosition);
                _activePlatforms.Add(newPlatformInstance);

                if (piece.affectsSectionEnd)
                {
                    sectionRightEdge = Mathf.Max(sectionRightEdge, newPlatformInstance.RightEdge);
                }

                if (!piece.isCeiling)
                {
                    currentLeftEdgeX = newPlatformInstance.RightEdge;
                    FishSpawnManager.Instance.HandleSectionSpawn(newPlatformInstance);
                }
            }

            _lastPlatformY = targetY;

            if (showDebugLogs)
            {
                DebuggerManager.Instance.DebugLog(
                    $"PlatformGenerator: Spawned section '{chosenData.name}' | StartX: {sectionStartX:F2}, BaseY: {targetY:F2}, ProgressionRightEdge: {sectionRightEdge:F2}",
                    gameObject);
            }
        }

        private float GetNextPlatformY(PlatformData data)
        {
            float randomOffset = Random.Range(data.minHeightOffset, data.maxHeightOffset);
            float unclampedY = _lastPlatformY + randomOffset;

            float clampedByStep = Mathf.Clamp(
                unclampedY,
                _lastPlatformY - maxStepDown,
                _lastPlatformY + maxStepUp
            );

            float finalY = Mathf.Clamp(clampedByStep, minY, maxY);

            if (showDebugLogs)
            {
                DebuggerManager.Instance.DebugLog(
                    $"PlatformGenerator: GetNextPlatformY() - lastY: {_lastPlatformY}, randomOffset: {randomOffset}, unclampedY: {unclampedY}, clampedByStep: {clampedByStep}, finalY: {finalY}",
                    gameObject);
            }

            return finalY;
        }

        private PlatformData GetWeightRandomPlatformData()
        {
            float totalWeight = 0f;

            for (int i = 0; i < platformDatas.Count; i++)
            {
                PlatformData platformData = platformDatas[i];

                if (!IsValidPlatformData(platformData))
                {
                    DebuggerManager.Instance.Warn(
                        $"PlatformGenerator: platformDatas[{i}] is invalid or has no valid pieces",
                        gameObject);
                    continue;
                }

                totalWeight += Mathf.Max(0f, platformData.spawnWeight);
            }

            if (totalWeight <= 0f)
            {
                for (int i = 0; i < platformDatas.Count; i++)
                {
                    if (IsValidPlatformData(platformDatas[i]))
                        return platformDatas[i];
                }

                return null;
            }

            float roll = Random.Range(0f, totalWeight);
            float cumulativeWeight = 0f;

            for (int i = 0; i < platformDatas.Count; i++)
            {
                PlatformData platformData = platformDatas[i];

                if (!IsValidPlatformData(platformData))
                    continue;

                cumulativeWeight += Mathf.Max(0f, platformData.spawnWeight);

                if (roll <= cumulativeWeight)
                    return platformData;
            }

            return platformDatas[^1];
        }

        private bool IsValidPlatformData(PlatformData data)
        {
            if (data == null || data.pieces == null || data.pieces.Length == 0)
                return false;

            for (int i = 0; i < data.pieces.Length; i++)
            {
                PlatformPieceData piece = data.pieces[i];
                if (piece != null && piece.prefab != null)
                    return true;
            }

            return false;
        }

        private float GetCameraRightEdgeX()
        {
            if (_mainCamera == null)
                return 0f;

            float halfWidth = _mainCamera.orthographicSize * _mainCamera.aspect;
            return _mainCamera.transform.position.x + halfWidth;
        }

        private float GetCameraLeftEdgeX()
        {
            if (_mainCamera == null)
                return 0f;

            float halfWidth = _mainCamera.orthographicSize * _mainCamera.aspect;
            return _mainCamera.transform.position.x - halfWidth;
        }

        private float GetRightmostPlatformEdge()
        {
            if (_activePlatforms.Count == 0)
                return GetCameraLeftEdgeX() - 2f;

            float rightmostX = float.MinValue;

            for (int i = 0; i < _activePlatforms.Count; i++)
            {
                PlatformInstance platform = _activePlatforms[i];
                if (platform == null) continue;

                if (platform.RightEdge > rightmostX)
                    rightmostX = platform.RightEdge;
            }

            return rightmostX;
        }

        private float GetRightmostProgressionEdge()
        {
            if (_activePlatforms.Count == 0)
                return GetCameraLeftEdgeX() - 2f;

            float rightmostX = float.MinValue;

            for (int i = 0; i < _activePlatforms.Count; i++)
            {
                PlatformInstance platform = _activePlatforms[i];
                if (platform == null) continue;

                PlatformPieceData pieceData = platform.CurrentPieceData;
                if (pieceData == null) continue;

                if (!pieceData.affectsSectionEnd)
                    continue;

                if (platform.RightEdge > rightmostX)
                    rightmostX = platform.RightEdge;
            }

            return rightmostX == float.MinValue ? GetCameraLeftEdgeX() - 2f : rightmostX;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Camera cam = Camera.main;
            if (cam == null)
                return;

            float halfWidth = cam.orthographicSize * cam.aspect;
            float rightEdge = cam.transform.position.x + halfWidth;
            float leftEdge = cam.transform.position.x - halfWidth;

            Gizmos.color = Color.green;
            Gizmos.DrawLine(
                new Vector3(rightEdge + generateAheadDistance, -10f, 0f),
                new Vector3(rightEdge + generateAheadDistance, 10f, 0f)
            );

            Gizmos.color = Color.red;
            Gizmos.DrawLine(
                new Vector3(leftEdge - despawnBehindDistance, -10f, 0f),
                new Vector3(leftEdge - despawnBehindDistance, 10f, 0f)
            );
        }
#endif
    }
}