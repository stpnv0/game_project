using ConnectDotsGame.Models;
using System.Collections.Generic;
using Avalonia.Media;

namespace ConnectDotsGame.Services
{
    public interface IPathService
    {
        string? CurrentPathId { get; }
        IBrush? CurrentPathColor { get; }
        List<Point> CurrentPath { get; }
        Point? LastSelectedPoint { get; }

        void StartPath(GameState gameState, Point point);
        void ContinuePath(GameState gameState, Point point);
        void EndPath(GameState gameState, Point? endPoint = null);
        void CancelPath(GameState gameState);
        bool CheckCompletion(Level level);
        Point? GetPointByPosition(Level level, int row, int column);
    }
} 