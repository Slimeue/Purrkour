using System.Globalization;
using Managers;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIGameMain : MonoBehaviour
    {
        public static UIGameMain Instance;
        
        [SerializeField] private Transform elements;

        [Header("Points UI")]
        [SerializeField] private TextMeshProUGUI pointsText;

        [Header("Health UI")]
        [SerializeField] private Slider hpSlider;

        private PlayerHealthComponent _playerHealth;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            // Points
            // PointsManager.Instance.OnPointsChanged += ChangePoints;

            // Find Player Health
            _playerHealth = FindAnyObjectByType<PlayerHealthComponent>();

            if (_playerHealth != null)
            {
                // Setup slider range
                hpSlider.maxValue = _playerHealth.MaxHealth;
                hpSlider.value = _playerHealth.CurrentHealth;

                // Subscribe to event
                _playerHealth.OnHealthChangedEvent += ChangeHp;
            }
        }
        
        public void SetStatusElementsActive(bool isActive)
        {
            if (elements != null)
                elements.gameObject.SetActive(isActive);
        }

        private void OnDestroy()
        {
            if (PointsManager.Instance != null)
                PointsManager.Instance.OnPointsChanged -= ChangePoints;

            if (_playerHealth != null)
                _playerHealth.OnHealthChangedEvent -= ChangeHp;
        }

        public void ChangePoints(float amount)
        {
            pointsText.text = amount.ToString(CultureInfo.InvariantCulture);
        }

        private void ChangeHp(int currentHealth)
        {
            hpSlider.value = currentHealth;
        }
    }
}
