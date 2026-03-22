using Managers;
using S_Machine;
using UI;

namespace State.States
{
    public class GameOverState : IState<GameContext>
    {
        public void OnEnter(GameContext context)
        {
            context.gameState = Data.GameState.GameOver;
            context.playerBase.InputHandler.DisablePlayerInputHandler();
            
            WorldScrollManager.Instance.StopScrolling();
            
            UIGameOver.Instance.ShowGameOver(true);
        }

        public void OnUpdate(GameContext context)
        {
        }

        public void OnExit(GameContext context)
        {
            UIGameOver.Instance.ShowGameOver(false);
        }
    }
}