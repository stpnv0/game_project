using ConnectDotsGame.Models;
using ConnectDotsGame.Utils;
using System.Collections.Generic;
using System.Linq;
using static ConnectDotsGame.Utils.PointLocator;
using Avalonia.Media;
using BrushExt = ConnectDotsGame.Utils.BrushExtensions;
using GamePath = ConnectDotsGame.Models.Path;

namespace ConnectDotsGame.Services
{
    public class PathManager : IPathManager
    {
        private readonly GamePath _currentPath = new GamePath();

        public bool TryConnectPoints(GameState gameState, Point clickedPoint)
        {
            if (_currentPath.PathColor == null || clickedPoint.Color == null)
                return false;

            if (!BrushExt.AreBrushesEqual(_currentPath.PathColor, clickedPoint.Color) || 
                _currentPath.LastSelectedPoint == clickedPoint)
                return false;

            if (_currentPath.LastSelectedPoint == null || !IsNeighbor(_currentPath.LastSelectedPoint, clickedPoint))
                return false;

            gameState.CheckCompletedPaths();
            ResetPath();
            return true;
        }

        public void StartPath(GameState gameState, Point point)
        {
            if (gameState.CurrentLevel == null || !point.HasColor || point.Color == null)
                return;

            ResetPath();
            _currentPath.LastSelectedPoint = point;
            _currentPath.PathColor = point.Color;
            _currentPath.PathId = $"{point.Color}-path";
            point.IsConnected = true;
            _currentPath.Points.Add(point);
            
            gameState.LastSelectedPoint = point;
        }

        public void ContinuePath(GameState gameState, Point point)
        {
            var level = gameState.CurrentLevel;
            if (level == null || _currentPath.PathColor == null || _currentPath.Points.Count == 0)
                return;

            var lastPoint = _currentPath.Points[^1];
            if (lastPoint.Row == point.Row && lastPoint.Column == point.Column)
                return;

            if (!IsNeighbor(lastPoint, point))
                return;

            HandlePathContinuation(gameState, point);
        }

        private void HandlePathContinuation(GameState gameState, Point point)
        {
            // Проверка на возврат по пути
            var (isInPath, returnIndex) = CheckPointInPath(point);
            if (isInPath)
            {
                HandlePathReturn(gameState, returnIndex, point);
                return;
            }

            // Проверка на пересечение с другим путем
            var crossingPath = FindCrossingPath(gameState.CurrentLevel!, point);
            if (crossingPath != null)
            {
                gameState.CurrentLevel!.ClearPath(crossingPath);
            }

            if (!CanContinuePath(point))
                return;

            if (IsEndPoint(point))
            {
                CompletePath(gameState, point);
                return;
            }

            AddPointToPath(gameState, point);
        }

        private void HandlePathReturn(GameState gameState, int returnIndex, Point point)
        {
            RemovePointsAfter(returnIndex);
            _currentPath.LastSelectedPoint = point;
            gameState.LastSelectedPoint = point;
            RedrawCurrentPath(gameState, _currentPath.PathColor!.ToString()!, _currentPath.Points);
        }

        private bool CanContinuePath(Point point)
        {
            if (!point.HasColor)
                return true;

            return BrushExt.AreBrushesEqual(point.Color, _currentPath.PathColor);
        }

        private bool IsEndPoint(Point point)
        {
            return point.HasColor && 
                   BrushExt.AreBrushesEqual(point.Color, _currentPath.PathColor) && 
                   point != _currentPath.Points[0];
        }

        private void CompletePath(GameState gameState, Point point)
        {
            _currentPath.Points.Add(point);
            CreatePathLines(gameState, _currentPath.PathColor!.ToString()!, _currentPath.Points);
            
            // Находим и помечаем обе конечные точки как соединенные
            var startPoint = _currentPath.Points[0];
            var endPoint = point;
            
            if (startPoint.HasColor && endPoint.HasColor && 
                BrushExt.AreBrushesEqual(startPoint.Color, _currentPath.PathColor) &&
                BrushExt.AreBrushesEqual(endPoint.Color, _currentPath.PathColor))
            {
                startPoint.IsConnected = true;
                endPoint.IsConnected = true;
            }
            
            ResetPath();
            gameState.LastSelectedPoint = null;
        }

        private void AddPointToPath(GameState gameState, Point point)
        {
            _currentPath.Points.Add(point);
            _currentPath.LastSelectedPoint = point;
            gameState.LastSelectedPoint = point;
            RedrawCurrentPath(gameState, _currentPath.PathColor!.ToString()!, _currentPath.Points);
        }

        public void EndPath(GameState gameState, Point? endPoint = null)
        {
            if (gameState.CurrentLevel == null || _currentPath.PathColor == null || _currentPath.Points.Count == 0)
                return;

            if (endPoint == null || !IsValidEndPoint(endPoint))
            {
                CancelPath(gameState);
                return;
            }

            CompletePath(gameState, endPoint);
        }

        private bool IsValidEndPoint(Point endPoint)
        {
            if (!endPoint.HasColor || !BrushExt.AreBrushesEqual(endPoint.Color, _currentPath.PathColor))
                return false;

            if (_currentPath.Points[0] == endPoint)
                return false;

            var lastPoint = _currentPath.Points[^1];
            return IsNeighbor(lastPoint, endPoint);
        }

        public void CancelPath(GameState gameState)
        {
            if (gameState.CurrentLevel == null || _currentPath.PathColor == null)
                return;

            gameState.CurrentLevel.ClearPath(_currentPath.PathId ?? "");
            ResetPath();
            gameState.LastSelectedPoint = null;
        }

        public void RedrawCurrentPath(GameState gameState, string colorKey, List<Point> pathPoints)
        {
            var level = gameState.CurrentLevel;
            if (level == null || pathPoints.Count < 2)
                return;

            level.ClearPath(_currentPath.PathId ?? "");
            CreatePathLines(gameState, colorKey, pathPoints);
        }

        public void CreatePathLines(GameState gameState, string colorKey, List<Point> pathPoints)
        {
            var level = gameState.CurrentLevel;
            if (level == null || pathPoints.Count < 2)
                return;

            for (int i = 0; i < pathPoints.Count - 1; i++)
            {
                var startPoint = pathPoints[i];
                var endPoint = pathPoints[i + 1];
                var line = new Line(startPoint, endPoint, _currentPath.PathColor, _currentPath.PathId)
                {
                    PathId = _currentPath.PathId ?? ""
                };
                
                level.Lines.Add(line);
                level.AddLineToPaths(line);
            }
        }

        public string? FindCrossingPath(Level level, Point point)
        {
            return level.Paths
                .FirstOrDefault(path => path.Value.Any(line =>
                    (line.StartPoint.Row == point.Row && line.StartPoint.Column == point.Column) ||
                    (line.EndPoint.Row == point.Row && line.EndPoint.Column == point.Column)))
                .Key;
        }

        private void ResetPath()
        {
            _currentPath.LastSelectedPoint = null;
            _currentPath.PathColor = null;
            _currentPath.Points.Clear();
            _currentPath.PathId = null;
        }

        private (bool isInPath, int index) CheckPointInPath(Point point)
        {
            int index = _currentPath.Points.FindIndex(p => p.Row == point.Row && p.Column == point.Column);
            return (index != -1, index);
        }

        private bool RemovePointsAfter(int index)
        {
            if (index < 0 || index >= _currentPath.Points.Count - 1)
                return false;

            _currentPath.Points.RemoveRange(index + 1, _currentPath.Points.Count - index - 1);
            _currentPath.LastSelectedPoint = _currentPath.Points.Count > 0 ? _currentPath.Points[^1] : null;

            return true;
        }

        // Публичные свойства для доступа к текущему пути
        public string? CurrentPathId => _currentPath.PathId;
        public IBrush? CurrentPathColor => _currentPath.PathColor;
        public List<Point> CurrentPath => _currentPath.Points;
    }
} 