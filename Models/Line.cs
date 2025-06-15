using System;
using Avalonia.Media;

namespace ConnectDotsGame.Models
{
    // Представляет линию между двумя точками с цветом и идентификатором пути
    public class Line
    {
        public Point StartPoint { get; }
        public Point EndPoint { get; }
        public IBrush Color { get; }
        public string PathId { get; }

        public Line(Point startPoint, Point endPoint, IBrush? color = null, string? pathId = null)
        {
            StartPoint = startPoint ?? throw new ArgumentNullException(nameof(startPoint));
            EndPoint = endPoint ?? throw new ArgumentNullException(nameof(endPoint));
            Color = color ?? Brushes.Black;
            PathId = pathId ?? string.Empty;
        }
    }
}