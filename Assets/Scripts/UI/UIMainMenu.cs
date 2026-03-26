using System.Collections.Generic;
using Leaderboard;
using Managers;
using Scriptables;
using TMPro;
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
        [SerializeField] private List<TutorialListData> tutorialListData;
        [SerializeField] private UITutorialElement _tutorialElementPrefab;
        [SerializeField] private RectTransform tutorialListCont;
        [SerializeField] private RectTransform tutorialPageCont;
        [SerializeField] private Image tutorialPageImage;
        [SerializeField] private TextMeshProUGUI tutorialPageText;
        [SerializeField] private TextMeshProUGUI tutorialPageCount;
        [SerializeField] private Button tutorialPageNextButton;
        [SerializeField] private Button tutorialPagePreviousButton;
        private TutorialListData _selectedTutorial;
        private TutorialPageData _selectedTutorialPage;
        private int currentPage;
        

        [Header("Shop UI")] [SerializeField] private Button shopButton;
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

            if (closeAboutButton != null)
                closeAboutButton.onClick.AddListener(() => { SetAboutStatus(false); });

            if (shopButton != null)
                shopButton.onClick.AddListener(() => { UIShopElements.instance.SetStatus(true); });
            if (shopCloseButton != null)
                shopCloseButton.onClick.AddListener(() => { UIShopElements.instance.SetStatus(false); });
            
            if(tutorialPageNextButton != null)
                tutorialPageNextButton.onClick.AddListener(NextPage);
            
            if(tutorialPagePreviousButton != null)
                tutorialPagePreviousButton.onClick.AddListener(PreviousPage);
            
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
            foreach (var data in tutorialListData)
            {
                var instance = GenericObjectPool<UITutorialElement>.Get(_tutorialElementPrefab, aboutListCont);
                instance.Init(data);
            }
        }

        private void SetAboutStatus(bool status)
        {
            aboutElements.gameObject.SetActive(status);
        }

        public void SelectTutorial(TutorialListData data)
        {
            _selectedTutorial = data;
            if (_selectedTutorial == null || _selectedTutorial.pages.Count <= 0)
                return;
            
            tutorialPageCont.gameObject.SetActive(true);
            tutorialListCont.gameObject.SetActive(false);

            currentPage = 1;
            SetPage(data.pages[ currentPage - 1 ]);
        }

        public void SetPage(TutorialPageData data)
        {
            _selectedTutorialPage = data;

            tutorialPageCount.text = $"{currentPage} / {_selectedTutorial.pages.Count}";
            
            if(_selectedTutorialPage == null) return;
            tutorialPageImage.sprite = _selectedTutorialPage.image;
            tutorialPageText.text = _selectedTutorialPage.description;
        }

        private void NextPage()
        {
            currentPage++;
            if (currentPage > _selectedTutorial.pages.Count)
                currentPage = 1;
            
            SetPage(_selectedTutorial.pages[currentPage - 1]);
        }

        private void PreviousPage()
        {
            currentPage--;
            if (currentPage <= 0)
                currentPage = _selectedTutorial.pages.Count;
            
            SetPage(_selectedTutorial.pages[currentPage - 1]);
        }
        
        
    }
}