using System.Collections.Generic;

namespace ConnectDotsGame.Models
{
    public class PathModel
    {
        private readonly Dictionary<string, List<Point>> _currentPaths = new();

        public IReadOnlyDictionary<string, List<Point>> CurrentPaths => _currentPaths;

        public void StartPath(string colorKey, Point point)
        {
            if (_currentPaths.ContainsKey(colorKey))
                _currentPaths[colorKey].Clear();
            else
                _currentPaths[colorKey] = new List<Point>();
            _currentPaths[colorKey].Add(point);
        }

        public void AddPoint(string colorKey, Point point)
        {
            if (_currentPaths.ContainsKey(colorKey))
                _currentPaths[colorKey].Add(point);
        }

        public List<Point> GetPath(string colorKey)
        {
            return _currentPaths.TryGetValue(colorKey, out var path) ? path : new List<Point>();
        }

        public void ResetPath(string colorKey)
        {
            if (_currentPaths.ContainsKey(colorKey))
                _currentPaths.Remove(colorKey);
        }

        public void ResetAll()
        {
            _currentPaths.Clear();
        }
    }
}