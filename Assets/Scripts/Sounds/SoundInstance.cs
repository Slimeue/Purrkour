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
            _audioSource.clip   = request.Clip;
            _audioSource.volume = request.Volume;
            _audioSource.pitch  = request.Pitch;
            _audioSource.loop   = request.isLoop;
            _audioSource.Play();

            // Only auto-release if not looping
            if (!request.isLoop)
            {
                _releaseCoroutine = StartCoroutine(ReleaseWhenDone());
            }
        }

        // Called manually for looping sounds
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

        private System.Collections.IEnumerator ReleaseWhenDone()
        {
            yield return new WaitForSeconds(_audioSource.clip.length / _audioSource.pitch);
            Tools.GenericObjectPool<SoundInstance>.Release(this);
        }
    }
}
