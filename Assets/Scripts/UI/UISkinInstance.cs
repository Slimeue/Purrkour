using System;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISkinInstance : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button buyButton;

    [SerializeField]
    private TextMeshProUGUI buyButtonText; // assign in inspector, avoid GetComponentInChildren every Init call

    public CatSkinData CatSkinData { get; private set; }

    private void Start()
    {
        if(buyButton)
            buyButton.onClick.AddListener(Buy);
    }

    public void Init(CatSkinData catSkinData)
    {
        CatSkinData = catSkinData;

        var sm = SkinManager.Instance; // cache — avoids repeated property lookup
        var isUnlocked = sm.IsUnlocked(catSkinData);
        var isEquipped = sm.EquippedSkin == catSkinData;

        if (image) image.sprite = catSkinData.sprite;
        if (nameText) nameText.text = catSkinData.catsType.ToString(); // use enum, not SO name
        if (costText) costText.text = isUnlocked ? "Owned" : catSkinData.cost.ToString();

        if (buyButton)
        {
            buyButton.interactable = !isEquipped; // uninteractable if already equipped, not just unlocked
            if (buyButtonText)
                buyButtonText.text = !isUnlocked ? $"Buy" : isEquipped ? "Equipped" : "Equip";
        }
    }

    public void Buy()
    {
        var sm = SkinManager.Instance; // cache — avoids repeated property lookup
        var isUnlocked = sm.IsUnlocked(CatSkinData);

        if (isUnlocked)
        {
            sm.Equip(CatSkinData, this);
            return;
        }

        if (sm.TryUnlock(CatSkinData, Mathf.CeilToInt(PointsManager.Instance.TotalPoints)))
            Init(CatSkinData);
    }
}