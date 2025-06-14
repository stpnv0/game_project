using ConnectDotsGame.Models;
using ConnectDotsGame.Utils;
using ConnectDotsGame.Services;
using ConnectDotsGame.ViewModels;
using Avalonia.Media;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Layout;
using static ConnectDotsGame.Utils.PointLocator;

namespace ConnectDotsGame.Services
{
    public class GameService : IGameService
    {
        private readonly INavigation _navigation;
        private readonly IModalService _modalService;
        private readonly IPathManager _pathManager;
        private readonly ILevelManager _levelManager;

        public GameService(INavigation navigation, IModalService modalService, IPathManager pathManager, ILevelManager levelManager)
        {
            _navigation = navigation;
            _modalService = modalService;
            _pathManager = pathManager;
            _levelManager = levelManager;
        }

        // Пытается соединить две точки на игровом поле.
        public bool TryConnectPoints(GameState gameState, Point clickedPoint)
        {
            return _pathManager.TryConnectPoints(gameState, clickedPoint);
        }

        // Начинает новый путь от точки.
        public void StartPath(GameState gameState, Point point)
        {
            _pathManager.StartPath(gameState, point);
        }

        // Продолжает путь через точку.
        public void ContinuePath(GameState gameState, Point point)
        {
            _pathManager.ContinuePath(gameState, point);
        }

        /// Завершает путь на указанной точке.
        public void EndPath(GameState gameState, Point? endPoint = null)
        {
            _pathManager.EndPath(gameState, endPoint);
        }

        /// Отменяет текущий путь.
        public void CancelPath(GameState gameState)
        {
            _pathManager.CancelPath(gameState);
        }

        /// Сбрасывает состояние текущего уровня.
        public void ResetLevel(GameState gameState)
        {
            _levelManager.ResetLevel(gameState);
        }

        /// Переходит к следующему уровню.
        public void NextLevel(GameState gameState)
        {
            _levelManager.NextLevel(gameState);
        }

        // Проверяет завершён ли уровень.
        public bool CheckLevelCompletion(GameState gameState)
        {
            return _levelManager.CheckLevelCompletion(gameState);
        }

        /// Перерисовывает текущий путь на игровом поле.
        public void RedrawCurrentPath(GameState gameState, string colorKey, List<Point> pathPoints)
        {
            _pathManager.RedrawCurrentPath(gameState, colorKey, pathPoints);
        }

        /// Создаёт линии для пути на игровом поле.
        public void CreatePathLines(GameState gameState, string colorKey, List<Point> pathPoints)
        {
            _pathManager.CreatePathLines(gameState, colorKey, pathPoints);
        }

        /// Находит идентификатор пути, проходящего через точку.
        public string? FindCrossingPath(Level level, Point point)
        {
            return _pathManager.FindCrossingPath(level, point);
        }
    }
} 
