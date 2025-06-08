using ConnectDotsGame.Models;
using System.Collections.Generic;

namespace ConnectDotsGame.Services
{
    public interface IGameService
    {
        // Пытается соединить две точки на игровом поле.
        bool TryConnectPoints(GameState gameState, Point clickedPoint);
        // Начинает новый путь от точки.
        void StartPath(GameState gameState, Point point);
        // Продолжает путь через точку.
        void ContinuePath(GameState gameState, Point point);
        // Завершает путь на указанной точке.
        void EndPath(GameState gameState, Point? endPoint = null);
        // Отменяет текущий путь.
        void CancelPath(GameState gameState);
        // Сбрасывает состояние текущего уровня.
        void ResetLevel(GameState gameState);
        // Переходит к следующему уровню.
        void NextLevel(GameState gameState);
        // Проверяет завершён ли уровень.
        bool CheckLevelCompletion(GameState gameState);
        // Перерисовывает текущий путь на игровом поле.
        void RedrawCurrentPath(GameState gameState, string colorKey, List<Point> pathPoints);
        // Создаёт линии для пути на игровом поле.
        void CreatePathLines(GameState gameState, string colorKey, List<Point> pathPoints);
        // Находит идентификатор пути, проходящего через точку.
        string? FindCrossingPath(Level level, Point point);
    }
} 