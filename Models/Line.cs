using Avalonia;
using Avalonia.Media;

namespace ConnectDotsGame.Models
{
    public class Line
    {
        public Point StartPoint { get; set; } = null!;
        public Point EndPoint { get; set; } = null!;
        public bool IsVisible { get; set; }
        public IBrush Color { get; set; }
        public string PathId { get; set; } = string.Empty;
        
        public Line(Point startPoint, Point endPoint, IBrush color, string pathId) // Добавляем pathId
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            IsVisible = false; // По умолчанию невидима? Или true?
            Color = color;
            PathId = pathId; // Присваиваем переданный ID
        }
                
        public Line()
        {
            Color = Brushes.Black; // Значение по умолчанию
            PathId = string.Empty; // Значение по умолчанию
        }
        
        public Line Clone()
        {
            return new Line
            {
                StartPoint = this.StartPoint?.Clone(), // Используем null-conditional оператор
                EndPoint = this.EndPoint?.Clone(),   // Используем null-conditional оператор
                IsVisible = this.IsVisible,
                Color = this.Color, // Кисти обычно не клонируют, если они неизменяемые
                PathId = this.PathId // Копируем ID
            };
        }
    }
} 