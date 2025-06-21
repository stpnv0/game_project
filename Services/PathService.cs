using ConnectDotsGame.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using Path = ConnectDotsGame.Models.Path;
using Point = ConnectDotsGame.Models.Point;

namespace ConnectDotsGame.Services
{
    public class PathService : IPathService
    {
        private readonly Path _currentPath = new();

        public string? CurrentPathId => _currentPath.PathId;
        public IBrush? CurrentPathColor => _currentPath.PathColor;
        public List<Point> CurrentPath => _currentPath.Points;
        public Point? LastSelectedPoint => _currentPath.LastSelectedPoint;

        public void StartPath(GameState gameState, Point point)
        {
            if (!point.HasColor || gameState.CurrentLevel == null) return;
            
            ResetPath();
            _currentPath.PathColor = point.Color;
            _currentPath.PathId = $"{point.Color}-path";
            _currentPath.LastSelectedPoint = point;
            _currentPath.Points.Add(point);
            point.IsConnected = true;
        }

        public void ContinuePath(GameState gameState, Point point)
        {
            var level = gameState.CurrentLevel;
            if (level == null || !_currentPath.Points.Any() || !IsNeighbor(_currentPath.Points[^1], point))
                return;

            // Возврат по пути
            var pathIndex = _currentPath.Points.FindIndex(p => p.Row == point.Row && p.Column == point.Column);
            if (pathIndex >= 0)
            {
                _currentPath.Points.RemoveRange(pathIndex + 1, _currentPath.Points.Count - pathIndex - 1);
                _currentPath.LastSelectedPoint = point;
                RedrawPath(level);
                return;
            }

            // Пересечение - проверяем только промежуточные точки
            var crossingLine = level.Lines.FirstOrDefault(l => 
                (l.StartPoint == point && !l.StartPoint.HasColor) || (l.EndPoint == point && !l.EndPoint.HasColor));
                 
            if (crossingLine?.PathId != null && crossingLine.PathId != _currentPath.PathId)
                ClearPath(level, crossingLine.PathId);

            if (IsEndPoint(point))
                CompletePath(level, point);
            else if (CanAddPoint(point))
                AddPoint(level, point);
        }

        public void EndPath(GameState gameState, Point? endPoint = null)
        {
            if (endPoint == null || !_currentPath.Points.Any() || !IsNeighbor(_currentPath.Points[^1], endPoint))
                CancelPath(gameState);
            else if (IsEndPoint(endPoint))
                CompletePath(gameState.CurrentLevel!, endPoint);
            else
                CancelPath(gameState);
        }

        public void CancelPath(GameState gameState)
        {
            if (gameState.CurrentLevel != null && _currentPath.PathId != null)
                ClearPath(gameState.CurrentLevel, _currentPath.PathId);
            ResetPath();
        }

        // Завершает путь, соединяя его с конечной точкой
        private void CompletePath(Level level, Point point)
        {
            _currentPath.Points.Add(point);
            _currentPath.LastSelectedPoint = point;
            point.IsConnected = true;
            _currentPath.Points[0].IsConnected = true;
            CreateLines(level);
            ResetPath();
        }

        // Добавляет новую точку к текущему пути
        private void AddPoint(Level level, Point point)
        {
            _currentPath.Points.Add(point);
            _currentPath.LastSelectedPoint = point;
            RedrawPath(level);
        }

        private bool CanAddPoint(Point point) =>
            !point.HasColor || AreBrushesEqual(point.Color, _currentPath.PathColor);

        // Проверяет, является ли точка конечной для пути
        private bool IsEndPoint(Point point) =>
            point.HasColor && AreBrushesEqual(point.Color, _currentPath.PathColor) && 
            point != _currentPath.Points[0];

        // Проверяет, являются ли точки соседними
        private bool IsNeighbor(Point a, Point b) =>
            Math.Abs(a.Row - b.Row) + Math.Abs(a.Column - b.Column) == 1;

        // Сравнивает цвета
        private static bool AreBrushesEqual(IBrush? a, IBrush? b) =>
            ReferenceEquals(a, b) || 
            (a is SolidColorBrush s1 && b is SolidColorBrush s2 && s1.Color == s2.Color);

        // Перерисовывает путь
        private void RedrawPath(Level level)
        {
            if (_currentPath.PathId != null) ClearPath(level, _currentPath.PathId);
            CreateLines(level);
        }

        private void CreateLines(Level level)
        {
            if (_currentPath.Points.Count < 2) return;

            for (int i = 0; i < _currentPath.Points.Count - 1; i++)
            {
                var line = new Line(_currentPath.Points[i], _currentPath.Points[i + 1], 
                                  _currentPath.PathColor, _currentPath.PathId);
                level.Lines.Add(line);
                
                level.Paths.TryAdd(_currentPath.PathId!, new List<Line>());
                level.Paths[_currentPath.PathId!].Add(line);
            }   
        }

        // Очищает путь с указанным id
        private void ClearPath(Level level, string pathId)
        {
            if (!level.Paths.TryGetValue(pathId, out var lines)) return;

            lines.ForEach(line => level.Lines.Remove(line));
            level.Paths.Remove(pathId);

            // Сброс соединений
            var colorString = pathId.Replace("-path", "");
            level.Points.Where(p => p.HasColor && p.Color?.ToString() == colorString)
                       .ToList().ForEach(p => p.IsConnected = false);
        }

        // Сбрасывает состояние
        private void ResetPath()
        {
            _currentPath.Points.Clear();
            _currentPath.PathColor = null;
            _currentPath.PathId = null;
            _currentPath.LastSelectedPoint = null;
        }

        // Проверяет завершенность уровня
        public bool CheckCompletion(Level level) =>
            level.Points.Where(p => p.HasColor)
                       .GroupBy(p => p.Color?.ToString())
                       .All(g => g.Count() == 2 && g.All(p => p.IsConnected));

        // Возвращает точку по координатам
        public Point? GetPointByPosition(Level level, int row, int column) => 
            level.Points.FirstOrDefault(p => p.Row == row && p.Column == column);
    }
}