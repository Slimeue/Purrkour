using Managers;
using S_Machine;
using UI;
using UnityEngine;

namespace State.States
{
    public class MainMenuState : IState<GameContext>
    {
        public void OnEnter(GameContext context)
        {
            UIMainMenu.Instance.SetStatus(true);
            WorldScrollManager.Instance.StopScrolling();
            context.playerBase.InputHandler.DisablePlayerInputHandler();
            
            
            if (PlayerPrefs.GetInt($"{UIMainMenu.Instance._defaultTutorial.title}", 0) == 0)
            {
                UIMainMenu.Instance.QuickTutorial();
            }
            else
            {
                AudioManager.Instance.PlayBgm(Data.SoundId.MainMenu);
            }
            
            context.gameState = Data.GameState.MainMenu;
        }

        public void OnUpdate(GameContext context)
        { }

        public void OnExit(GameContext context)
        {
            UIMainMenu.Instance.SetStatus(false);
        }
    }
}