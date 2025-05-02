using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media; // Убедись, что пространство имен для Color верное
using ConnectDotsGame.Models; // Пространство имен для твоих моделей
using ModelPoint = ConnectDotsGame.Models.Point; // Используем псевдоним для ясности

namespace ConnectDotsGame.Utils
{
    public static class GameLogic
    {
        // --- ВСПОМОГАТЕЛЬНЫЙ МЕТОД СРАВНЕНИЯ КИСТЕЙ ---
        /// <summary>
        /// Сравнивает два объекта IBrush, уделяя особое внимание SolidColorBrush.
        /// </summary>
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

        // --- ВСПОМОГАТЕЛЬНЫЙ МЕТОД ПОЛУЧЕНИЯ ЦВЕТА ПУТИ ПО ID ---
        // (Остается полезным, если нужно получить цвет пути по его ID)
        private static IBrush? GetPathColorById(Level level, string pathId)
        {
            if (level != null && !string.IsNullOrEmpty(pathId) && level.Paths.TryGetValue(pathId, out var lines) && lines.Any())
            {
                return lines[0].Color;
            }
            return null;
        }

        // --- НОВЫЙ ВСПОМОГАТЕЛЬНЫЙ МЕТОД: ПОЛУЧЕНИЕ ЛИНИИ, ЗАНИМАЮЩЕЙ ТОЧКУ ---
        /// <summary>
        /// Проверяет, занята ли указанная точка сегментом линии существующего пути.
        /// </summary>
        /// <param name="level">Текущий уровень.</param>
        /// <param name="point">Точка для проверки.</param>
        /// <returns>Объект Line, занимающий точку, или null, если точка свободна.</returns>
        private static Line? GetOccupyingLine(Level level, ModelPoint point)
        {
            if (level == null) return null;

            // Итерируем по всем путям в словаре Paths
            foreach (var pathEntry in level.Paths)
            {
                // Итерируем по всем линиям (сегментам) текущего пути
                foreach (var line in pathEntry.Value)
                {
                    // Является ли проверяемая точка КОНЕЧНОЙ точкой этого сегмента?
                    if (line.EndPoint.Row == point.Row && line.EndPoint.Column == point.Column)
                    {
                        return line; // Точка занята этой линией
                    }

                    // Является ли проверяемая точка НАЧАЛЬНОЙ точкой этого сегмента?
                    // Исключаем проверку для исходных цветных точек уровня,
                    // чтобы можно было начать перерисовывать путь с его начала.
                    bool isStartAnOriginalColoredPoint = level.Points.Any(p => p.HasColor &&
                                                                           p.Row == line.StartPoint.Row &&
                                                                           p.Column == line.StartPoint.Column &&
                                                                           AreBrushesEqual(p.Color, line.Color));

                    if (!isStartAnOriginalColoredPoint && line.StartPoint.Row == point.Row && line.StartPoint.Column == point.Column)
                    {
                         return line; // Точка занята этой линией
                    }
                }
            }
            return null; // Точка не занята ни одним сегментом линии
        }


        // --- ОСНОВНОЙ МЕТОД ЛОГИКИ СОЕДИНЕНИЯ ---
        public static bool TryConnectPoints(GameState gameState, ModelPoint clickedPoint)
        {
            if (gameState.CurrentLevel == null)
                return false;

            var currentLevel = gameState.CurrentLevel;

            // --- Логика для ЦВЕТНЫХ точек (Начало или Конец пути) ---
            if (clickedPoint.HasColor)
            {
                IBrush? pointBrush = clickedPoint.Color;

                // Сценарий 1: Начинаем новый путь ИЛИ кликнули на цветную точку другого цвета
                if (gameState.LastSelectedPoint == null || !AreBrushesEqual(gameState.CurrentPathColor, pointBrush))
                {
                    // Проверяем, не занята ли точка, с которой начинаем, ЛИНИЕЙ другого пути.
                    // Используем новый метод GetOccupyingLine.
                    Line? occupyingLine = GetOccupyingLine(currentLevel, clickedPoint);
                    if (occupyingLine != null)
                    {
                        // Точка занята. Стираем путь, только если цвета РАЗНЫЕ.
                        if (!AreBrushesEqual(occupyingLine.Color, pointBrush))
                        {
                            Console.WriteLine($"Начинаем новый путь ({pointBrush}) поверх существующего пути {occupyingLine.PathId} (Цвет: {occupyingLine.Color}). Стираем его.");
                            // ВАЖНО: Убедитесь, что ClearPath в Level.cs работает корректно с уникальными ID
                            currentLevel.ClearPath(occupyingLine.PathId);
                        }
                        // Если цвета совпадают, мы просто начнем перерисовывать этот путь заново.
                        // Старый путь будет неявно удален или перезаписан при вызове StartNewPath,
                        // если ClearPath также вызывается из StartNewPath или подобного метода при старте на занятой точке.
                        // Либо нужно явно очистить существующий путь того же цвета перед StartNewPath.
                        // Рассмотрим вариант явной очистки:
                        else if (AreBrushesEqual(occupyingLine.Color, pointBrush)) {
                             Console.WriteLine($"Перерисовываем путь {occupyingLine.PathId}, начиная с точки {clickedPoint.Row},{clickedPoint.Column}.");
                             currentLevel.ClearPath(occupyingLine.PathId); // Очищаем старый путь перед началом нового
                        }
                    }

                    Console.WriteLine($"Начинаем новый путь с цветной точки {clickedPoint.Row},{clickedPoint.Column}");
                    // ВАЖНО: Убедитесь, что StartNewPath в GameState.cs генерирует и сохраняет УНИКАЛЬНЫЙ CurrentPathId
                    gameState.StartNewPath(clickedPoint);
                    return true;
                }

                // Сценарий 2: Завершаем путь, кликнув на вторую точку того же цвета
                if (AreBrushesEqual(gameState.CurrentPathColor, pointBrush) && gameState.LastSelectedPoint != clickedPoint)
                {
                    if (CanConnectPoints(currentLevel, gameState.LastSelectedPoint, clickedPoint))
                    {
                        // Проверяем, не лежит ли конечная точка на пути другого цвета.
                        Line? occupyingLine = GetOccupyingLine(currentLevel, clickedPoint);
                        if (occupyingLine != null && !AreBrushesEqual(occupyingLine.Color, gameState.CurrentPathColor))
                        {
                             Console.WriteLine($"Завершаем путь, стирая пересекаемый путь {occupyingLine.PathId} в конечной точке.");
                             currentLevel.ClearPath(occupyingLine.PathId);
                        }

                        Console.WriteLine($"Завершаем путь {gameState.CurrentPathId} в точке {clickedPoint.Row},{clickedPoint.Column}");
                        // Проверяем наличие цвета и ID текущего пути
                        if (gameState.CurrentPathColor == null || gameState.CurrentPathId == null)
                        {
                             Console.WriteLine("Ошибка: Невозможно завершить путь, отсутствует цвет или ID текущего пути.");
                             return false;
                        }

                        // Создаем последнюю линию пути, передавая УНИКАЛЬНЫЙ ID из GameState
                        // ВАЖНО: Убедитесь, что конструктор Line в Line.cs принимает pathId
                        var finalLine = new Line(gameState.LastSelectedPoint, clickedPoint, gameState.CurrentPathColor, gameState.CurrentPathId);
                        finalLine.IsVisible = true;
                        currentLevel.Lines.Add(finalLine);
                        // ВАЖНО: Убедитесь, что AddLineToPaths в Level.cs использует line.PathId для добавления в словарь Paths
                        currentLevel.AddLineToPaths(finalLine);

                        clickedPoint.IsConnected = true;
                        gameState.CurrentPath.Add(clickedPoint);
                        gameState.CheckCompletedPaths();
                        gameState.ResetPathState(); // Сбрасывает CurrentPathId
                        return true;
                    }
                     else {
                         Console.WriteLine($"Невозможно завершить путь: точки не соседние.");
                         return false;
                     }
                }
                return false;
            }

            // --- Логика для ПУСТЫХ точек (Продолжение пути) ---
            if (!clickedPoint.HasColor && gameState.LastSelectedPoint != null && gameState.CurrentPathColor != null)
            {
                if (CanConnectPoints(currentLevel, gameState.LastSelectedPoint, clickedPoint))
                {
                    // Проверка 1: Точка уже является частью ТЕКУЩЕГО рисуемого пути? (Предотвращает разворот)
                    if (gameState.IsPointInCurrentPath(clickedPoint))
                    {
                        Console.WriteLine($"Недопустимый ход: Точка {clickedPoint.Row},{clickedPoint.Column} уже есть в текущем рисуемом пути.");
                        return false;
                    }
                    
                    // Проверка 1.5: Проверяем, не пересекает ли линия саму себя (линию того же цвета)
                    foreach (var pathEntry in currentLevel.Paths)
                    {
                        if (gameState.CurrentPathColor != null && pathEntry.Key.StartsWith(gameState.CurrentPathColor.ToString() ?? ""))
                        {
                            foreach (var line in pathEntry.Value)
                            {
                                if ((line.StartPoint.Row == clickedPoint.Row && line.StartPoint.Column == clickedPoint.Column) ||
                                    (line.EndPoint.Row == clickedPoint.Row && line.EndPoint.Column == clickedPoint.Column))
                                {
                                    Console.WriteLine($"Недопустимый ход: Точка {clickedPoint.Row},{clickedPoint.Column} уже занята линией того же цвета.");
                                    return false;
                                }
                            }
                        }
                    }

                    // Проверка 2: Занята ли целевая точка существующей ЛИНИЕЙ?
                    Line? occupyingLine = GetOccupyingLine(currentLevel, clickedPoint);

                    if (occupyingLine != null)
                    {
                        // Точка занята. Сравниваем цвет занимающей линии с цветом текущего пути.
                        if (AreBrushesEqual(occupyingLine.Color, gameState.CurrentPathColor))
                        {
                            // Цвета СОВПАДАЮТ -> Недопустимый ход! Нельзя рисовать поверх пути того же цвета.
                            Console.WriteLine($"Недопустимый ход: Точка {clickedPoint.Row},{clickedPoint.Column} уже занята сегментом пути того же цвета (PathId: {occupyingLine.PathId}).");
                            return false;
                        }
                        else
                        {
                            // Цвета РАЗНЫЕ -> Стираем ВЕСЬ путь другого цвета.
                            Console.WriteLine($"Пересечение пути {occupyingLine.PathId} (Цвет: {occupyingLine.Color}) в точке {clickedPoint.Row},{clickedPoint.Column}. Стираем его.");
                            currentLevel.ClearPath(occupyingLine.PathId);
                            // После стирания продолжаем и добавляем новый сегмент.
                        }
                    }

                    // Если мы дошли сюда, точка либо свободна, либо была очищена.
                    // Добавляем новый сегмент линии.
                    Console.WriteLine($"Добавляем линию к точке {clickedPoint.Row},{clickedPoint.Column}");

                    // Проверяем наличие ID текущего пути
                    if (gameState.CurrentPathId == null) {
                         Console.WriteLine("Ошибка: Невозможно добавить линию, отсутствует ID текущего пути в GameState.");
                         return false;
                    }

                    // Создаем новую линию, передавая УНИКАЛЬНЫЙ ID из GameState
                    // ВАЖНО: Убедитесь, что конструктор Line в Line.cs принимает pathId
                    var newLine = new Line(gameState.LastSelectedPoint, clickedPoint, gameState.CurrentPathColor, gameState.CurrentPathId);
                    newLine.IsVisible = true;
                    currentLevel.Lines.Add(newLine);
                    // ВАЖНО: Убедитесь, что AddLineToPaths в Level.cs использует line.PathId
                    currentLevel.AddLineToPaths(newLine);

                    gameState.LastSelectedPoint = clickedPoint;
                    gameState.CurrentPath.Add(clickedPoint);
                    return true;
                }
                else {
                    Console.WriteLine($"Недопустимый ход: Точки не являются соседними.");
                    return false;
                }
            }
            return false;
        }


        // --- Проверка, можно ли соединить точки (соседние ли они) ---
        public static bool CanConnectPoints(Level level, ModelPoint? startPoint, ModelPoint endPoint)
        {
            if (startPoint == null) return false;
            int rowDiff = Math.Abs(endPoint.Row - startPoint.Row);
            int colDiff = Math.Abs(endPoint.Column - startPoint.Column);
            bool areAdjacent = (rowDiff == 0 && colDiff == 1) || (rowDiff == 1 && colDiff == 0);
            return areAdjacent;
        }


        // --- МЕТОД ПРОВЕРКИ ПЕРЕСЕЧЕНИЯ CheckForCrossingPath ---
        // (Теперь менее важен, т.к. GetOccupyingLine делает более детальную проверку,
        // но может остаться для других целей или для совместимости, если где-то используется)
        public static string? CheckForCrossingPath(Level level, ModelPoint point)
        {
            Line? occupyingLine = GetOccupyingLine(level, point);
            return occupyingLine?.PathId; // Возвращаем ID занимающей линии или null
        }


        // --- Остальные вспомогательные методы ---
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
                    // Console.WriteLine($"Найдена точка в позиции {point.Row},{point.Column}");
                    return point;
                }
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

                if (currentPoint.Row == end.Row && currentPoint.Column == end.Column) return true;

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
                                // Можно идти на пустую точку ИЛИ на конечную точку
                                if (!nextPoint.HasColor || (nextPoint.Row == end.Row && nextPoint.Column == end.Column))
                                {
                                    // Проверяем, не занята ли точка ЛИНИЕЙ
                                    if (GetOccupyingLine(level, nextPoint) == null || (nextPoint.Row == end.Row && nextPoint.Column == end.Column))
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
