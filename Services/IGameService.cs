using ConnectDotsGame.Models;

namespace ConnectDotsGame.Services
{
    public interface IGameService
    {
        // Обновляет ссылку на сервис навигации.
        void SetNavigation(INavigation navigation);
        void ResetAllPaths(GameState gameState);
        // Переходит к следующему уровню.
        void NextLevel(GameState gameState);
        // Проверяет завершён ли уровень.
        bool CheckLevelCompletion(GameState gameState);
        void LoadProgress(GameState gameState);
        void SaveProgress(GameState gameState);
    }
} 