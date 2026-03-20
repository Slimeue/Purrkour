using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIMainMenu : MonoBehaviour
    {
        public static UIMainMenu Instance { get; private set; }

        [SerializeField] private Transform elements;
        
        [SerializeField] private Button startButton;
    
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
            if (startButton != null)
            {
                startButton.onClick.AddListener(StartButtonClicked);
            }
        }

        public void SetStatus(bool status)
        {
            elements.gameObject.SetActive(status);
        }
        
        private static void StartButtonClicked()
        {
            GameManager.Instance.StartGame();
        }
    }
}
