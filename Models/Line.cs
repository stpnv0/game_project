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
        
        public Line(Point startPoint, Point endPoint, IBrush color)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            IsVisible = false;
            Color = color;
            PathId = $"{startPoint.Color}-path";
        }
        
        public Line() 
        {
            Color = Brushes.Black;
        }
        
        public Line Clone()
        {
            return new Line
            {
                    StartPoint = this.StartPoint != null ? this.StartPoint.Clone() : null!,
                    EndPoint = this.EndPoint != null ? this.EndPoint.Clone() : null!,
                    IsVisible = this.IsVisible,
                    Color = this.Color,
                    PathId = this.PathId
            };
        }
    }
} 