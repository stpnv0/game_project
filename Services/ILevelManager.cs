using ConnectDotsGame.Models;

namespace ConnectDotsGame.Services
{
    public interface ILevelManager
    {
        void ResetLevel(GameState gameState);
        void NextLevel(GameState gameState);
        bool CheckLevelCompletion(GameState gameState);
    }
} 