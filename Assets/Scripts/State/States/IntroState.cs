using Managers;
using S_Machine;
using UI;

namespace State.States
{
    public class IntroState : IState<GameContext>
    {
        public void OnEnter(GameContext context)
        {
            WorldScrollManager.Instance.StopScrolling();
            context.playerBase.InputHandler.DisablePlayerInputHandler();
            context.gameState = Data.GameState.Intro;
            UIMainMenu.Instance.StartIntro();
        }

        public void OnUpdate(GameContext context) { }

        public void OnExit(GameContext context)
        {
            UIMainMenu.Instance.StopIntro();
        }
    }
}