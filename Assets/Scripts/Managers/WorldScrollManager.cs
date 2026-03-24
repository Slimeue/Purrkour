using System;
using System.Globalization;
using UnityEngine;

namespace Managers
{
    public class WorldScrollManager : MonoBehaviour
    {
        public static WorldScrollManager Instance { get; private set; }

        [SerializeField] private float baseScrollSpeed = 5f;
        [SerializeField] private float maxScrollSpeed = 15f;
        [SerializeField] private float timeToMaxSpeed = 120f; // seconds to reach max speed
        [SerializeField] private bool isScrolling = true;

        private float _currentSpeed;
        private float _elapsedTime;

        public float ScrollSpeed => isScrolling ? _currentSpeed : 0f;
        public bool IsScrolling => isScrolling;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            _currentSpeed = baseScrollSpeed;
        }

        private void Start()
        {
            GameManager.Instance.OnRestartGame += ResetSpeed;
        }

        private void Update()
        {
            if (!isScrolling) return;

            _elapsedTime += Time.deltaTime;

            // gradually increase from base to max over timeToMaxSpeed
            float t = Mathf.Clamp01(_elapsedTime / timeToMaxSpeed);
            _currentSpeed = Mathf.Lerp(baseScrollSpeed, maxScrollSpeed, t);
        }

        public void MoveObject(Transform obj)
        {
            if (obj == null || !isScrolling) return;
            obj.Translate(Vector3.left * ScrollSpeed * Time.deltaTime);
        }

        public void ResetSpeed()
        {
            _elapsedTime = 0f;
            _currentSpeed = baseScrollSpeed;
        }

        public void StopScrolling()
        {
            isScrolling = false;
        }

        public void ResumeScrolling()
        {
            isScrolling = true;
        }

        public void SetScrollSpeed(float speed)
        {
            baseScrollSpeed = Mathf.Max(0f, speed);
            _currentSpeed = baseScrollSpeed;
            _elapsedTime = 0f;
        }
    }
}
