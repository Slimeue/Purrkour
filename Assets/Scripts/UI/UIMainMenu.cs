using System.Collections.Generic;
using Leaderboard;
using Managers;
using Scriptables;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIMainMenu : MonoBehaviour
    {
        public static UIMainMenu Instance { get; private set; }

        [SerializeField] private Transform elements;

        [SerializeField] private Button startButton;

        [Header("Leaderboard UI")] [SerializeField]
        private RectTransform leaderBoardCont;

        [SerializeField] private BestScoreInstance _scoreInstance;
        private List<BestScoreInstance> _scoreInstances = new List<BestScoreInstance>();

        [Header("About UI")] [SerializeField] private AboutElementInstance aboutElementPrefab;
        [SerializeField] private Button aboutButton;
        [SerializeField] private Button closeAboutButton;
        [SerializeField] private List<AboutElementInstanceData> aboutElementsData;
        [SerializeField] private RectTransform aboutElements;
        [SerializeField] private RectTransform aboutListCont;
        
        [Header("Shop UI")]
        [SerializeField] private Button shopButton;
        [SerializeField] private Button shopCloseButton;

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

            InitializeAbout();
            
            aboutElements.gameObject.SetActive(false);

            if (aboutButton != null)
                aboutButton.onClick.AddListener(() => { SetAboutStatus(true); });
            
            if(closeAboutButton != null)
                closeAboutButton.onClick.AddListener(() => { SetAboutStatus(false); });
            
            if(shopButton != null)
                shopButton.onClick.AddListener(() => { UIShopElements.instance.SetStatus(true); });
            if(shopCloseButton!= null)
                shopCloseButton.onClick.AddListener(() => { UIShopElements.instance.SetStatus(false); });
        }

        public void SetStatus(bool status)
        {
            elements.gameObject.SetActive(status);
        }

        private static void StartButtonClicked()
        {
            GameManager.Instance.StartGame();
        }

        public void InitializeLeaderboard()
        {
            if (leaderBoardCont == null || _scoreInstance == null) return;

            // Clear existing instances
            foreach (var instance in _scoreInstances)
            {
                GenericObjectPool<BestScoreInstance>.Release(instance);
            }

            _scoreInstances.Clear();

            // Get scores and sort them
            var scores = LeaderboardManager.Instance.Scores;

            // Create UI instances for each score
            // ✅ Show best score first (descending, as sorted)
            foreach (var t in scores)
            {
                var instance = GenericObjectPool<BestScoreInstance>.Get(_scoreInstance, leaderBoardCont);
                _scoreInstances.Add(instance);
                instance.SetBestScore(t);
                instance.transform.SetAsLastSibling();
            }
        }

        private void InitializeAbout()
        {
            foreach (var data in aboutElementsData)
            {
                var instance = GenericObjectPool<AboutElementInstance>.Get(aboutElementPrefab, aboutListCont);
                instance.Init(data);
            }
        }

        private void SetAboutStatus(bool status)
        {
            aboutElements.gameObject.SetActive(status);
        }
    }
}