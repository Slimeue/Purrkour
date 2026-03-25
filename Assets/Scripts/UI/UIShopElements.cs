using System.Collections.Generic;
using Managers;
using Tools;
using UnityEngine;

public class UIShopElements : MonoBehaviour
{
    public static UIShopElements instance;
    public RectTransform element;

    public List<CatSkinData> catSkinData;
    public UISkinInstance prefab;
    public RectTransform rectListCont;
    public List<UISkinInstance> skinList;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        InitShop();
    }

    private void Start()
    {
        SetStatus(false);
    }

    public void SetStatus(bool status)
    {
        element.gameObject.SetActive(status);
    }

    private void InitShop()
    {
        foreach (var item in catSkinData)
        {
            var skinInstance = GenericObjectPool<UISkinInstance>.Get(prefab, rectListCont);
            skinList.Add(skinInstance);
            skinInstance.Init(item);
        }

        // after all instances exist, wire up the equipped skin instance reference
        var equippedSkin = SkinManager.Instance.EquippedSkin;
        if (equippedSkin != null)
        {
            var equippedInstance = skinList.Find(s => s.CatSkinData == equippedSkin);
            if (equippedInstance != null)
                SkinManager.Instance.SetEquippedInstance(equippedInstance);
        }
    }
}