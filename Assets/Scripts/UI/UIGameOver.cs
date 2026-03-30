using System;
using System.Globalization;
using Core;
using DG.Tweening;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIGameOver : MonoBehaviour
    {
        public static UIGameOver Instance { get; private set; }

        [SerializeField] private RectTransform elements;

        [SerializeField] private CanvasGroup introGameOverPanel;
        Tween introTween;
        [SerializeField] private Image gameOverImage;
        [SerializeField] private Sprite black;
        [SerializeField] private Sprite orange;
        [SerializeField] private Sprite siamese;
        [SerializeField] private Sprite blue;
        [SerializeField] private Sprite white;
        [SerializeField] private Sprite calico;

        [Header("Reward UI References")] [SerializeField]
        private RectTransform rewardPanel;

        [SerializeField] private TextMeshProUGUI rewardPointsText;
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button exitButton;

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
            newGameButton.onClick.AddListener(RestartGame);
            exitButton.onClick.AddListener(ReturnToMainMenu);
        }

        public void ShowGameOver(bool show)
        {
            if (elements == null) return;

            introTween?.Kill();
            elements.gameObject.SetActive(show);
            introGameOverPanel.gameObject.SetActive(show);
            introGameOverPanel.alpha = 0f;

            var skinInt = (Data.CatsType)PlayerPrefs.GetInt("skin_equipped");

            gameOverImage.sprite = GetSpriteForCat(skinInt);

            if (show)
                introTween = introGameOverPanel.DOFade(1f, 2f)
                    .SetUpdate(true)
                    .OnComplete(() =>
                    {
                        introTween = introGameOverPanel.DOFade(0f, 2f)
                            .SetUpdate(true)
                            .OnComplete(() =>
                            {
                                introGameOverPanel.gameObject.SetActive(false);
                                if (rewardPanel != null)
                                    ShowRewardPanel();
                            });
                    });
        }
        
        private Sprite GetSpriteForCat(Data.CatsType catType)
        {
            return catType switch
            {
                Data.CatsType.Black   => black,
                Data.CatsType.Siamese => siamese,
                Data.CatsType.White   => white,
                Data.CatsType.Blue    => blue,
                Data.CatsType.Orange  => orange,
                Data.CatsType.Calico  => calico,
                _                     => black
            };
        } 

        private void ShowRewardPanel(bool show = true)
        {
            if (rewardPanel != null)
            {
                rewardPointsText.text = PointsManager.Instance.CurrentPoints.ToString(CultureInfo.InvariantCulture);
                rewardPanel.gameObject.SetActive(show);
            }
        }

        private void RestartGame()
        {
            GameManager.Instance.StartGame();
            ShowRewardPanel(false);
        }

        private void ReturnToMainMenu()
        {
            GameManager.Instance.GoToMainMenu();
            ShowRewardPanel(false);
        }
    }
}