using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using ConnectDotsGame.Models;
using ConnectDotsGame.Services;

namespace ConnectDotsGame.Models
{
    public class GameState
    {
        private readonly IGameStorageService _gameStorage;
        internal readonly PathState _pathState;

        public List<Level> Levels { get; set; }
        public int CurrentLevelIndex { get; set; }

        public GameState(IGameStorageService gameStorage)
        {
            _gameStorage = gameStorage ?? throw new ArgumentNullException(nameof(gameStorage));
            _pathState = new PathState();
            Levels = new List<Level>();
            CurrentLevelIndex = 0;
        }

        public void LoadProgress() => _gameStorage.LoadProgress(Levels);
        public void SaveProgress() => _gameStorage.SaveProgress(Levels);

        public Level? CurrentLevel => 
            CurrentLevelIndex >= 0 && CurrentLevelIndex < Levels.Count 
                ? Levels[CurrentLevelIndex] 
                : null;

        public bool HasNextLevel => CurrentLevelIndex < Levels.Count - 1;

        public bool GoToNextLevel()
        {
            if (!HasNextLevel) return false;
            
            CurrentLevelIndex++;
            ResetPathState();
            return true;
        }

        public void ResetCurrentLevel()
        {
            if (CurrentLevel == null) return;

            bool wasEverCompleted = CurrentLevel.WasEverCompleted;

            foreach (var point in CurrentLevel.Points)
            {
                point.IsConnected = false;
            }

            CurrentLevel.Lines.Clear();
            CurrentLevel.Paths.Clear();

            ResetPathState();
            CurrentLevel.IsCompleted = false;
            CurrentLevel.WasEverCompleted = wasEverCompleted;
        }

        public void ResetPathState()
        {
            _pathState.Reset();
            LastSelectedPoint = null;
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

        public void ResetAllPathsAndLines()
        {
            foreach (var level in Levels)
            {
                level.Lines.Clear();
                level.Paths.Clear();
                foreach (var point in level.Points)
                {
                    point.IsConnected = false;
                }
            }
        }

        // Публичные свойства для доступа к состоянию пути
        public string? CurrentPathId => _pathState.PathId;
        public Point? LastSelectedPoint { get; set; }
        public IBrush? CurrentPathColor => _pathState.PathColor;
        public List<Point> CurrentPath => _pathState.Points;
    }
}