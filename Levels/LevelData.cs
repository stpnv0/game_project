using System;
using System.Collections.Generic;
using Avalonia.Media;
using ConnectDotsGame.Models;

namespace ConnectDotsGame.Levels
{
    public class LevelData
    {
        // Список всех уровней
        public List<LevelDto> Levels { get; set; } = new();
    }

    // DTO класс для уровня
    public class LevelDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Rows { get; set; }
        public int Columns { get; set; }
        public List<ColorPointDto> ColorPoints { get; set; } = new();

        // Преобразует DTO в модель уровня
        public Level ToLevel(Dictionary<string, IBrush> colorMap)
        {
            var level = new Level
            {
                Id = Id,
                Name = Name,
                Rows = Rows,
                Columns = Columns,
                Points = new List<Point>(),
                Lines = new List<Line>()
            };
            
            // Точки на сетке
            int pointId = 1;
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    level.Points.Add(new Point(pointId++, row, col));
                }
            }
            
            // Устанавливаем цвета для точек
            foreach (var colorPoint in ColorPoints)
            {
                if (!colorMap.TryGetValue(colorPoint.Color, out var brush))
                {
                    throw new System.InvalidOperationException($"Неизвестный цвет: {colorPoint.Color}");
                }
                var point = level.Points.Find(p => p.Row == colorPoint.Row && p.Column == colorPoint.Column)
                    ?? throw new System.InvalidOperationException($"Точка с координатами {colorPoint.Row},{colorPoint.Column} не найдена");
                point.Color = brush;
            }
            
            return level;
        }
    }

    // DTO класс для цветной точки
    public class ColorPointDto
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public string Color { get; set; } = string.Empty;
    }
}
