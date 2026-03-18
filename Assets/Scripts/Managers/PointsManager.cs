using System;
using DG.Tweening;
using Scriptables;
using TMPro;
using Tools;
using UnityEngine;

namespace Managers
{
    public class PointsManager : MonoBehaviour
    {
        public static PointsManager Instance;

        public float CurrentPoints { get; private set; }

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _pointsText;
        [SerializeField] private TextMeshProUGUI _floatingTextPrefab;
        [SerializeField] private RectTransform _floatingTextContainer;

        [Header("Floating Text")]
        [SerializeField] private float _floatingTextDuration = 1f;
        [SerializeField] private float _floatingMoveUpDistance = 80f;
        
        //Actions
        public delegate void PointsChanged(float points);

        public delegate void PointsGain(float points);
        public delegate void PointsLose(float points);
        public event PointsChanged OnPointsChanged;
        public event PointsGain OnPointsGain;
        public event PointsLose OnPointsLose;
        

        private Camera _mainCamera;
        private Canvas _parentCanvas;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            _mainCamera = Camera.main;

            if (_floatingTextContainer != null)
            {
                _parentCanvas = _floatingTextContainer.GetComponentInParent<Canvas>();
            }
        }

        private void Start()
        {
            RefreshPointsText();
            OnPointsChanged?.Invoke(CurrentPoints);
        }

        public void AddPoints(FishData data, Transform worldTransform)
        {
            if (data == null || worldTransform == null)
            {
                Debug.LogWarning($"[{nameof(PointsManager)}] AddPoints called with null data or transform.", this);
                return;
            }

            float pointsToAdd = GetFishPointsValueByRarity(data.fishRarity);

            if (pointsToAdd < 0)
            {
                Debug.LogWarning($"[{nameof(PointsManager)}] Attempted to add negative points. Use RemovePoints instead.", this);
                return;
            }

            CurrentPoints += pointsToAdd;
            RefreshPointsText();
            OnPointsGain?.Invoke(pointsToAdd);
            OnPointsChanged?.Invoke(CurrentPoints);
            SpawnFloatingText($"+{pointsToAdd:0}", worldTransform.position);

            DebuggerManager.Instance.Log(
                $"Added {pointsToAdd} points. Total: {CurrentPoints}",
                DebuggerManager.LogLevel.Info,
                this
            );
        }

        public void RemovePoints(FishData data)
        {
            if (data == null)
            {
                Debug.LogWarning($"[{nameof(PointsManager)}] RemovePoints called with null data.", this);
                return;
            }

            float amount = GetFishPointsValueByRarity(data.fishRarity);

            if (amount < 0)
            {
                Debug.LogWarning($"[{nameof(PointsManager)}] Attempted to remove negative points. Use AddPoints instead.", this);
                return;
            }

            CurrentPoints = Mathf.Max(CurrentPoints - amount, 0f);
            RefreshPointsText();

            DebuggerManager.Instance.Log(
                $"Removed {amount} points. Total: {CurrentPoints}",
                DebuggerManager.LogLevel.Info,
                this
            );
        }

        private void RefreshPointsText()
        {
            if (_pointsText == null)
                return;

            _pointsText.text = CurrentPoints.ToString("0");
        }

        private void SpawnFloatingText(string text, Vector3 worldPosition)
        {
            if (_floatingTextPrefab == null || _floatingTextContainer == null)
            {
                Debug.LogWarning($"[{nameof(PointsManager)}] Floating text prefab or container is missing.", this);
                return;
            }

            TextMeshProUGUI floatingText =
                GenericObjectPool<TextMeshProUGUI>.Get(_floatingTextPrefab, _floatingTextContainer);

            if (floatingText == null)
            {
                Debug.LogWarning($"[{nameof(PointsManager)}] Failed to get floating text from pool.", this);
                return;
            }

            floatingText.gameObject.SetActive(true);
            floatingText.text = text;

            RectTransform floatingRect = floatingText.rectTransform;
            CanvasGroup canvasGroup = floatingText.GetComponent<CanvasGroup>();

            if (canvasGroup == null)
            {
                canvasGroup = floatingText.gameObject.AddComponent<CanvasGroup>();
            }

            // Kill any previous tween on reused pooled object.
            floatingRect.DOKill();
            canvasGroup.DOKill();

            canvasGroup.alpha = 1f;
            floatingRect.localScale = Vector3.one;
            
            worldPosition += Vector3.up; // Offset to appear above the fish

            Vector2 localPoint = WorldToContainerLocalPoint(worldPosition);
            floatingRect.anchoredPosition = localPoint;

            Vector2 targetPosition = localPoint + Vector2.up * _floatingMoveUpDistance;

            Sequence sequence = DOTween.Sequence();
            sequence.Append(floatingRect.DOAnchorPos(targetPosition, _floatingTextDuration).SetEase(Ease.OutCubic));
            sequence.Join(canvasGroup.DOFade(0f, _floatingTextDuration).SetEase(Ease.OutQuad));
            sequence.OnComplete(() =>
            {
                GenericObjectPool<TextMeshProUGUI>.Release(floatingText);
            });
        }

        private Vector2 WorldToContainerLocalPoint(Vector3 worldPosition)
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }

            Vector3 screenPoint = _mainCamera != null
                ? _mainCamera.WorldToScreenPoint(worldPosition)
                : worldPosition;

            if (_parentCanvas == null && _floatingTextContainer != null)
            {
                _parentCanvas = _floatingTextContainer.GetComponentInParent<Canvas>();
            }

            Camera uiCamera = null;
            if (_parentCanvas != null && _parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                uiCamera = _parentCanvas.worldCamera;
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _floatingTextContainer,
                screenPoint,
                uiCamera,
                out Vector2 localPoint
            );

            return localPoint;
        }

        public float GetFishPointsValueByRarity(Data.FishRarity rarity)
        {
            switch (rarity)
            {
                case Data.FishRarity.Common:
                    return 1f;
                case Data.FishRarity.Uncommon:
                    return 5f;
                case Data.FishRarity.Rare:
                    return 10f;
                case Data.FishRarity.Legendary:
                    return 50f;
                default:
                    Debug.LogWarning($"[{nameof(PointsManager)}] Unknown fish rarity: {rarity}", this);
                    return 0f;
            }
        }
    }
}
