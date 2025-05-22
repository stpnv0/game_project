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
        /// <summary>
/// Основной метод логики соединения точек.
/// </summary>
/// <param name="gameState">Текущее состояние игры.</param>
/// <param name="clickedPoint">Точка, на которую нажал пользователь.</param>
/// <returns>True, если соединение успешно; иначе false.</returns>
/// <summary>
/// Основной метод логики соединения точек.
/// </summary>
/// <param name="gameState">Текущее состояние игры.</param>
/// <param name="clickedPoint">Точка, на которую нажал пользователь.</param>
/// <returns>True, если соединение успешно; иначе false.</returns>
/// <summary>
/// Основной метод логики соединения точек.
/// </summary>
/// <param name="gameState">Текущее состояние игры.</param>
/// <param name="clickedPoint">Точка, на которую нажал пользователь.</param>
/// <returns>True, если соединение успешно; иначе false.</returns>
/// <summary>
/// Основной метод логики соединения точек.
/// </summary>
/// <param name="gameState">Текущее состояние игры.</param>
/// <param name="clickedPoint">Точка, на которую нажал пользователь.</param>
/// <returns>True, если соединение успешно; иначе false.</returns>
public static bool TryConnectPoints(GameState gameState, ModelPoint clickedPoint)
{
    if (gameState == null || clickedPoint == null)
    {
        throw new ArgumentNullException("GameState или clickedPoint не может быть null.");
    }

    var currentLevel = gameState.CurrentLevel;
    if (currentLevel == null)
    {
        return false; // Уровень не загружен
    }

    // Проверяем, есть ли активная точка
    if (gameState.LastSelectedPoint == null)
    {
        // Если нет активной точки, начинаем новый путь
        if (clickedPoint.HasColor)
        {
            gameState.StartNewPath(clickedPoint);
            return true;
        }
        return false; // Нельзя начать путь с пустой точки
    }

    var lastPoint = gameState.LastSelectedPoint;

    // Проверяем, не является ли кликнутая точка той же самой
    if (lastPoint == clickedPoint)
    {
        return false; // Нельзя соединить точку саму с собой
    }

    // Проверяем, находится ли точка в текущем пути
    if (gameState.IsPointInCurrentPath(clickedPoint))
    {
        // Если точка уже в пути, удаляем её и все последующие точки
        while (gameState.CurrentPath.Last() != clickedPoint)
        {
            gameState.RemoveLastPointFromPath();
        }
        return true;
    }

    // Проверяем, имеет ли точка цвет и совпадает ли он с активным цветом
    if (clickedPoint.HasColor && clickedPoint.Color != gameState.CurrentPathColor)
    {
        return false; // Нельзя соединить точки разных цветов
    }

    // Проверяем, не находится ли текущая точка на расстоянии больше одной клетки от предыдущей
    int rowDiff = Math.Abs(clickedPoint.Row - lastPoint.Row);
    int colDiff = Math.Abs(clickedPoint.Column - lastPoint.Column);
    if ((rowDiff > 1 || colDiff > 1) || (rowDiff == 1 && colDiff == 1))
    {
        Console.WriteLine($"Недопустимое соединение: {lastPoint.Row},{lastPoint.Column} -> {clickedPoint.Row},{clickedPoint.Column}. Расстояние превышает одну клетку.");
        return false; // Автодостраивание запрещено для точек на большем расстоянии
    }

    // Проверяем, можно ли соединить точки (соседние ли они)
    if (!CanConnectPoints(currentLevel, lastPoint, clickedPoint))
    {
        return false; // Точки не соседние
    }

    // Проверяем, не занята ли точка линией другого цвета
    Line? occupyingLine = GetOccupyingLine(currentLevel, clickedPoint);
    if (occupyingLine != null && !AreBrushesEqual(occupyingLine.Color, gameState.CurrentPathColor))
    {
        return false; // Точка занята линией другого цвета
    }

    // Создаем линию между последней выбранной точкой и текущей
    var newLine = new Line(lastPoint, clickedPoint, gameState.CurrentPathColor, gameState.CurrentPathId);
    currentLevel.Lines.Add(newLine);
    currentLevel.AddLineToPaths(newLine);

    // Добавляем точку в текущий путь
    gameState.CurrentPath.Add(clickedPoint);
    gameState.LastSelectedPoint = clickedPoint;

    // Если точка имеет цвет, отмечаем её как соединённую
    if (clickedPoint.HasColor)
    {
        clickedPoint.IsConnected = true;
    }

    return true;
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
            if (startPoint == null)
            {
                Console.WriteLine("CanConnectPoints: StartPoint is null. Returning false.");
                return false;
            }

            int rowDiff = Math.Abs(endPoint.Row - startPoint.Row);
            int colDiff = Math.Abs(endPoint.Column - startPoint.Column);

            Console.WriteLine($"CanConnectPoints: Checking connection from Row={startPoint.Row}, Column={startPoint.Column} to Row={endPoint.Row}, Column={endPoint.Column}.");
            Console.WriteLine($"CanConnectPoints: Calculated rowDiff={rowDiff}, colDiff={colDiff}.");

            // Блокируем диагональные движения и движения на большое расстояние
            if ((rowDiff == 1 && colDiff == 1) || rowDiff > 1 || colDiff > 1)
            {
                Console.WriteLine("CanConnectPoints: Invalid movement. Returning false.");
                return false;
            }

            
            return true;
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
