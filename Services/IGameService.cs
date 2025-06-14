using ConnectDotsGame.Models;

namespace ConnectDotsGame.Services
{
    public interface IGameService
    {
        // Сбрасывает состояние текущего уровня.
        void ResetLevel(GameState gameState);
        // Переходит к следующему уровню.
        void NextLevel(GameState gameState);
        // Проверяет завершён ли уровень.
        bool CheckLevelCompletion(GameState gameState);
        // Обновляет ссылку на сервис навигации.
        void SetNavigation(INavigation navigation);
    }
} 