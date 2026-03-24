using UnityEngine;

[CreateAssetMenu(fileName = "CatSkinData", menuName = "Scriptable Objects/CatSkinData")]
public class CatSkinData : ScriptableObject
{
    public Data.CatsType catsType = Data.CatsType.Orange;
    public AnimatorOverrideController animatorOverrideController;
    public Sprite sprite;
    public int cost;
}
