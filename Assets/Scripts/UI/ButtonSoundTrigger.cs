using Sounds;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(Button))]
    public class ButtonSoundTrigger : MonoBehaviour
    {
        [SerializeField] private Data.SoundId soundId = Data.SoundId.ButtonClick;

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            AudioManager.Instance.Request(soundId);
        }
    }
}
