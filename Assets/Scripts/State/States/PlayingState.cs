using Core;
using Managers;
using S_Machine;
using UI;

namespace State.States
{
    public class PlayingState : IState<GameContext>
    {
        public void OnEnter(GameContext context)
        {
            PointsManager.Instance.OnPointsChanged += UIGameMain.Instance.ChangePoints;
            
            PointsManager.Instance.PointsChange();
            
            WorldScrollManager.Instance.ResumeScrolling();
            UIGameMain.Instance.SetStatusElementsActive(true);
            PlatformGenerator.Instance.InitializeGeneration();
            
            context.playerBase.InputHandler.EnablePlayerInputHandler();
            context.gameState = Data.GameState.Playing;
        }

        public void OnUpdate(GameContext context) { }

        public void OnExit(GameContext context)
        {
            UIGameMain.Instance.SetStatusElementsActive(false);
            PointsManager.Instance.OnPointsChanged -= UIGameMain.Instance.ChangePoints;
        }
    }
}