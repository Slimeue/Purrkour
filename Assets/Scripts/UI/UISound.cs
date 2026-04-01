using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UISound : MonoBehaviour
    {
        public static UISound Instance;

        public RectTransform elements;
        public Slider sfxSlider;
        public Slider musicSlider;

        private const string SfxVolumeKey   = "vol_sfx";
        private const string MusicVolumeKey = "vol_music";

        public Button closeButton;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            // Load saved values, default to 1 if never set
            sfxSlider.value   = PlayerPrefs.GetFloat(SfxVolumeKey,   1f);
            musicSlider.value = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);

            // Apply on load so volume matches saved state immediately
            AudioManager.Instance.SetCategoryVolume(Data.AudioCategory.SFX,   sfxSlider.value);
            AudioManager.Instance.SetCategoryVolume(Data.AudioCategory.BGM,   musicSlider.value);

            sfxSlider.onValueChanged.AddListener(OnSfxChanged);
            musicSlider.onValueChanged.AddListener(OnMusicChanged);
            closeButton.onClick.AddListener(CloseSettings);
        }

        private void OnSfxChanged(float value)
        {
            AudioManager.Instance.SetCategoryVolume(Data.AudioCategory.SFX, value);
            PlayerPrefs.SetFloat(SfxVolumeKey, value);
        }

        private void OnMusicChanged(float value)
        {
            AudioManager.Instance.SetCategoryVolume(Data.AudioCategory.BGM, value);
            PlayerPrefs.SetFloat(MusicVolumeKey, value);
        }

        public void SetStatus(bool status)
        {
            elements.gameObject.SetActive(status);
        }

        void CloseSettings()
        {
            SetStatus(false);
        }
        
    }
}
