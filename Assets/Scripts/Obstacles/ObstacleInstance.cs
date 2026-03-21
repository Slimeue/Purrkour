using System;
using Interface;
using Managers;
using Scriptables;
using Platform;
using Player;
using Tools;
using UnityEngine;

namespace Obstacles
{
    public class ObstacleInstance : MonoBehaviour
    {
        [SerializeField] private ObstacleData currentData;
        [SerializeField] private PlatformInstance platform;
        [SerializeField] private float speedMultiplier = 1f;
        [SerializeField] private float despawnOffset = 2f;

        private Camera _mainCamera;

        public void Initialize(ObstacleData data, Vector3 worldPosition, PlatformInstance platformRef)
        {
            currentData = data;
            platform = platformRef;
            transform.position = worldPosition;
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
                ObstacleSpawnManager.Instance.obstacleInstances.Remove(this);
                GenericObjectPool<ObstacleInstance>.Release(this);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            var killable = other.GetComponentInParent<IKillable>();

            killable?.TakeDamage(currentData.damage);

            ObstacleSpawnManager.Instance.obstacleInstances.Remove(this);
            GenericObjectPool<ObstacleInstance>.Release(this);
        }
    }
}