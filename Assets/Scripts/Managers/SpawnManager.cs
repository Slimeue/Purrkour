using Platform;
using UnityEngine;
namespace Managers
{
    public class SpawnManager : MonoBehaviour
    {
        public static SpawnManager Instance;
        
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void HandleScetionSpawn(PlatformInstance nextPlatform, PlatformInstance previousPlatform)
        {
            if (nextPlatform == null) return;
            
            

        }

    }
}