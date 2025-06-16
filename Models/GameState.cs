using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using ConnectDotsGame.Models;
using ConnectDotsGame.Services;

namespace ConnectDotsGame.Models
{
    // Хранит текущее состояние игры, включая список уровней и текущий уровень
    public class GameState
    {
        private readonly IPathService _pathService;

        public List<Level> Levels { get; set; }
        public int CurrentLevelIndex { get; set; }
        
        public GameState(IPathService pathService)
        {
            _pathService = pathService ?? throw new ArgumentNullException(nameof(pathService));
            Levels = new List<Level>();
            CurrentLevelIndex = 0;
        }
        
        public Level? CurrentLevel => 
            CurrentLevelIndex >= 0 && CurrentLevelIndex < Levels.Count 
                ? Levels[CurrentLevelIndex] 
                : null;

        public bool HasNextLevel => CurrentLevelIndex < Levels.Count - 1;
    }
}