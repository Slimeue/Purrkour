using Scriptables;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class AboutElementInstance : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;

        public void Init(AboutElementInstanceData data)
        {
            image.sprite = data.sprite;
            titleText.text = data.title;
            descriptionText.text = data.description;
        }
    }
}
