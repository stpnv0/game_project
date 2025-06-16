using ConnectDotsGame.Models;

namespace ConnectDotsGame.Services
{
    public interface IGameService
    {
        void SetNavigation(INavigation navigation);
        void ResetAllPaths(GameState gameState);
        void NextLevel(GameState gameState);
        bool CheckLevelCompletion(GameState gameState);
        void LoadProgress(GameState gameState);
        void SaveProgress(GameState gameState);
    }
} 