using System;
using Managers;
using Platform;
using Scriptables;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Fish
{
    public class FishInstance : MonoBehaviour
    {
        [SerializeField] private FishData fishData;
        public FishData FishData => fishData;
        [SerializeField] private PlatformInstance platform;

        [SerializeField] private float speedMultiplier = 1f;
        [SerializeField] private float despawnOffset = 2f;

        private Camera _mainCamera;

        public void Initialize(FishData data, Vector3 worldPosition, PlatformInstance platformRef)
        {
            fishData = data;
            transform.position = worldPosition;
            platform = platformRef;
            gameObject.SetActive(true);

            if (_mainCamera == null)
                _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (WorldScrollManager.Instance == null)
                return;

            WorldScrollManager.Instance.MoveObject(transform);

            if (_mainCamera == null)
                return;

            float halfWidth = _mainCamera.orthographicSize * _mainCamera.aspect;
            float leftEdge = _mainCamera.transform.position.x - halfWidth;

            if (transform.position.x < leftEdge - despawnOffset)
            {
                FishSpawnManager.Instance.activeFishInstances.Remove(this);
                GenericObjectPool<FishInstance>.Release(this);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            FishSpawnManager.Instance.activeFishInstances.Remove(this);
            PointsManager.Instance.AddPoints(fishData, other.transform);
            GenericObjectPool<FishInstance>.Release(this);
        }


        private void OnDrawGizmos()
        {
            if (!DebuggerManager.Instance.showLogs)
                return;

            Vector3 pos = transform.position;

            // Draw vertical line (like a marker)
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(pos, pos + Vector3.up * 0.5f);

            // Draw small sphere at fish position
            Gizmos.DrawSphere(pos, 0.1f);

#if UNITY_EDITOR
            // Draw text label
            Handles.color = Color.white;

            string label = $"Fish\nX: {pos.x:F2}\nY: {pos.y:F2}";
            Handles.Label(pos + Vector3.up * 0.6f, label);
#endif
        }
    }
}