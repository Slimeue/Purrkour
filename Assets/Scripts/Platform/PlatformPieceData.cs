using Platform;
using UnityEngine;

[System.Serializable]
public class PlatformPieceData
{
    public PlatformInstance prefab;
    public Sprite sprite;

    [Header("Placement")]
    public float apartValue = 0f;      // Horizontal distance from the previous piece
    public float verticalOffset = 0f;  // Y offset relative to the section base Y

    [Header("Size")]
    public float height = 0f;
    public float width = 3f;

    [Header("Flags")]
    public bool isCeiling = false;

    public bool affectsSectionEnd = true; // If true, this piece will determine the end of the section for spawning the next one. If false, the next section can spawn based on the previous piece's position plus its width and apart value.
}
