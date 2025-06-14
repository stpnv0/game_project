using ConnectDotsGame.Models;
using System.Collections.Generic;

namespace ConnectDotsGame.Services
{
    public interface IPathManager
    {
        bool TryConnectPoints(GameState gameState, Point clickedPoint);
        void StartPath(GameState gameState, Point point);
        void ContinuePath(GameState gameState, Point point);
        void EndPath(GameState gameState, Point? endPoint = null);
        void CancelPath(GameState gameState);
        void RedrawCurrentPath(GameState gameState, string colorKey, List<Point> pathPoints);
        void CreatePathLines(GameState gameState, string colorKey, List<Point> pathPoints);
        string? FindCrossingPath(Level level, Point point);
    }
} 