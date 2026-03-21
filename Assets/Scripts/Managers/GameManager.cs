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
        private StateMachine<GameContext> _stateMachine;
        public GameContext GameContext { get; private set; }

        public Data.GameState CurrentGameState => GameContext.gameState;
        
        public static GameManager Instance { get; private set; }

        private MainMenuState MainMenuState { get; set; }
        private PlayingState PlayingState { get; set; }
        private GameOverState GameOverState { get; set; }

        public delegate void RestartGame();
        public delegate void ReturnToMainMenu();
        public event RestartGame OnRestartGame;
        public event ReturnToMainMenu OnReturnToMainMenu;
        
        private void Awake()
        {
            if (Instance != null)
            {
                return;
            }

            Instance = this;

            GameContext = new GameContext
            {
                // Initialize any shared data or references here
                gameManager =  this,
                uiManager = FindAnyObjectByType<UIManager>(),
                playerBase = FindAnyObjectByType<PlayerBase>()
            };
            
            _stateMachine = new StateMachine<GameContext>(GameContext);
            
            MainMenuState = new MainMenuState();
            PlayingState = new PlayingState();
            GameOverState = new GameOverState();
            
        }

        private void Start()
        {
            GoToMainMenu();
        }

        private void Update()
        {
            _stateMachine.Update();
        }
        
        public void StartGame()
        {
            OnRestartGame?.Invoke();
            _stateMachine.ChangeState(PlayingState);
        }

        public void GoToMainMenu()
        {
            OnReturnToMainMenu?.Invoke();
            OnRestartGame?.Invoke();
            _stateMachine.ChangeState(MainMenuState);
        }

        public void GameOver() => _stateMachine.ChangeState(GameOverState);


    }
}