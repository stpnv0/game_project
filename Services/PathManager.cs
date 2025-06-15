using ConnectDotsGame.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;

namespace ConnectDotsGame.Services
{
    public class PathManager : IPathManager
    {
        private readonly Path _currentPath = new Path();

        // Проверяет, являются ли две точки соседями по сетке
        private static bool IsNeighbor(Point a, Point b)
        {
            return (Math.Abs(a.Row - b.Row) == 1 && a.Column == b.Column) ||
                   (Math.Abs(a.Column - b.Column) == 1 && a.Row == b.Row);
        }

        // Сравнивает две кисти на равенство с учетом их типов и значений
        private static bool AreBrushesEqual(IBrush? brush1, IBrush? brush2)
        {
            if (ReferenceEquals(brush1, brush2)) return true;
            if (brush1 == null || brush2 == null) return false;
            if (brush1 is SolidColorBrush solid1 && brush2 is SolidColorBrush solid2)
                return solid1.Color == solid2.Color;
            return brush1.Equals(brush2);
        }

        public bool TryConnectPoints(GameState gameState, Point clickedPoint)
        {
            if (_currentPath.PathColor == null || clickedPoint.Color == null)
                return false;

            if (!AreBrushesEqual(_currentPath.PathColor, clickedPoint.Color) || 
                _currentPath.LastSelectedPoint == clickedPoint)
                return false;

            if (_currentPath.LastSelectedPoint == null || !IsNeighbor(_currentPath.LastSelectedPoint, clickedPoint))
                return false;

            if (gameState.CurrentLevel == null)
                return false;

            bool isCompleted = CheckCompletion(gameState.CurrentLevel);
            if (isCompleted)
            {
                gameState.CurrentLevel.WasEverCompleted = true;
            }
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
                ClearPath(gameState.CurrentLevel!, crossingPath);
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
            RedrawCurrentPath(gameState, _currentPath.PathColor!.ToString()!, _currentPath.Points);
        }

        private bool CanContinuePath(Point point)
        {
            if (!point.HasColor)
                return true;

            return AreBrushesEqual(point.Color, _currentPath.PathColor);
        }

        private bool IsEndPoint(Point point)
        {
            return point.HasColor && 
                   AreBrushesEqual(point.Color, _currentPath.PathColor) && 
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
                AreBrushesEqual(startPoint.Color, _currentPath.PathColor) &&
                AreBrushesEqual(endPoint.Color, _currentPath.PathColor))
            {
                startPoint.IsConnected = true;
                endPoint.IsConnected = true;
            }
            
            ResetPath();
        }

        private void AddPointToPath(GameState gameState, Point point)
        {
            _currentPath.Points.Add(point);
            _currentPath.LastSelectedPoint = point;
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
            if (!endPoint.HasColor || !AreBrushesEqual(endPoint.Color, _currentPath.PathColor))
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

            ClearPath(gameState.CurrentLevel, _currentPath.PathId ?? "");
            ResetPath();
        }

        public void RedrawCurrentPath(GameState gameState, string colorKey, List<Point> pathPoints)
        {
            var level = gameState.CurrentLevel;
            if (level == null || pathPoints.Count < 2)
                return;

            ClearPath(level, _currentPath.PathId ?? "");
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
                var line = new Line(startPoint, endPoint, _currentPath.PathColor, _currentPath.PathId);
                
                level.Lines.Add(line);
                AddLineToPaths(level, line);
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
        public Point? LastSelectedPoint => _currentPath.LastSelectedPoint;

        public bool IsPathComplete(Level level, IBrush? color)
        {
            if (color == null)
                return false;

            string pathId = $"{color}-path";
            if (!level.Paths.ContainsKey(pathId))
                return false;
                
            var colorPoints = level.Points.Where(p => p.Color != null && p.Color.ToString() == color.ToString()).ToList();
            
            if (colorPoints.Count != 2)
                return false;
                
            return colorPoints.All(p => p.IsConnected);
        }

        public bool CheckCompletion(Level level)
        {
            var colorGroups = level.Points.Where(p => p.HasColor && p.Color != null)
                                  .GroupBy(p => p.Color);
                                  
            return colorGroups.All(group => IsPathComplete(level, group.Key));
        }

        public void AddLineToPaths(Level level, Line line)
        {
            if (line == null)
                return;
                
            if (!level.Paths.ContainsKey(line.PathId))
            {
                level.Paths[line.PathId] = new List<Line>();
            }
            
            level.Paths[line.PathId].Add(line);
        }

        public void ClearPath(Level level, string pathId)
        {
            if (level.Paths.ContainsKey(pathId))
            {
                var linesToRemove = level.Paths[pathId].ToList();
                
                foreach (var line in linesToRemove)
                {
                    level.Lines.Remove(line);
                }
                
                level.Paths.Remove(pathId);
                
                // Сброс статуса соединения
                var color = pathId.Replace("-path", "");
                var colorPoints = level.Points.Where(p => p.HasColor && p.Color != null && 
                                                p.Color.ToString() == color).ToList();
                foreach (var point in colorPoints)
                {
                    point.IsConnected = false;
                }
            }
        }

        public Point? GetPointByPosition(Level level, int row, int column)
        {
            return level.Points.FirstOrDefault(p => p.Row == row && p.Column == column);
        }
    }
} 