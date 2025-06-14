using ConnectDotsGame.Models;
using ConnectDotsGame.Utils;
using System.Collections.Generic;
using System.Linq;
using static ConnectDotsGame.Utils.PointLocator;

namespace ConnectDotsGame.Services
{
    public class PathManager : IPathManager
    {
        public bool TryConnectPoints(GameState gameState, Point clickedPoint)
        {
            if (gameState.CurrentPathColor == null || clickedPoint.Color == null)
                return false;

            if (!BrushExtensions.AreBrushesEqual(gameState.CurrentPathColor, clickedPoint.Color) || 
                gameState.LastSelectedPoint == clickedPoint)
                return false;

            if (gameState.LastSelectedPoint == null || !IsNeighbor(gameState.LastSelectedPoint, clickedPoint))
                return false;

            gameState.CheckCompletedPaths();
            gameState.ResetPathState();
            return true;
        }

        public void StartPath(GameState gameState, Point point)
        {
            if (gameState.CurrentLevel == null || !point.HasColor || point.Color == null)
                return;

            gameState._pathState.StartNewPath(point);
            gameState.LastSelectedPoint = point;
        }

        public void ContinuePath(GameState gameState, Point point)
        {
            var level = gameState.CurrentLevel;
            if (level == null || gameState.CurrentPathColor == null || gameState.CurrentPath.Count == 0)
                return;

            var lastPoint = gameState.CurrentPath[^1];
            if (lastPoint.Row == point.Row && lastPoint.Column == point.Column)
                return;

            if (!IsNeighbor(lastPoint, point))
                return;

            HandlePathContinuation(gameState, point);
        }

        private void HandlePathContinuation(GameState gameState, Point point)
        {
            // Проверка на возврат по пути
            var (isInPath, returnIndex) = gameState._pathState.CheckPointInPath(point);
            if (isInPath)
            {
                HandlePathReturn(gameState, returnIndex, point);
                return;
            }

            // Проверка на пересечение с другим путем
            var crossingPath = FindCrossingPath(gameState.CurrentLevel!, point);
            if (crossingPath != null)
            {
                gameState.CurrentLevel!.ClearPath(crossingPath);
            }

            if (!CanContinuePath(gameState, point))
                return;

            if (IsEndPoint(gameState, point))
            {
                CompletePath(gameState, point);
                return;
            }

            AddPointToPath(gameState, point);
        }

        private void HandlePathReturn(GameState gameState, int returnIndex, Point point)
        {
            gameState._pathState.RemovePointsAfter(returnIndex);
            gameState.LastSelectedPoint = point;
            RedrawCurrentPath(gameState, gameState.CurrentPathColor!.ToString()!, gameState.CurrentPath);
        }

        private bool CanContinuePath(GameState gameState, Point point)
        {
            if (!point.HasColor)
                return true;

            return BrushExtensions.AreBrushesEqual(point.Color, gameState.CurrentPathColor);
        }

        private bool IsEndPoint(GameState gameState, Point point)
        {
            return point.HasColor && 
                   BrushExtensions.AreBrushesEqual(point.Color, gameState.CurrentPathColor) && 
                   point != gameState.CurrentPath[0];
        }

        private void CompletePath(GameState gameState, Point point)
        {
            gameState._pathState.Points.Add(point);
            CreatePathLines(gameState, gameState.CurrentPathColor!.ToString()!, gameState.CurrentPath);
            
            foreach (var pathPoint in gameState.CurrentPath)
            {
                if (pathPoint.HasColor && BrushExtensions.AreBrushesEqual(pathPoint.Color, gameState.CurrentPathColor))
                {
                    pathPoint.IsConnected = true;
                }
            }
            
            gameState.ResetPathState();
        }

        private void AddPointToPath(GameState gameState, Point point)
        {
            gameState._pathState.Points.Add(point);
            gameState.LastSelectedPoint = point;
            RedrawCurrentPath(gameState, gameState.CurrentPathColor!.ToString()!, gameState.CurrentPath);
        }

        public void EndPath(GameState gameState, Point? endPoint = null)
        {
            if (gameState.CurrentLevel == null || gameState.CurrentPathColor == null || gameState.CurrentPath.Count == 0)
                return;

            if (endPoint == null || !IsValidEndPoint(gameState, endPoint))
            {
                CancelPath(gameState);
                return;
            }

            CompletePath(gameState, endPoint);
        }

        private bool IsValidEndPoint(GameState gameState, Point endPoint)
        {
            if (!endPoint.HasColor || !BrushExtensions.AreBrushesEqual(endPoint.Color, gameState.CurrentPathColor))
                return false;

            if (gameState.CurrentPath[0] == endPoint)
                return false;

            var lastPoint = gameState.CurrentPath[^1];
            return IsNeighbor(lastPoint, endPoint);
        }

        public void CancelPath(GameState gameState)
        {
            if (gameState.CurrentLevel == null || gameState.CurrentPathColor == null)
                return;

            gameState.CurrentLevel.ClearPath(gameState.CurrentPathId ?? "");
            gameState.ResetPathState();
        }

        public void RedrawCurrentPath(GameState gameState, string colorKey, List<Point> pathPoints)
        {
            var level = gameState.CurrentLevel;
            if (level == null || pathPoints.Count < 2)
                return;

            level.ClearPath(gameState.CurrentPathId ?? "");
            CreatePathLines(gameState, colorKey, pathPoints);
        }

        public void CreatePathLines(GameState gameState, string colorKey, List<Point> pathPoints)
        {
            var level = gameState.CurrentLevel;
            if (level == null || pathPoints.Count < 2)
                return;

            for (int i = 0; i < pathPoints.Count - 1; i++)
            {
                var startPoint = pathPoints[i];
                var endPoint = pathPoints[i + 1];
                var line = new Line(startPoint, endPoint, gameState.CurrentPathColor, gameState.CurrentPathId)
                {
                    PathId = gameState.CurrentPathId ?? ""
                };
                
                level.Lines.Add(line);
                level.AddLineToPaths(line);
            }
        }

        public string? FindCrossingPath(Level level, Point point)
        {
            return level.Paths
                .FirstOrDefault(path => path.Value.Any(line =>
                    (line.StartPoint.Row == point.Row && line.StartPoint.Column == point.Column) ||
                    (line.EndPoint.Row == point.Row && line.EndPoint.Column == point.Column)))
                .Key;
        }
    }
} 