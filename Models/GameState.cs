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
        private readonly IPathManager _pathManager;

        public List<Level> Levels { get; set; }
        public int CurrentLevelIndex { get; set; }
        public Point? LastSelectedPoint { get; set; }
        
        public GameState(IGameStorageService gameStorage, IPathManager pathManager)
        {
            _gameStorage = gameStorage ?? throw new ArgumentNullException(nameof(gameStorage));
            _pathManager = pathManager ?? throw new ArgumentNullException(nameof(pathManager));
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
            
            ResetCurrentLevel();
            CurrentLevelIndex++;
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
            
            _pathManager.CancelPath(this);
            LastSelectedPoint = null;
            CurrentLevel.WasEverCompleted = wasEverCompleted;
        }
        
        public void CheckCompletedPaths()
        {
            if (CurrentLevel == null) return;
            
            bool isNowCompleted = CurrentLevel.CheckCompletion();
            
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

        // Публичные свойства для доступа к состоянию пути через PathManager
        public string? CurrentPathId => _pathManager.CurrentPathId;
        public IBrush? CurrentPathColor => _pathManager.CurrentPathColor;
        public List<Point> CurrentPath => _pathManager.CurrentPath;

        public void GoToPreviousLevel()
        {
            if (CurrentLevelIndex > 0)
            {
                ResetCurrentLevel();
                CurrentLevelIndex--;
            }
        }
    }
}