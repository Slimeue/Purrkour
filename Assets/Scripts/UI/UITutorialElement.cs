using System;
using Scriptables;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UITutorialElement : MonoBehaviour
    {
        private TutorialListData _tutorialListData;
        public TextMeshProUGUI titleText;
        public Button tutorialButton;

        private void Start()
        {
            tutorialButton.onClick.AddListener(SelectTutorial);
        }

        public void Init(TutorialListData data)
        {
            _tutorialListData = data;

            titleText.text = data.title;
        }

        private void SelectTutorial()
        {
            UIMainMenu.Instance.SelectTutorial(_tutorialListData);
        }
    }
}