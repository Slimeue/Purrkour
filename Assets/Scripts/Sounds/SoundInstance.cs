using UnityEngine;

namespace Sounds
{
    public class SoundInstance : MonoBehaviour
    {
        private AudioSource _audioSource;
        private Coroutine   _releaseCoroutine;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        public void Play(Data.AudioRequest request)
        {
            // Stop any leftover coroutine from previous use
            if (_releaseCoroutine != null)
            {
                StopCoroutine(_releaseCoroutine);
                _releaseCoroutine = null;
            }

            _audioSource.clip   = request.Clip;
            _audioSource.volume = request.Volume;
            _audioSource.pitch  = request.Pitch;
            _audioSource.loop   = request.isLoop;
            _audioSource.Play();

            if (!request.isLoop)
                _releaseCoroutine = StartCoroutine(ReleaseWhenDone());
        }

        public void Stop()
        {
            _audioSource.Stop();

            if (_releaseCoroutine != null)
            {
                StopCoroutine(_releaseCoroutine);
                _releaseCoroutine = null;
            }

            Tools.GenericObjectPool<SoundInstance>.Release(this);
        }

        public void SetVolume(float volume)
        {
            _audioSource.volume = volume;
        }

        private System.Collections.IEnumerator ReleaseWhenDone()
        {
            yield return new WaitForSeconds(_audioSource.clip.length / _audioSource.pitch);
            _releaseCoroutine = null;
            Tools.GenericObjectPool<SoundInstance>.Release(this);
        }
    }
}
