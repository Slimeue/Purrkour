using System;
using Managers;
using UnityEngine;

namespace Player
{
    public class PlayerBase : MonoBehaviour
    {
        //add player data
        //that will include skins
        
        private PlayerHealthComponent _playerHealthComponent;
        public PlayerInputHandler InputHandler { get; private set; }
        public CatSkinData SkinData { get; private set; }
        public Animator _animator;
        public SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _playerHealthComponent = GetComponent<PlayerHealthComponent>();
            InputHandler = GetComponent<PlayerInputHandler>();
        }

        private void Start()
        {
            GameManager.Instance.OnRestartGame += ResetPlayer;
            _animator = GetComponentInChildren<Animator>();
            SkinManager.Instance.OnSwapSkin += SetCatSkinData;
            
            SetCatSkinData(SkinManager.Instance.EquippedSkin);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Death")) return;
            _playerHealthComponent.TakeDamage(_playerHealthComponent.CurrentHealth);
        }

        private void ResetPlayer()
        {
            transform.position = Vector3.zero;
            _playerHealthComponent.Reset();
        }

        public void SetCatSkinData(CatSkinData skinData)
        {
            SkinData = skinData;
            _spriteRenderer.sprite = SkinData.sprite;
            SetAnimatorOverride(skinData.animatorOverrideController);
        }

        public void SetAnimatorOverride(AnimatorOverrideController animatorOverrideController)
        {
            _animator.runtimeAnimatorController = animatorOverrideController;
        }
        
        
    }
}