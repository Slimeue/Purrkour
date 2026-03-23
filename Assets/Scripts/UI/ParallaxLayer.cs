using Managers;
using UnityEngine;

namespace UI
{
    public class ParallaxLayer : MonoBehaviour
    {
        [SerializeField] private float parallaxMultiplier = 0.5f;

        private float _spriteWidth;

        private void Start()
        {
            // Get width of sprite (important for looping)
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                _spriteWidth = sr.bounds.size.x;
            }
        }

        private void Update()
        {
            if (WorldScrollManager.Instance == null)
                return;

            float speed = WorldScrollManager.Instance.ScrollSpeed;

            transform.position += Vector3.left * (speed * parallaxMultiplier * Time.deltaTime);

            HandleLoop();
        }

        private void HandleLoop()
        {
            if (_spriteWidth <= 0f)
                return;

            Camera cam = Camera.main;
            if (cam == null)
                return;

            float camLeft = cam.transform.position.x - (cam.orthographicSize * cam.aspect);

            // If fully off screen → move to right
            if (transform.position.x + (_spriteWidth / 2f) < camLeft)
            {
                transform.position += Vector3.right * _spriteWidth * 2f;
            }
        }
    }
}