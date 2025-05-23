using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;

namespace ConnectDotsGame.Models
{
    public class Level
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Rows { get; set; }
        public int Columns { get; set; }
        public List<Point> Points { get; set; }
        public List<Line> Lines { get; set; }
        public Dictionary<string, List<Line>> Paths { get; set; } = new Dictionary<string, List<Line>>();
        public bool IsCompleted { get; set; }
        public bool WasEverCompleted { get; set; }

        public Level()
        {
            Points = new List<Point>();
            Lines = new List<Line>();
            IsCompleted = false;
            WasEverCompleted = false;
        }

        public Level Clone()
        {
            return new Level
            {
                Id = this.Id,
                Name = this.Name,
                Rows = this.Rows,
                Columns = this.Columns,
                Points = this.Points?.Select(p => p.Clone()).ToList() ?? new List<Point>(),
                Lines = this.Lines?.Select(l => l.Clone()).ToList() ?? new List<Line>(),
                Paths = this.Paths?.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Select(l => l.Clone()).ToList()
                ) ?? new Dictionary<string, List<Line>>(),
                IsCompleted = this.IsCompleted
            };
        }

        public bool IsPathComplete(IBrush? color)
        {
            string pathId = $"{color}-path";
            if (!Paths.ContainsKey(pathId))
                return false;
                
            var colorPoints = Points.Where(p => p.Color != null && p.Color.Equals(color)).ToList();
            
            if (colorPoints.Count != 2)
                return false;
                
            return colorPoints.All(p => p.IsConnected);
        }

        public bool CheckCompletion()
        {
            var colorGroups = Points.Where(p => p.HasColor)
                                  .GroupBy(p => p.Color);
                                  
            return colorGroups.All(group => IsPathComplete(group.Key));
        }

        public Point? GetPointByPosition(int row, int column)
        {
            return Points.FirstOrDefault(p => p.Row == row && p.Column == column);
        }

        public void AddLineToPaths(Line line)
        {
            if (line == null)
                return;
                
            if (!Paths.ContainsKey(line.PathId))
            {
                Paths[line.PathId] = new List<Line>();
            }
            
            Paths[line.PathId].Add(line);
        }

        public void ClearPath(string pathId)
        {
            if (Paths.ContainsKey(pathId))
            {
                var linesToRemove = Paths[pathId].ToList();
                
                foreach (var line in linesToRemove)
                {
                    Lines.Remove(line);
                }
                
                Paths.Remove(pathId);
                
                // Сброс статуса соединения
                var color = pathId.Replace("-path", "");
                var colorPoints = Points.Where(p => p.HasColor && p.Color != null && 
                                                p.Color.ToString() == color).ToList();
                foreach (var point in colorPoints)
                {
                    point.IsConnected = false;
                }
            }
        }
    }
}