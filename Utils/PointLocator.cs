using ConnectDotsGame.Models;
using ConnectDotsGame.Utils;
using System;
using System.Linq;

namespace ConnectDotsGame.Utils
{
    public static class PointLocator
    {
        // Находит точку по координатам x, y на игровом поле.
        public static Point? FindPointAtPosition(Level level, double x, double y, double cellSize)
        {
            double clickRadius = cellSize * GameConstants.PointClickRadiusFactor;
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

        // Проверяет, являются ли две точки соседями по сетке.
        public static bool IsNeighbor(Point a, Point b)
        {
            return (Math.Abs(a.Row - b.Row) == 1 && a.Column == b.Column) ||
                   (Math.Abs(a.Column - b.Column) == 1 && a.Row == b.Row);
        }

    }
} 