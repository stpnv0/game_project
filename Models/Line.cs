using System;
using Avalonia;
using Avalonia.Media;

namespace ConnectDotsGame.Models
{
    public class Line
    {
        public Point StartPoint { get; set; } = null!;
        public Point EndPoint { get; set; } = null!;
        public IBrush Color { get; set; } = Brushes.Black;
        public string PathId { get; set; } = string.Empty;

        public Line(Point startPoint, Point endPoint, IBrush? color, string? pathId)
        {
            StartPoint = startPoint ?? throw new ArgumentNullException(nameof(startPoint));
            EndPoint = endPoint ?? throw new ArgumentNullException(nameof(endPoint));
            Color = color ?? Brushes.Black;
            PathId = pathId ?? string.Empty;
        }

        public Line() { }

        public Line Clone()
        {
            return new Line
            {
                StartPoint = this.StartPoint,
                EndPoint = this.EndPoint,
                Color = this.Color,
                PathId = this.PathId
            };
        }
    }
}