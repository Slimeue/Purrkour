using Managers;
using S_Machine;
using UI;

namespace State.States
{
    public class MainMenuState : IState<GameContext>
    {
        public void OnEnter(GameContext context)
        {
            UIMainMenu.Instance.SetStatus(true);
            WorldScrollManager.Instance.StopScrolling();
            context.playerBase.InputHandler.DisablePlayerInputHandler();
        }

        public void OnUpdate(GameContext context)
        { }

        public void OnExit(GameContext context)
        {
            UIMainMenu.Instance.SetStatus(false);
        }
    }
}