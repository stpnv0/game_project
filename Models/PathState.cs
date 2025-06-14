using System.Collections.Generic;
using Avalonia.Media;

namespace ConnectDotsGame.Models
{
    public class PathState
    {
        public string? PathId { get; private set; }
        public Point? LastSelectedPoint { get; private set; }
        public IBrush? PathColor { get; private set; }
        public List<Point> Points { get; } = new List<Point>();

        public void Reset()
        {
            LastSelectedPoint = null;
            PathColor = null;
            Points.Clear();
            PathId = null;
        }

        public void StartNewPath(Point point)
        {
            Reset();
            LastSelectedPoint = point;
            PathColor = point.Color;
            PathId = $"{point.Color}-path";
            point.IsConnected = true;
            Points.Add(point);
        }

        public bool RemoveLastPoint()
        {
            if (Points.Count <= 1)
                return false;

            Points.RemoveAt(Points.Count - 1);
            LastSelectedPoint = Points.Count > 0 ? Points[^1] : null;

            return true;
        }

        public bool RemovePointsAfter(int index)
        {
            if (index < 0 || index >= Points.Count - 1)
                return false;

            Points.RemoveRange(index + 1, Points.Count - index - 1);
            LastSelectedPoint = Points.Count > 0 ? Points[^1] : null;

            return true;
        }

        public (bool isInPath, int index) CheckPointInPath(Point point)
        {
            int index = Points.FindIndex(p => p.Row == point.Row && p.Column == point.Column);
            return (index != -1, index);
        }
    }
} 