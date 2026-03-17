using UnityEngine;

namespace Platform
{
    public class PlatformInstance : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private BoxCollider2D boxCollider;

        [Header("Runtime")]
        [SerializeField] private PlatformData currentData;
        [SerializeField] private PlatformPieceData currentPieceData;
        [SerializeField] private float speedMultiplier = 1f;

        public static float WorldSpeed = 5f;

        public PlatformData CurrentData => currentData;
        public PlatformPieceData CurrentPieceData => currentPieceData;

        // Width should now come from the spawned piece, not the whole section data.
        public float Width => currentPieceData != null ? currentPieceData.width : 0f;
        public float Heigh => currentPieceData != null ? currentPieceData.height : 0f;

        public float LeftEdge => transform.position.x - (Width * 0.5f);
        public float RightEdge => transform.position.x + (Width * 0.5f);

        private void Update()
        {
            // Temporary local movement approach for prototyping.
            // Later you can move this to a centralized scroller manager.
            transform.position += Vector3.left * (WorldSpeed * speedMultiplier * Time.deltaTime);
        }

        public void Initialize(PlatformData data, PlatformPieceData pieceData, Vector3 worldPosition)
        {
            if (data == null)
            {
                Debug.LogError($"[{nameof(PlatformInstance)}] Initialize failed because PlatformData is null.", this);
                return;
            }

            if (pieceData == null)
            {
                Debug.LogError($"[{nameof(PlatformInstance)}] Initialize failed because PlatformPieceData is null.", this);
                return;
            }

            currentData = data;
            currentPieceData = pieceData;

            transform.position = worldPosition;

            ApplyVisuals();
            ApplySize();
        }

        private void ApplyVisuals()
        {
            if (spriteRenderer != null && currentPieceData.sprite != null)
            {
                spriteRenderer.sprite = currentPieceData.sprite;
            }
        }

        private void ApplySize()
        {
            if (spriteRenderer != null)
            {
                Vector3 scale = spriteRenderer.transform.localScale;
                scale.x = currentPieceData.width;
                scale.y = currentPieceData.height;
                spriteRenderer.transform.localScale = scale;
            }
        }

        private void ApplyCollider()
        {
            if (boxCollider != null)
            {
                Vector2 size = boxCollider.size;
                size.x = currentPieceData.width;
                boxCollider.size = size;
            }
        }

        public bool IsOffScreenLeft(float despawnX)
        {
            return RightEdge < despawnX;
        }

        private void Reset()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (boxCollider == null)
                boxCollider = GetComponent<BoxCollider2D>();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            float width = currentPieceData != null ? currentPieceData.width : 1f;

            Vector3 left = new Vector3(
                transform.position.x - width * 0.5f,
                transform.position.y,
                transform.position.z
            );

            Vector3 right = new Vector3(
                transform.position.x + width * 0.5f,
                transform.position.y,
                transform.position.z
            );

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(left + Vector3.down * 0.5f, left + Vector3.up * 0.5f);
            Gizmos.DrawLine(right + Vector3.down * 0.5f, right + Vector3.up * 0.5f);
            Gizmos.DrawLine(left, right);
        }
#endif
    }
}
