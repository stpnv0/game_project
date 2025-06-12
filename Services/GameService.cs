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

        public GameService(INavigation navigation, IModalService modalService)
        {
            _navigation = navigation;
            _modalService = modalService;
        }

        // Пытается соединить две точки на игровом поле.
        public bool TryConnectPoints(GameState gameState, Point clickedPoint)
        {
            IBrush? pointBrush = clickedPoint.Color;
            // Завершаем путь, кликнув на вторую точку того же цвета
            if (ConnectDotsGame.Utils.BrushExtensions.AreBrushesEqual(gameState.CurrentPathColor, pointBrush) && gameState.LastSelectedPoint != clickedPoint)
            {
                if (gameState.LastSelectedPoint != null && IsNeighbor(gameState.LastSelectedPoint, clickedPoint))
                {
                    gameState.CheckCompletedPaths();
                    gameState.ResetPathState();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        // Начинает новый путь от точки.
        public void StartPath(GameState gameState, Point point)
        {
            if (gameState.CurrentLevel == null || !point.HasColor || point.Color == null)
                return;
            gameState.StartNewPath(point);
        }

        // Продолжает путь через точку.
        public void ContinuePath(GameState gameState, Point point)
        {
            var level = gameState.CurrentLevel;
            if (level == null || gameState.CurrentPathColor == null || gameState.CurrentPath.Count == 0)
                return;
            var lastPoint = gameState.CurrentPath[^1];
            if (lastPoint.Row == point.Row && lastPoint.Column == point.Column)
                return;
            if (!IsNeighbor(lastPoint, point))
                return;
            // Проверка на возврат по пути
            int returnIndex = gameState.CurrentPath.FindIndex(p => p.Row == point.Row && p.Column == point.Column);
            if (returnIndex != -1)
            {
                while (gameState.CurrentPath.Count > returnIndex + 1)
                {
                    gameState.CurrentPath.RemoveAt(gameState.CurrentPath.Count - 1);
                }
                gameState.LastSelectedPoint = point;
                RedrawCurrentPath(gameState, gameState.CurrentPathColor.ToString() ?? "", gameState.CurrentPath);
                return;
            }
            // Проверка на пересечение с другим путем
            var crossingPath = FindCrossingPath(level, point);
            if (crossingPath != null)
            {
                level.ClearPath(crossingPath);
            }
            // Если точка уже имеет другой цвет — запрещаем
            if (point.HasColor && !ConnectDotsGame.Utils.BrushExtensions.AreBrushesEqual(point.Color, gameState.CurrentPathColor))
                return;
            // Если это конечная точка нашего цвета
            if (point.HasColor && ConnectDotsGame.Utils.BrushExtensions.AreBrushesEqual(point.Color, gameState.CurrentPathColor) && point != gameState.CurrentPath[0])
            {
                gameState.CurrentPath.Add(point);
                CreatePathLines(gameState, gameState.CurrentPathColor.ToString() ?? "", gameState.CurrentPath);
                foreach (var pathPoint in gameState.CurrentPath)
                {
                    if (pathPoint.HasColor && ConnectDotsGame.Utils.BrushExtensions.AreBrushesEqual(pathPoint.Color, gameState.CurrentPathColor))
                    {
                        pathPoint.IsConnected = true;
                    }
                }
                gameState.ResetPathState();
                CheckLevelCompletion(gameState);
                return;
            }
            // Добавляем точку в путь
            gameState.CurrentPath.Add(point);
            gameState.LastSelectedPoint = point;
            RedrawCurrentPath(gameState, gameState.CurrentPathColor.ToString() ?? "", gameState.CurrentPath);
        }

        /// Завершает путь на указанной точке.
        public void EndPath(GameState gameState, Point? endPoint = null)
        {
            if (gameState.CurrentLevel == null || gameState.CurrentPathColor == null || gameState.CurrentPath.Count == 0)
                return;
            if (endPoint == null)
            {
                CancelPath(gameState);
                return;
            }
            if (!endPoint.HasColor || !ConnectDotsGame.Utils.BrushExtensions.AreBrushesEqual(endPoint.Color, gameState.CurrentPathColor))
            {
                CancelPath(gameState);
                return;
            }
            if (gameState.CurrentPath[0] == endPoint)
            {
                CancelPath(gameState);
                return;
            }
            var lastPoint = gameState.CurrentPath[^1];
            if (!IsNeighbor(lastPoint, endPoint))
            {
                CancelPath(gameState);
                return;
            }
            if (!gameState.CurrentPath.Contains(endPoint))
            {
                gameState.CurrentPath.Add(endPoint);
                CreatePathLines(gameState, gameState.CurrentPathColor.ToString() ?? "", gameState.CurrentPath);
                foreach (var pathPoint in gameState.CurrentPath)
                {
                    if (pathPoint.HasColor && ConnectDotsGame.Utils.BrushExtensions.AreBrushesEqual(pathPoint.Color, gameState.CurrentPathColor))
                    {
                        pathPoint.IsConnected = true;
                    }
                }
            }
            gameState.ResetPathState();
            CheckLevelCompletion(gameState);
        }

        /// Отменяет текущий путь.
        public void CancelPath(GameState gameState)
        {
            if (gameState.CurrentLevel == null || gameState.CurrentPathColor == null)
                return;
            gameState.CurrentLevel.ClearPath(gameState.CurrentPathId ?? "");
            gameState.ResetPathState();
        }

        /// Сбрасывает состояние текущего уровня.
        public void ResetLevel(GameState gameState)
        {
            gameState.ResetCurrentLevel();
        }

        /// Переходит к следующему уровню.
        public void NextLevel(GameState gameState)
        {
            gameState.GoToNextLevel();
        }

        // Проверяет завершён ли уровень.
        public bool CheckLevelCompletion(GameState gameState)
        {
            if (gameState.CurrentLevel == null)
                return false;
            bool allCompleted = gameState.CurrentLevel.CheckCompletion();
            if (allCompleted)
            {
                gameState.CurrentLevel.WasEverCompleted = true;
                gameState.SaveProgress();
                if (!gameState.HasNextLevel)
                {
                    _modalService.ShowModal(
                        "Поздравляем!",
                        "Вы прошли все уровни!",
                        "В меню",
                        () => _navigation.NavigateTo<LevelSelectViewModel>()
                    );
                }
                else
                {
                    _modalService.ShowModal(
                        "Уровень завершён!",
                        $"Вы успешно завершили {gameState.CurrentLevel.Name}",
                        "Следующий уровень",
                        () =>
                        {
                            if (gameState.GoToNextLevel())
                            {
                                _navigation.NavigateTo<GameViewModel>(gameState);
                            }
                        }
                    );
                }
            }
            return allCompleted;
        }

        /// Перерисовывает текущий путь на игровом поле.
        public void RedrawCurrentPath(GameState gameState, string colorKey, List<Point> pathPoints)
        {
            var level = gameState.CurrentLevel;
            if (level == null || pathPoints.Count < 2)
                return;
            level.ClearPath(gameState.CurrentPathId ?? "");
            for (int i = 0; i < pathPoints.Count - 1; i++)
            {
                var startPoint = pathPoints[i];
                var endPoint = pathPoints[i + 1];
                var line = new Line(startPoint, endPoint, gameState.CurrentPathColor, gameState.CurrentPathId);
                line.PathId = gameState.CurrentPathId ?? "";
                level.Lines.Add(line);
                level.AddLineToPaths(line);
            }
        }

        /// Создаёт линии для пути на игровом поле.
        public void CreatePathLines(GameState gameState, string colorKey, List<Point> pathPoints)
        {
            var level = gameState.CurrentLevel;
            if (level == null || pathPoints.Count < 2)
                return;
            for (int i = 0; i < pathPoints.Count - 1; i++)
            {
                var startPoint = pathPoints[i];
                var endPoint = pathPoints[i + 1];
                var line = new Line(startPoint, endPoint, gameState.CurrentPathColor, gameState.CurrentPathId);
                line.PathId = gameState.CurrentPathId ?? "";
                level.Lines.Add(line);
                level.AddLineToPaths(line);
            }
        }

        /// Находит идентификатор пути, проходящего через точку.
        public string? FindCrossingPath(Level level, Point point)
        {
            foreach (var path in level.Paths)
            {
                if (path.Value.Any(line =>
                    (line.StartPoint.Row == point.Row && line.StartPoint.Column == point.Column) ||
                    (line.EndPoint.Row == point.Row && line.EndPoint.Column == point.Column)))
                {
                    return path.Key;
                }
            }
            return null;
        }
    }
} 
