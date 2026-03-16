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

        public PlatformData CurrentData => currentData;
        public float Width => currentData != null ? currentData.width : 0f;

        public float LeftEdge => transform.position.x - (Width * 0.5f);
        public float RightEdge => transform.position.x + (Width * 0.5f);

        public void Initialize(PlatformData data, Vector3 worldPosition)
        {
            if (data == null)
            {
                Debug.LogError($"[{nameof(PlatformInstance)}] Initialize failed because data is null.", this);
                return;
            }

            currentData = data;
            transform.position = worldPosition;

            ApplyVisuals();
            ApplyWidth();
        }

        private void ApplyVisuals()
        {
            if (spriteRenderer != null && currentData.sprite != null)
            {
                spriteRenderer.sprite = currentData.sprite;
            }
        }

        private void ApplyWidth()
        {
            if (spriteRenderer != null)
            {
                Vector3 scale = spriteRenderer.transform.localScale;
                scale.x = currentData.width;
                spriteRenderer.transform.localScale = scale;
            }

            // if (boxCollider != null)
            // {
            //     Vector2 size = boxCollider.size;
            //     size.x = currentData.width;
            //     boxCollider.size = size;
            //
            //     Vector2 offset = boxCollider.offset;
            //     offset.x = 0f;
            //     boxCollider.offset = offset;
            // }
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
            float width = currentData != null ? currentData.width : 1f;

            Vector3 left = new Vector3(transform.position.x - width * 0.5f, transform.position.y, transform.position.z);
            Vector3 right = new Vector3(transform.position.x + width * 0.5f, transform.position.y, transform.position.z);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(left + Vector3.down * 0.5f, left + Vector3.up * 0.5f);
            Gizmos.DrawLine(right + Vector3.down * 0.5f, right + Vector3.up * 0.5f);
            Gizmos.DrawLine(left, right);
        }
#endif
    }
}
