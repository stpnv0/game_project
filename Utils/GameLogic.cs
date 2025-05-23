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
        /// <summary>
        /// Сравнивает два объекта IBrush, уделяя особое внимание SolidColorBrush.
        /// </summary>
        /// <param name="brush1">Первая кисть для сравнения.</param>
        /// <param name="brush2">Вторая кисть для сравнения.</param>
        /// <returns>True, если кисти равны; иначе false.</returns>
        private static bool AreBrushesEqual(IBrush? brush1, IBrush? brush2)
        {
            if (ReferenceEquals(brush1, brush2)) return true;
            if (brush1 == null || brush2 == null) return false;

            if (brush1 is SolidColorBrush solid1 && brush2 is SolidColorBrush solid2)
            {
                return solid1.Color == solid2.Color;
            }
            return brush1.Equals(brush2);
        }

        /// <summary>
        /// Получает цвет пути по его идентификатору.
        /// </summary>
        /// <param name="level">Текущий уровень.</param>
        /// <param name="pathId">Идентификатор пути.</param>
        /// <returns>Цвет пути или null, если путь не найден.</returns>
        private static IBrush? GetPathColorById(Level level, string pathId)
        {
            if (level != null && !string.IsNullOrEmpty(pathId) && 
                level.Paths.TryGetValue(pathId, out var lines) && lines.Any())
            {
                return lines[0].Color;
            }
            return null;
        }

        /// <summary>
        /// Проверяет, занята ли указанная точка сегментом линии существующего пути.
        /// </summary>
        /// <param name="level">Текущий уровень.</param>
        /// <param name="point">Точка для проверки.</param>
        /// <returns>Объект Line, занимающий точку, или null, если точка свободна.</returns>
        private static Line? GetOccupyingLine(Level level, ModelPoint point)
        {
            if (level == null) return null;

            foreach (var pathEntry in level.Paths)
            {
                foreach (var line in pathEntry.Value)
                {
                    // Проверяем, является ли точка конечной точкой сегмента
                    if (line.EndPoint.Row == point.Row && line.EndPoint.Column == point.Column)
                    {
                        return line;
                    }

                    // Проверяем, является ли точка начальной точкой сегмента
                    // Всегда считаем точку занятой, если она является начальной точкой сегмента
                    // Это предотвратит прохождение через начальную точку пути
                    if (line.StartPoint.Row == point.Row && 
                        line.StartPoint.Column == point.Column)
                    {
                        return line;
                    }
                }
            }
            return null; // Точка не занята ни одним сегментом линии
        }

        /// <summary>
        /// Основной метод логики соединения точек.
        /// </summary>
        /// <param name="gameState">Текущее состояние игры.</param>
        /// <param name="clickedPoint">Точка, на которую нажал пользователь.</param>
        /// <returns>True, если соединение успешно; иначе false.</returns>
        public static bool TryConnectPoints(GameState gameState, ModelPoint clickedPoint)
        {
            if (gameState.CurrentLevel == null)
                return false;

            var currentLevel = gameState.CurrentLevel;

            // Логика для цветных точек (начало или конец пути)
            if (clickedPoint.HasColor)
            {
                IBrush? pointBrush = clickedPoint.Color;

                // Сценарий 1: Начинаем новый путь или кликнули на цветную точку другого цвета
                if (gameState.LastSelectedPoint == null || !AreBrushesEqual(gameState.CurrentPathColor, pointBrush))
                {
                    // Проверяем, не занята ли точка, с которой начинаем, линией другого пути
                    Line? occupyingLine = GetOccupyingLine(currentLevel, clickedPoint);
                    if (occupyingLine != null)
                    {
                        // Точка занята. Стираем путь, только если цвета разные
                        if (!AreBrushesEqual(occupyingLine.Color, pointBrush))
                        {
                            currentLevel.ClearPath(occupyingLine.PathId);
                        }
                        // Если цвета совпадают, перерисовываем путь заново
                        else if (AreBrushesEqual(occupyingLine.Color, pointBrush)) 
                        {
                            currentLevel.ClearPath(occupyingLine.PathId); // Очищаем старый путь перед началом нового
                        }
                    }

                    gameState.StartNewPath(clickedPoint);
                    return true;
                }

                // Сценарий 2: Завершаем путь, кликнув на вторую точку того же цвета
                if (AreBrushesEqual(gameState.CurrentPathColor, pointBrush) && gameState.LastSelectedPoint != clickedPoint)
                {
                    if (CanConnectPoints(currentLevel, gameState.LastSelectedPoint, clickedPoint))
                    {
                        // Проверяем, не лежит ли конечная точка на пути другого цвета
                        Line? occupyingLine = GetOccupyingLine(currentLevel, clickedPoint);
                        if (occupyingLine != null && !AreBrushesEqual(occupyingLine.Color, gameState.CurrentPathColor))
                        {
                            currentLevel.ClearPath(occupyingLine.PathId);
                        }

                        // Проверяем наличие цвета и ID текущего пути
                        if (gameState.CurrentPathColor == null || gameState.CurrentPathId == null)
                        {
                            return false;
                        }

                        // Создаем последнюю линию пути
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
                    // Проверка 1: Точка уже является частью текущего рисуемого пути?
                    var (isInPath, _) = gameState.CheckPointInPath(clickedPoint);
                    if (isInPath)
                    {
                        return false;
                    }
                    
                    // Проверка 1.5: Проверяем, не является ли точка начальной точкой текущего пути
                    if (gameState.CurrentPath.Count > 0)
                    {
                        var startPoint = gameState.CurrentPath[0];
                        if (startPoint.Row == clickedPoint.Row && startPoint.Column == clickedPoint.Column)
                        {
                            return false;
                        }
                    }
                    
                    // Проверка 2: Не пересекает ли линия саму себя (линию того же цвета)
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

                    // Проверка 3: Занята ли целевая точка существующей линией?
                    Line? occupyingLine = GetOccupyingLine(currentLevel, clickedPoint);

                    if (occupyingLine != null)
                    {
                        // Сравниваем цвет занимающей линии с цветом текущего пути
                        if (AreBrushesEqual(occupyingLine.Color, gameState.CurrentPathColor))
                        {
                            // Цвета совпадают - недопустимый ход
                            return false;
                        }
                        else
                        {
                            // Цвета разные - стираем путь другого цвета
                            currentLevel.ClearPath(occupyingLine.PathId);
                        }
                    }

                    // Добавляем новый сегмент линии

                    // Проверяем наличие ID текущего пути
                    if (gameState.CurrentPathId == null) {
                        return false;
                    }

                    // Создаем новую линию
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

        /// <summary>
        /// Проверяет, можно ли соединить две точки (являются ли они соседними).
        /// </summary>
        /// <param name="level">Текущий уровень.</param>
        /// <param name="startPoint">Начальная точка.</param>
        /// <param name="endPoint">Конечная точка.</param>
        /// <returns>True, если точки соседние; иначе false.</returns>
        public static bool CanConnectPoints(Level level, ModelPoint? startPoint, ModelPoint endPoint)
        {
            if (startPoint == null) return false;
            int rowDiff = Math.Abs(endPoint.Row - startPoint.Row);
            int colDiff = Math.Abs(endPoint.Column - startPoint.Column);
            bool areAdjacent = (rowDiff == 0 && colDiff == 1) || (rowDiff == 1 && colDiff == 0);
            return areAdjacent;
        }

        /// <summary>
        /// Проверяет, пересекает ли точка существующий путь.
        /// </summary>
        /// <param name="level">Текущий уровень.</param>
        /// <param name="point">Точка для проверки.</param>
        /// <returns>ID пути, который пересекает точка, или null.</returns>
        public static string? CheckForCrossingPath(Level level, ModelPoint point)
        {
            Line? occupyingLine = GetOccupyingLine(level, point);
            return occupyingLine?.PathId;
        }

        /// <summary>
        /// Находит точку на указанной позиции.
        /// </summary>
        /// <param name="level">Текущий уровень.</param>
        /// <param name="x">Координата X.</param>
        /// <param name="y">Координата Y.</param>
        /// <param name="cellSize">Размер ячейки.</param>
        /// <returns>Найденная точка или null.</returns>
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
                {
                    return point;
                }
            }
            return null;
        }

        /// <summary>
        /// Вычисляет расстояние между двумя точками по манхэттенской метрике.
        /// </summary>
        /// <param name="p1">Первая точка.</param>
        /// <param name="p2">Вторая точка.</param>
        /// <returns>Расстояние между точками.</returns>
        public static double CalculateDistance(ModelPoint p1, ModelPoint p2)
        {
            return Math.Abs(p1.Row - p2.Row) + Math.Abs(p1.Column - p2.Column);
        }

        /// <summary>
        /// Находит ближайшую точку к указанным координатам.
        /// </summary>
        /// <param name="level">Текущий уровень.</param>
        /// <param name="x">Координата X.</param>
        /// <param name="y">Координата Y.</param>
        /// <param name="threshold">Максимальное расстояние для поиска.</param>
        /// <returns>Ближайшая точка или null.</returns>
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

        /// <summary>
        /// Проверяет, возможен ли путь между двумя точками.
        /// </summary>
        /// <param name="level">Текущий уровень.</param>
        /// <param name="start">Начальная точка.</param>
        /// <param name="end">Конечная точка.</param>
        /// <returns>True, если путь возможен; иначе false.</returns>
        private static bool IsPathPossible(Level level, ModelPoint start, ModelPoint end)
        {
            bool[,] visited = new bool[level.Rows, level.Columns];
            var queue = new Queue<Tuple<ModelPoint, List<ModelPoint>>>();
            var startPath = new List<ModelPoint> { start };
            queue.Enqueue(Tuple.Create(start, startPath));
            visited[start.Row, start.Column] = true;
            
            // Направления движения: вверх, вправо, вниз, влево
            int[] dx = { -1, 0, 1, 0 };
            int[] dy = { 0, 1, 0, -1 };

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var currentPoint = current.Item1;
                var currentPath = current.Item2;

                // Если достигли конечной точки, путь найден
                if (currentPoint.Row == end.Row && currentPoint.Column == end.Column) 
                    return true;

                // Проверяем все четыре направления
                for (int dir = 0; dir < 4; dir++)
                {
                    int newRow = currentPoint.Row + dx[dir];
                    int newCol = currentPoint.Column + dy[dir];

                    // Проверяем, что новая позиция в пределах поля
                    if (newRow >= 0 && newRow < level.Rows && newCol >= 0 && newCol < level.Columns)
                    {
                        if (!visited[newRow, newCol])
                        {
                            var nextPoint = level.GetPointByPosition(newRow, newCol);
                            if (nextPoint != null)
                            {
                                // Можно идти на пустую точку или на конечную точку
                                if (!nextPoint.HasColor || (nextPoint.Row == end.Row && nextPoint.Column == end.Column))
                                {
                                    // Проверяем, не занята ли точка линией
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
