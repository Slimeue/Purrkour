using Scriptables;
using Platform;
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
            transform.position += Vector3.left * (PlatformInstance.WorldSpeed * speedMultiplier * Time.deltaTime);

            if (_mainCamera == null)
                return;

            float halfWidth = _mainCamera.orthographicSize * _mainCamera.aspect;
            float leftEdge = _mainCamera.transform.position.x - halfWidth;

            if (transform.position.x < leftEdge - despawnOffset)
            {
                GenericObjectPool<ObstacleInstance>.Release(this);
            }
        }
    }
}
