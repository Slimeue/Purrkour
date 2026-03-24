using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISkinInstance : MonoBehaviour
{
    public CatSkinData CatSkinData;
    public Image _image;
    public TextMeshProUGUI _name;
    public TextMeshProUGUI cost;


    public void Init(CatSkinData catSkinData)
    {
        CatSkinData = catSkinData;

        if (_image)
            _image.sprite = catSkinData.sprite;
        
        if(_name)
            _name.text = catSkinData.name;

        if (cost)
        {
            cost.text = catSkinData.catsType == Data.CatsType.Orange ? "Owned" :catSkinData.cost.ToString();
        }
    }
}
