using System.Collections.Generic;
using Platform;
using UnityEngine;

namespace Core
{
    public class PlatformGenerator : MonoBehaviour
    {
        [SerializeField] private float generateAheadDistance = 25f;
        [SerializeField] private float despawnBehindDistance = 20f;

        [Header("Parent")]
        [SerializeField] private Transform platformParent;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs;

        private readonly List<PlatformInstance> _activePlatforms = new();
        
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
