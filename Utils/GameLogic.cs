using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using ConnectDotsGame.Models;
using ModelPoint = ConnectDotsGame.Models.Point;

namespace ConnectDotsGame.Utils
{
    /// <summary>
    /// Класс, содержащий логику игры Flow.
    /// </summary>
    public static class GameLogic
    {
        // --- ДОБАВЛЕНО ДЛЯ PathModel ---
        public static bool CheckLevelCompleted(Level level, PathModel pathModel)
        {
            var colorGroups = level.Points
                .Where(p => p.HasColor && p.Color != null)
                .GroupBy(p => p.Color.ToString());

            foreach (var group in colorGroups)
            {
                string colorKey = group.Key ?? "";
                if (!level.Paths.ContainsKey($"{colorKey}-path"))
                    return false;
                if (!group.All(p => p.IsConnected))
                    return false;
            }
            return true;
        }

        public static void ResetLevel(Level level, PathModel pathModel)
        {
            foreach (var point in level.Points)
                point.IsConnected = false;
            level.Lines.Clear();
            level.Paths.Clear();
            pathModel.ResetAll();
            level.IsCompleted = false;
        }
        // --- КОНЕЦ ДОБАВЛЕНИЯ ---

        private static bool AreBrushesEqual(IBrush? brush1, IBrush? brush2)
        {
            if (ReferenceEquals(brush1, brush2)) return true;
            if (brush1 == null || brush2 == null) return false;
            if (brush1 is SolidColorBrush solid1 && brush2 is SolidColorBrush solid2)
                return solid1.Color == solid2.Color;
            return brush1.Equals(brush2);
        }

        private static IBrush? GetPathColorById(Level level, string pathId)
        {
            if (level != null && !string.IsNullOrEmpty(pathId) &&
                level.Paths.TryGetValue(pathId, out var lines) && lines.Any())
            {
                return lines[0].Color;
            }
            return null;
        }

        private static Line? GetOccupyingLine(Level level, ModelPoint point)
        {
            if (level == null) return null;
            foreach (var pathEntry in level.Paths)
            {
                foreach (var line in pathEntry.Value)
                {
                    if (line.EndPoint.Row == point.Row && line.EndPoint.Column == point.Column)
                        return line;
                    if (line.StartPoint.Row == point.Row && line.StartPoint.Column == point.Column)
                        return line;
                }
            }
            return null;
        }

        public static bool TryConnectPoints(GameState gameState, ModelPoint clickedPoint)
        {
            if (gameState.CurrentLevel == null)
                return false;
            var currentLevel = gameState.CurrentLevel;

            if (clickedPoint.HasColor)
            {
                IBrush? pointBrush = clickedPoint.Color;
                if (gameState.LastSelectedPoint == null || !AreBrushesEqual(gameState.CurrentPathColor, pointBrush))
                {
                    Line? occupyingLine = GetOccupyingLine(currentLevel, clickedPoint);
                    if (occupyingLine != null)
                    {
                        if (!AreBrushesEqual(occupyingLine.Color, pointBrush))
                            currentLevel.ClearPath(occupyingLine.PathId);
                        else if (AreBrushesEqual(occupyingLine.Color, pointBrush))
                            currentLevel.ClearPath(occupyingLine.PathId);
                    }
                    gameState.StartNewPath(clickedPoint);
                    return true;
                }

                if (AreBrushesEqual(gameState.CurrentPathColor, pointBrush) && gameState.LastSelectedPoint != clickedPoint)
                {
                    if (CanConnectPoints(currentLevel, gameState.LastSelectedPoint, clickedPoint))
                    {
                        Line? occupyingLine = GetOccupyingLine(currentLevel, clickedPoint);
                        if (occupyingLine != null && !AreBrushesEqual(occupyingLine.Color, gameState.CurrentPathColor))
                            currentLevel.ClearPath(occupyingLine.PathId);

                        if (gameState.CurrentPathColor == null || gameState.CurrentPathId == null)
                            return false;

                        var finalLine = new Line(gameState.LastSelectedPoint, clickedPoint, gameState.CurrentPathColor, gameState.CurrentPathId);
                        currentLevel.Lines.Add(finalLine);
                        currentLevel.AddLineToPaths(finalLine);

                        clickedPoint.IsConnected = true;
                        gameState.CurrentPath.Add(clickedPoint);
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

            // Логика для пустых точек (продолжение пути)
            if (!clickedPoint.HasColor && gameState.LastSelectedPoint != null && gameState.CurrentPathColor != null)
            {
                if (CanConnectPoints(currentLevel, gameState.LastSelectedPoint, clickedPoint))
                {
                    var (isInPath, _) = gameState.CheckPointInPath(clickedPoint);
                    if (isInPath)
                        return false;

                    if (gameState.CurrentPath.Count > 0)
                    {
                        var startPoint = gameState.CurrentPath[0];
                        if (startPoint.Row == clickedPoint.Row && startPoint.Column == clickedPoint.Column)
                            return false;
                    }

                    foreach (var pathEntry in currentLevel.Paths)
                    {
                        if (gameState.CurrentPathColor != null &&
                            pathEntry.Key.StartsWith(gameState.CurrentPathColor.ToString() ?? ""))
                        {
                            foreach (var line in pathEntry.Value)
                            {
                                if ((line.StartPoint.Row == clickedPoint.Row && line.StartPoint.Column == clickedPoint.Column) ||
                                    (line.EndPoint.Row == clickedPoint.Row && line.EndPoint.Column == clickedPoint.Column))
                                {
                                    return false;
                                }
                            }
                        }
                    }

                    Line? occupyingLine = GetOccupyingLine(currentLevel, clickedPoint);
                    if (occupyingLine != null)
                    {
                        if (AreBrushesEqual(occupyingLine.Color, gameState.CurrentPathColor))
                            return false;
                        else
                            currentLevel.ClearPath(occupyingLine.PathId);
                    }

                    if (gameState.CurrentPathId == null)
                        return false;

                    var newLine = new Line(gameState.LastSelectedPoint, clickedPoint, gameState.CurrentPathColor, gameState.CurrentPathId);
                    currentLevel.Lines.Add(newLine);
                    currentLevel.AddLineToPaths(newLine);

                    gameState.LastSelectedPoint = clickedPoint;
                    gameState.CurrentPath.Add(clickedPoint);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        public static bool CanConnectPoints(Level level, ModelPoint? startPoint, ModelPoint endPoint)
        {
            if (startPoint == null) return false;
            int rowDiff = Math.Abs(endPoint.Row - startPoint.Row);
            int colDiff = Math.Abs(endPoint.Column - startPoint.Column);
            bool areAdjacent = (rowDiff == 0 && colDiff == 1) || (rowDiff == 1 && colDiff == 0);
            return areAdjacent;
        }

        public static string? CheckForCrossingPath(Level level, ModelPoint point)
        {
            Line? occupyingLine = GetOccupyingLine(level, point);
            return occupyingLine?.PathId;
        }

        public static ModelPoint? FindPointAtPosition(Level level, double x, double y, double cellSize)
        {
            if (level == null) return null;
            double clickRadius = cellSize * 0.7;
            foreach (var point in level.Points)
            {
                double pointX = point.Column * cellSize + cellSize / 2;
                double pointY = point.Row * cellSize + cellSize / 2;
                double distance = Math.Sqrt(Math.Pow(x - pointX, 2) + Math.Pow(y - pointY, 2));
                if (distance <= clickRadius)
                    return point;
            }
            return null;
        }

        public static double CalculateDistance(ModelPoint p1, ModelPoint p2)
        {
            return Math.Abs(p1.Row - p2.Row) + Math.Abs(p1.Column - p2.Column);
        }

        public static ModelPoint? FindNearestPoint(Level level, double x, double y, double threshold = 20)
        {
            ModelPoint? nearestPoint = null;
            double minDistance = double.MaxValue;
            foreach (var point in level.Points)
            {
                double distance = Math.Sqrt(Math.Pow(point.X - x, 2) + Math.Pow(point.Y - y, 2));
                if (distance < minDistance && distance <= threshold)
                {
                    minDistance = distance;
                    nearestPoint = point;
                }
            }
            return nearestPoint;
        }

        private static bool IsPathPossible(Level level, ModelPoint start, ModelPoint end)
        {
            bool[,] visited = new bool[level.Rows, level.Columns];
            var queue = new Queue<Tuple<ModelPoint, List<ModelPoint>>>();
            var startPath = new List<ModelPoint> { start };
            queue.Enqueue(Tuple.Create(start, startPath));
            visited[start.Row, start.Column] = true;

            int[] dx = { -1, 0, 1, 0 };
            int[] dy = { 0, 1, 0, -1 };

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var currentPoint = current.Item1;
                var currentPath = current.Item2;

                if (currentPoint.Row == end.Row && currentPoint.Column == end.Column)
                    return true;

                for (int dir = 0; dir < 4; dir++)
                {
                    int newRow = currentPoint.Row + dx[dir];
                    int newCol = currentPoint.Column + dy[dir];

                    if (newRow >= 0 && newRow < level.Rows && newCol >= 0 && newCol < level.Columns)
                    {
                        if (!visited[newRow, newCol])
                        {
                            var nextPoint = level.GetPointByPosition(newRow, newCol);
                            if (nextPoint != null)
                            {
                                if (!nextPoint.HasColor || (nextPoint.Row == end.Row && nextPoint.Column == end.Column))
                                {
                                    if (GetOccupyingLine(level, nextPoint) == null ||
                                        (nextPoint.Row == end.Row && nextPoint.Column == end.Column))
                                    {
                                        var newPath = new List<ModelPoint>(currentPath) { nextPoint };
                                        queue.Enqueue(Tuple.Create(nextPoint, newPath));
                                        visited[newRow, newCol] = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}