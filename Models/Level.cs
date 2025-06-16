using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;

namespace ConnectDotsGame.Models
{
    // Представляет уровень игры с сеткой точек, линиями и путями между ними
    public class Level
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Rows { get; set; }
        public int Columns { get; set; }
        public List<Point> Points { get; set; }
        public List<Line> Lines { get; set; }
        public Dictionary<string, List<Line>> Paths { get; set; } = new Dictionary<string, List<Line>>();
        public bool WasEverCompleted { get; set; }

        public Level()
        {
            Points = new List<Point>();
            Lines = new List<Line>();
            WasEverCompleted = false;
        }
    }
}