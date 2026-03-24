using System;
using System.Collections.Generic;
using Tools;
using UnityEngine;

public class UIShopElements : MonoBehaviour
{
    public static UIShopElements instance;
    public RectTransform element;

    public List<CatSkinData> catSkinData;
    public UISkinInstance prefab;
    public RectTransform rectListCont;

    private void Start()
    {
        InitShop();
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
            skinInstance.Init(item);
        }
    }
}
