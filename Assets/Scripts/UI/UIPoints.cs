using System;
using System.Globalization;
using Fish;
using Managers;
using TMPro;
using UnityEngine;

namespace UI
{
    public class UIPoints : MonoBehaviour
    {
        public static UIPoints Instance;
        
        [SerializeField] private TextMeshProUGUI pointsText;

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
            PointsManager.Instance.OnPointsChanged += ChangePoints;
        }

        private void ChangePoints(float amount)
        {
            pointsText.text = $"{amount.ToString(CultureInfo.InvariantCulture)}";
        }
        
    }
}