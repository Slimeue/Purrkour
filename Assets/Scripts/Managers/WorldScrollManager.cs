using System;
using UnityEngine;

namespace Managers
{
    public class WorldScrollManager : MonoBehaviour
    {
        public static WorldScrollManager Instance { get; private set; }

        [SerializeField] private float baseScrollSpeed = 5f;
        [SerializeField] private bool isScrolling = true;

        public float ScrollSpeed => isScrolling ? baseScrollSpeed : 0f;
        public bool IsScrolling => isScrolling;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void MoveObject(Transform obj)
        {
            if (obj == null || !isScrolling)
                return;

            obj.Translate(Vector3.left * ScrollSpeed * Time.deltaTime);
        }

        public void SetScrollSpeed(float speed)
        {
            baseScrollSpeed = Mathf.Max(0f, speed);
        }

        public void StopScrolling()
        {
            isScrolling = false;
        }

        public void ResumeScrolling()
        {
            isScrolling = true;
        }
    }
}
