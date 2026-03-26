using System.Collections.Generic;
using UnityEngine;

namespace Scriptables
{
    [CreateAssetMenu(fileName = "TutorialListData", menuName = "Scriptable Objects/TutorialListData")]
    public class TutorialListData : ScriptableObject
    {
        public string title;
        public List<TutorialPageData> pages = new List<TutorialPageData>();
    }
}
