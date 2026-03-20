using Managers;
using S_Machine;
using UI;

namespace State.States
{
    public class PlayingState : IState<GameContext>
    {
        public void OnEnter(GameContext context)
        {
            context.playerBase.InputHandler.EnablePlayerInputHandler();
            WorldScrollManager.Instance.ResumeScrolling();
            
            UIGameMain.Instance.SetStatusElementsActive(true);
        }

        public void OnUpdate(GameContext context) { }

        public void OnExit(GameContext context)
        {
            UIGameMain.Instance.SetStatusElementsActive(false);
        }
    }
}