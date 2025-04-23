using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using ConnectDotsGame.Models;
using ModelPoint = ConnectDotsGame.Models.Point;

namespace ConnectDotsGame.Utils
{
    public static class GameLogic
    {
        public static bool TryConnectPoints(GameState gameState, ModelPoint clickedPoint)
        {
            if (gameState.CurrentLevel == null)
                return false;
                
            // Если это точка с цветом
            if (clickedPoint.HasColor)
            {
                // Если это первая выбранная точка или начинаем новый путь с точки другого цвета
                if (gameState.LastSelectedPoint == null || gameState.CurrentPathColor != clickedPoint.Color)
                {
                    // Начинаем новый путь
                    gameState.StartNewPath(clickedPoint);
                    return true;
                }
                
                // Если мы замкнули путь того же цвета (завершили путь)
                if (gameState.CurrentPathColor == clickedPoint.Color && gameState.LastSelectedPoint != clickedPoint)
                {
                    // Проверяем, можно ли соединить последнюю выбранную точку с текущей
                    if (CanConnectPoints(gameState.CurrentLevel, gameState.LastSelectedPoint, clickedPoint))
                    {
                        // Создаем линию между последней точкой и текущей
                        var line = new Line(gameState.LastSelectedPoint, clickedPoint, clickedPoint.Color);
                        line.IsVisible = true;
                        
                        // Добавляем линию в уровень
                        gameState.CurrentLevel.Lines.Add(line);
                        gameState.CurrentLevel.AddLineToPaths(line.PathId);
                        
                        // Отмечаем точку как соединенную
                        clickedPoint.IsConnected = true;
                        
                        // Добавляем точку в текущий путь
                        gameState.CurrentPath.Add(clickedPoint);
                        
                        // Обновляем завершенность уровня
                        gameState.CheckCompletedPaths();
                        
                        // Сбрасываем состояние пути, так как мы его завершили
                        gameState.ResetPathState();
                        
                        return true;
                    }
                }
                
                return false;
            }
            
            // Если это пустая точка (без цвета) и мы уже начали путь
            if (!clickedPoint.HasColor && gameState.LastSelectedPoint != null && gameState.CurrentPathColor != null)
            {
                // Проверяем, можно ли соединить последнюю выбранную точку с текущей
                if (CanConnectPoints(gameState.CurrentLevel, gameState.LastSelectedPoint, clickedPoint))
                {
                    // Создаем линию между последней точкой и текущей
                    var line = new Line(gameState.LastSelectedPoint, clickedPoint, gameState.CurrentPathColor);
                    line.IsVisible = true;
                    
                    // Проверяем, пересекаем ли мы существующий путь
                    var crossingPath = CheckForCrossingPath(gameState.CurrentLevel, clickedPoint);
                    if (crossingPath != null)
                    {
                        // Удаляем пересеченный путь
                        gameState.CurrentLevel.ClearPath(crossingPath);
                    }
                    
                    // Добавляем линию в уровень
                    gameState.CurrentLevel.Lines.Add(line);
                    gameState.CurrentLevel.AddLineToPaths(line.PathId);
                    
                    // Обновляем последнюю выбранную точку
                    gameState.LastSelectedPoint = clickedPoint;
                    
                    // Добавляем точку в текущий путь
                    gameState.CurrentPath.Add(clickedPoint);
                    
                    return true;
                }
            }
            
            return false;
        }
        
        // Проверка возможности соединения двух точек
        public static bool CanConnectPoints(Level level, ModelPoint startPoint, ModelPoint endPoint)
        {
            // Если это точки одного цвета
            if (startPoint.HasColor && endPoint.HasColor && startPoint.Color == endPoint.Color)
            {
                // Проверяем, находятся ли точки на соседних ячейках
                int rowDiff = Math.Abs(endPoint.Row - startPoint.Row);
                int colDiff = Math.Abs(endPoint.Column - startPoint.Column);
                
                // Если точки находятся рядом (по вертикали или горизонтали), их можно соединить напрямую
                if ((rowDiff == 0 && colDiff == 1) || (rowDiff == 1 && colDiff == 0))
                {
                    return true;
                }
                
                // Если точки находятся на расстоянии, проверяем возможность пути между ними
                return IsPathPossible(level, startPoint, endPoint);
            }
            
            // Если одна из точек не цветная, проверяем только смежность
            if (!endPoint.HasColor && startPoint.HasColor)
            {
                int rowDiff = Math.Abs(endPoint.Row - startPoint.Row);
                int colDiff = Math.Abs(endPoint.Column - startPoint.Column);
                
                // Точки должны быть соседними только по вертикали или горизонтали (не по диагонали)
                return (rowDiff == 0 && colDiff == 1) || (rowDiff == 1 && colDiff == 0);
            }
            
            return false;
        }
        
        // Проверка возможности построения пути между двумя точками
        private static bool IsPathPossible(Level level, ModelPoint start, ModelPoint end)
        {
            // Массив для хранения посещенных ячеек
            bool[,] visited = new bool[level.Rows, level.Columns];
            
            // Очередь для BFS поиска
            var queue = new Queue<Tuple<ModelPoint, List<ModelPoint>>>();
            
            // Начинаем с начальной точки и пустого пути
            var startPath = new List<ModelPoint> { start };
            queue.Enqueue(Tuple.Create(start, startPath));
            visited[start.Row, start.Column] = true;
            
            // Возможные направления движения (вверх, вправо, вниз, влево)
            int[] dx = { -1, 0, 1, 0 };
            int[] dy = { 0, 1, 0, -1 };
            
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var currentPoint = current.Item1;
                var currentPath = current.Item2;
                
                // Если дошли до конечной точки, путь найден
                if (currentPoint.Row == end.Row && currentPoint.Column == end.Column)
                {
                    return true;
                }
                
                // Проверяем все направления
                for (int dir = 0; dir < 4; dir++)
                {
                    int newRow = currentPoint.Row + dx[dir];
                    int newCol = currentPoint.Column + dy[dir];
                    
                    // Проверяем, что новая позиция находится в пределах сетки
                    if (newRow >= 0 && newRow < level.Rows && newCol >= 0 && newCol < level.Columns)
                    {
                        // Проверяем, что не посещали эту ячейку
                        if (!visited[newRow, newCol])
                        {
                            var nextPoint = level.GetPointByPosition(newRow, newCol);
                            
                            // Проверяем, что точка существует
                            if (nextPoint != null)
                            {
                                // Если точка пустая или это конечная точка
                                if (!nextPoint.HasColor || (nextPoint.Row == end.Row && nextPoint.Column == end.Column))
                                {
                                    // Проверяем, не пересекаем ли мы другой путь
                                    bool pathClear = true;
                                    
                                    // Проверяем каждый существующий путь
                                    foreach (var path in level.Paths)
                                    {
                                        // Если это не путь того же цвета
                                        if (!path.Key.StartsWith(start.Color.ToString()))
                                        {
                                            // Проверяем все линии в пути
                                            foreach (var line in path.Value)
                                            {
                                                // Проверяем, проходит ли линия через проверяемую точку
                                                if ((line.StartPoint.Row == nextPoint.Row && line.StartPoint.Column == nextPoint.Column) ||
                                                    (line.EndPoint.Row == nextPoint.Row && line.EndPoint.Column == nextPoint.Column))
                                                {
                                                    pathClear = false;
                                                    break;
                                                }
                                            }
                                        }
                                        
                                        if (!pathClear)
                                            break;
                                    }
                                    
                                    if (pathClear)
                                    {
                                        // Создаем новый путь с добавленной точкой
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
            
            // Если путь не найден
            return false;
        }
        
        // Проверка, пересекает ли точка какой-либо путь
        public static string? CheckForCrossingPath(Level level, ModelPoint point)
        {
            if (level == null)
                return null;
                
            // Проверяем все пути
            foreach (var pathEntry in level.Paths)
            {
                string pathId = pathEntry.Key;
                
                // Проверяем все линии пути
                foreach (var line in pathEntry.Value)
                {
                    // Проверка, является ли точка конечной точкой линии
                    if ((line.StartPoint.Row == point.Row && line.StartPoint.Column == point.Column) ||
                        (line.EndPoint.Row == point.Row && line.EndPoint.Column == point.Column))
                    {
                        return pathId;
                    }
                }
            }
            
            return null;
        }
        
        // Поиск точки в указанной позиции
        public static ModelPoint? FindPointAtPosition(Level level, double x, double y, double cellSize)
        {
            if (level == null)
                return null;
                
            // Увеличиваем радиус поиска для более удобного клика
            double clickRadius = cellSize * 0.7; // 70% от размера ячейки (было 0.5)
            
            foreach (var point in level.Points)
            {
                // Вычисляем центр точки
                double pointX = point.Column * cellSize + cellSize / 2;
                double pointY = point.Row * cellSize + cellSize / 2;
                
                // Вычисляем расстояние до точки
                double distance = Math.Sqrt(Math.Pow(x - pointX, 2) + Math.Pow(y - pointY, 2));
                
                // Если расстояние меньше или равно радиусу, считаем, что кликнули на эту точку
                if (distance <= clickRadius)
                {
                    Console.WriteLine($"Found point at position {point.Row},{point.Column}");
                    return point;
                }
            }
            
            // Если по координатам не найдена точка, возвращаем null
            return null;
        }
        
        // Расчет расстояния между двумя точками
        public static double CalculateDistance(ModelPoint p1, ModelPoint p2)
        {
            return Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
        }
        
        // Поиск ближайшей точки
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
    }
} 