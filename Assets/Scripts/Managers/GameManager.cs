using Player;
using S_Machine;
using State;
using State.States;
using UI;
using UnityEngine;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        public delegate void EndGame();

        public delegate void RestartGame();

        public delegate void ReturnToMainMenu();

        private StateMachine<GameContext> _stateMachine;
        public GameContext GameContext { get; private set; }

        public Data.GameState CurrentGameState => GameContext.gameState;

        public static GameManager Instance { get; private set; }

        private MainMenuState MainMenuState { get; set; }
        private PlayingState PlayingState { get; set; }
        private GameOverState GameOverState { get; set; }
        private IntroState IntroState { get; set; }

        private void Awake()
        {
            if (Instance != null) return;

            Instance = this;

            GameContext = new GameContext
            {
                // Initialize any shared data or references here
                gameManager = this,
                uiManager = FindAnyObjectByType<UIManager>(),
                playerBase = FindAnyObjectByType<PlayerBase>()
            };

            _stateMachine = new StateMachine<GameContext>(GameContext);

            MainMenuState = new MainMenuState();
            PlayingState = new PlayingState();
            GameOverState = new GameOverState();
            IntroState = new IntroState();
        }

        private void Start()
        {
            OnReturnToMainMenu += PointsManager.Instance.GetSavedPoints;
            OnReturnToMainMenu += LeaderboardManager.Instance.LoadLeaderboard;
            OnReturnToMainMenu += UIMainMenu.Instance.InitializeLeaderboard;

            var isIntroDone = PlayerPrefs.GetInt("isIntroDone", 0) == 1;

            GoToIntro();
            // return;


            // GoToMainMenu();
        }

        private void Update()
        {
            _stateMachine.Update();
        }

        public event EndGame OnEndGame;
        public event RestartGame OnRestartGame;
        public event ReturnToMainMenu OnReturnToMainMenu;

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

        public void GoToIntro()
        {
            _stateMachine.ChangeState(IntroState);
        }

        public void GameOver()
        {
            _stateMachine.ChangeState(GameOverState);
            OnEndGame?.Invoke();
        }
    }
}