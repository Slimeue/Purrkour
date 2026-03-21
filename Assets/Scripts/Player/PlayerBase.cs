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

        private void Awake()
        {
            _playerHealthComponent = GetComponent<PlayerHealthComponent>();
            InputHandler = GetComponent<PlayerInputHandler>();
        }

        private void Start()
        {
            GameManager.Instance.OnRestartGame += ResetPlayer;
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
    }
}