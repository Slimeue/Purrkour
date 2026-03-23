using UnityEngine;

namespace Scriptables
{
    [CreateAssetMenu(menuName = "Scriptable Objects/AboutElementInstanceData")]
    public class AboutElementInstanceData : ScriptableObject
    {
        public Sprite sprite;
        public string title;
        [TextArea] public string description;
    }
}