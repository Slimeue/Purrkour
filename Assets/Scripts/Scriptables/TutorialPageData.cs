using UnityEngine;

namespace Scriptables
{
    [CreateAssetMenu(fileName = "TutorialPageData", menuName = "Scriptable Objects/TutorialPageData")]
    public class TutorialPageData : ScriptableObject
    {
        public Sprite image;
        [TextArea]
        public string description;
    }
}
