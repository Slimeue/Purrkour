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

    }
}