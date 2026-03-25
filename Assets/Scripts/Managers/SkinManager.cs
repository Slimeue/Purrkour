using System;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    public class SkinManager : MonoBehaviour
    {
        public static SkinManager Instance { get; private set; }
        [SerializeField] private List<CatSkinData> allSkins;
        [SerializeField] private CatSkinData defaultSkin;

        private const string UnlockedKeyPrefix = "skin_unlocked_";
        private const string EquippedKey = "skin_equipped";

        private CatSkinData _equippedSkin;
        private CatSkinData _previewsEquippedSkin;
        public CatSkinData EquippedSkin => _equippedSkin;
        public CatSkinData PreviewsEquippedSkin => _previewsEquippedSkin;

        private UISkinInstance _skinInstance;
        private UISkinInstance _previousSkinInstance;
        public UISkinInstance SkinInstance => _skinInstance;
        public UISkinInstance PreviousSkinInstance => _previousSkinInstance;

        public delegate void BuySkin(int amount);

        public delegate void SwapSkin(CatSkinData skin);

        public BuySkin OnBuySkin;
        public SwapSkin OnSwapSkin;


        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Load();
        }

        public bool IsUnlocked(CatSkinData skin)
        {
            if (skin == defaultSkin) return true;
            return PlayerPrefs.GetInt(UnlockedKeyPrefix + skin.catsType, 0) == 1;
        }

        public bool TryUnlock(CatSkinData skin, int points)
        {
            if (IsUnlocked(skin)) return false;
            if (points < skin.cost) return false;

            PlayerPrefs.SetInt(UnlockedKeyPrefix + skin.catsType, 1);
            PlayerPrefs.Save();
            OnBuySkin?.Invoke(skin.cost);
            return true;
        }

        public void Unlock(CatSkinData skin)
        {
            PlayerPrefs.SetInt(UnlockedKeyPrefix + skin.catsType, 1);
            PlayerPrefs.Save();
        }

        public void Equip(CatSkinData skin, UISkinInstance skinInstance)
        {
            if (!IsUnlocked(skin)) return;

            _previewsEquippedSkin = _equippedSkin;
            _previousSkinInstance = _skinInstance;

            _equippedSkin = skin;
            _skinInstance = skinInstance;

            _previousSkinInstance?.Init(_previewsEquippedSkin);

            _skinInstance?.Init(skin);

            OnSwapSkin?.Invoke(skin);
            PlayerPrefs.SetInt(EquippedKey, (int)skin.catsType);
            PlayerPrefs.Save();
        }

        private void Load()
        {
            if (defaultSkin != null)
                Unlock(defaultSkin);

            if (PlayerPrefs.HasKey(EquippedKey))
            {
                var savedType = (Data.CatsType)PlayerPrefs.GetInt(EquippedKey);
                _equippedSkin = allSkins.Find(s => s.catsType == savedType);
            }

            if (_equippedSkin == null)
                _equippedSkin = defaultSkin;
        }

        public void SetEquippedInstance(UISkinInstance skinInstance)
        {
            _skinInstance = skinInstance;
        }
    }
}