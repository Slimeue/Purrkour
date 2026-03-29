using System;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public SoundManager instance { private set; get; }

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }
    
    // public void Play()
    
    
}
