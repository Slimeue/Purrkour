using Managers;
using Player;
namespace State
{
    public class GameContext
    {
        public GameManager gameManager;
        public UIManager uiManager;
        public PlayerBase playerBase;
        public Data.GameState gameState;
    }
}