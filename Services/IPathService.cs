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

        bool TryConnectPoints(GameState gameState, Point clickedPoint);
        void StartPath(GameState gameState, Point point);
        void ContinuePath(GameState gameState, Point point);
        void EndPath(GameState gameState, Point? endPoint = null);
        void CancelPath(GameState gameState);
        void RedrawCurrentPath(GameState gameState, string colorKey, List<Point> pathPoints);
        void CreatePathLines(GameState gameState, string colorKey, List<Point> pathPoints);
        string? FindCrossingPath(Level level, Point point);
        
        // Новые методы для управления путями
        bool IsPathComplete(Level level, IBrush? color);
        bool CheckCompletion(Level level);
        void AddLineToPaths(Level level, Line line);
        void ClearPath(Level level, string pathId);
        Point? GetPointByPosition(Level level, int row, int column);
    }
} 