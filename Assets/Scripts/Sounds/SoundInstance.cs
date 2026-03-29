using System;
using UnityEngine;

namespace Sounds
{
    public class SoundInstance : MonoBehaviour
    {
       AudioSource audioSource; 
        
        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        public void PlaySound(AudioClip clip, AudioSource source)
        {
        }
    }
}