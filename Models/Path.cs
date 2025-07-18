using System.Collections.Generic;
using Avalonia.Media;

namespace ConnectDotsGame.Models
{
    // Представляет путь между точками одного цвета с сохранением последней выбранной точки
    public class Path
    {
        public string? PathId { get; set; }
        public Point? LastSelectedPoint { get; set; }
        public IBrush? PathColor { get; set; }
        public List<Point> Points { get; } = new List<Point>();
    }
} 