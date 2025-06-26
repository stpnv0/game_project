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
            var scale = 0.8; // уменьшение на 20%
            var scaledRows = (int)Math.Round(Rows * scale);
            var scaledColumns = (int)Math.Round(Columns * scale);
            var level = new Level
            {
                Id = Id,
                Name = Name,
                Rows = scaledRows,
                Columns = scaledColumns,
                Points = new List<Point>(),
                Lines = new List<Line>()
            };
            
            // Точки на сетке
            int pointId = 1;
            for (int row = 0; row < scaledRows; row++)
            {
                for (int col = 0; col < scaledColumns; col++)
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
                // уменьшаем координаты точки
                int scaledRow = (int)Math.Round(colorPoint.Row * scale);
                int scaledCol = (int)Math.Round(colorPoint.Column * scale);
                var point = level.Points.Find(p => p.Row == scaledRow && p.Column == scaledCol)
                    ?? throw new System.InvalidOperationException($"Точка с координатами {scaledRow},{scaledCol} не найдена");
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
