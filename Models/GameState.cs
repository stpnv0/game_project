using System.Collections.Generic;
using Avalonia.Media;
using ConnectDotsGame.Utils;

namespace ConnectDotsGame.Models
{
    public class GameState
    {
        private readonly GameStorage _gameStorage;

        public string? CurrentPathId { get; private set; }
        public List<Level> Levels { get; set; }
        public int CurrentLevelIndex { get; set; }
        public Point? LastSelectedPoint { get; set; }
        public IBrush? CurrentPathColor { get; set; }
        public List<Point> CurrentPath { get; set; } = new List<Point>();

        public GameState()
        {
            Levels = new List<Level>();
            CurrentLevelIndex = 0;
            LastSelectedPoint = null;
            CurrentPathColor = null;
            _gameStorage = new GameStorage();
        }

        public void LoadProgress()
        {
            _gameStorage.LoadProgress(Levels);
        }

        public void SaveProgress()
        {
            _gameStorage.SaveProgress(Levels);
        }

        public Level? CurrentLevel
        {
            get
            {
                if (CurrentLevelIndex >= 0 && CurrentLevelIndex < Levels.Count)
                    return Levels[CurrentLevelIndex];
                return null;
            }
        }

        public bool HasNextLevel => CurrentLevelIndex < Levels.Count - 1;

        public bool GoToNextLevel()
        {
            if (HasNextLevel)
            {
                CurrentLevelIndex++;
                ResetPathState();
                return true;
            }
            return false;
        }

        public void ResetCurrentLevel()
        {
            if (CurrentLevel != null)
            {
                bool wasEverCompleted = CurrentLevel.WasEverCompleted;

                foreach (var point in CurrentLevel.Points)
                {
                    point.IsConnected = false;
                }

                CurrentLevel.Lines.Clear();
                CurrentLevel.Paths.Clear();

                ResetPathState();
                CurrentLevel.IsCompleted = false;
                CurrentLevel.WasEverCompleted = wasEverCompleted; // сохранить состояние
            }
        }

        public void ResetPathState()
        {
            LastSelectedPoint = null;
            CurrentPathColor = null;
            CurrentPath.Clear();
            CurrentPathId = null;
        }

        public void StartNewPath(Point point)
        {
            ResetPathState();
            LastSelectedPoint = point;
            CurrentPathColor = point.Color;
            CurrentPathId = $"{point.Color}-path";
            point.IsConnected = true;
            CurrentPath.Add(point);
        }

        public (bool isInPath, int index) CheckPointInPath(Point point)
        {
            int index = CurrentPath.FindIndex(p => p.Row == point.Row && p.Column == point.Column);
            return (index != -1, index);
        }

        public bool RemoveLastPointFromPath()
        {
            if (CurrentPath.Count <= 1)
                return false;

            CurrentPath.RemoveAt(CurrentPath.Count - 1);
            LastSelectedPoint = CurrentPath.Count > 0 ? CurrentPath[^1] : null;

            return true;
        }

        public bool RemovePointsAfter(int index)
        {
            if (index < 0 || index >= CurrentPath.Count - 1)
                return false;

            CurrentPath.RemoveRange(index + 1, CurrentPath.Count - index - 1);
            LastSelectedPoint = CurrentPath.Count > 0 ? CurrentPath[^1] : null;

            return true;
        }

        public void CheckCompletedPaths()
        {
            if (CurrentLevel == null) return;

            bool isNowCompleted = CurrentLevel.CheckCompletion();

            CurrentLevel.IsCompleted = isNowCompleted;

            if (isNowCompleted)
            {
                CurrentLevel.WasEverCompleted = true;
                SaveProgress();
            }
        }
    }
}