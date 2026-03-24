using Managers;
using UnityEngine;

namespace UI
{
    public class ParallaxLayer : MonoBehaviour
    {
        [SerializeField] private float parallaxMultiplier = 0.5f;

        private float _spriteWidth;
        private GameObject _clone;

        private void Start()
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
                _spriteWidth = sr.bounds.size.x;

            // spawn clone flush to the right
            _clone = Instantiate(gameObject, transform.position + Vector3.right * _spriteWidth, Quaternion.identity, transform.parent);
            Destroy(_clone.GetComponent<ParallaxLayer>()); // prevent recursive spawning
        }

        private void LateUpdate()
        {
            if (WorldScrollManager.Instance == null) return;

            float move = WorldScrollManager.Instance.ScrollSpeed * parallaxMultiplier * Time.deltaTime;

            transform.position += Vector3.left * move;
            _clone.transform.position += Vector3.left * move;

            HandleLoop();
        }

        private void HandleLoop()
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            float camLeft = cam.transform.position.x - (cam.orthographicSize * cam.aspect);

            if (transform.position.x + (_spriteWidth / 2f) < camLeft)
                transform.position = new Vector3(_clone.transform.position.x + _spriteWidth, transform.position.y, transform.position.z);

            if (_clone.transform.position.x + (_spriteWidth / 2f) < camLeft)
                _clone.transform.position = new Vector3(transform.position.x + _spriteWidth, _clone.transform.position.y, _clone.transform.position.z);
        }
    }
}
