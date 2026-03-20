using System;
using Player;
using S_Machine;
using State;
using State.States;
using UnityEngine;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        
        StateMachine<GameContext> _stateMachine;
        GameContext _gameContext;
        
        public static GameManager Instance { get; private set; }

        private MainMenuState MainMenuState { get; set; }
        private PlayingState PlayingState { get; set; }
        
        
        private void Awake()
        {
            if (Instance != null)
            {
                return;
            }

            Instance = this;

            _gameContext = new GameContext
            {
                // Initialize any shared data or references here
                gameManager =  this,
                uiManager = FindAnyObjectByType<UIManager>(),
                playerBase = FindAnyObjectByType<PlayerBase>()
            };
            
            _stateMachine = new StateMachine<GameContext>(_gameContext);
            
            MainMenuState = new MainMenuState();
            PlayingState = new PlayingState();
            
            GoToMainMenu();
        }

        private void Update()
        {
            _stateMachine.Update();
        }
        
        public void StartGame() => _stateMachine.ChangeState(PlayingState);
        public void GoToMainMenu() => _stateMachine.ChangeState(MainMenuState);
    }
}