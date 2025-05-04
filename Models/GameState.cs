using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Avalonia.Media;
using ConnectDotsGame.Utils;
using ModelPoint = ConnectDotsGame.Models.Point;

namespace ConnectDotsGame.Models
{
    public class GameState
    {
        private readonly GameStorage _gameStorage;

        public string? CurrentPathId { get; private set; }
        public List<Level> Levels { get; set; }
        public int CurrentLevelIndex { get; set; }
        public ModelPoint? LastSelectedPoint { get; set; }
        public IBrush? CurrentPathColor { get; set; }
        public List<ModelPoint> CurrentPath { get; set; } = new List<ModelPoint>();

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

        private class GameStorage
        {
            private readonly string _savePath;

            public GameStorage()
            {
                string appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "ConnectDotsGame"
                );

                if (!Directory.Exists(appDataPath))
                {
                    Directory.CreateDirectory(appDataPath);
                }

                _savePath = Path.Combine(appDataPath, "progress.json");
            }

            public void SaveProgress(List<Level> levels)
            {
                try
                {
                    var progressData = levels.Select(level => new LevelProgressData
                    {
                        Id = level.Id,
                        IsCompleted = level.IsCompleted
                    }).ToList();

                    string json = JsonSerializer.Serialize(progressData);
                    File.WriteAllText(_savePath, json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saving progress: {ex.Message}");
                }
            }

            public void LoadProgress(List<Level> levels)
            {
                try
                {
                    if (!File.Exists(_savePath)) return;

                    string json = File.ReadAllText(_savePath);
                    var progressData = JsonSerializer.Deserialize<List<LevelProgressData>>(json) ?? new List<LevelProgressData>();

                    foreach (var levelProgress in progressData)
                    {
                        var level = levels.FirstOrDefault(l => l.Id == levelProgress.Id);
                        if (level != null)
                        {
                            level.IsCompleted = levelProgress.IsCompleted;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading progress: {ex.Message}");
                }
            }

            private class LevelProgressData
            {
                public int Id { get; set; }
                public bool IsCompleted { get; set; }
            }
        }

        public Level? CurrentLevel => CurrentLevelIndex >= 0 && CurrentLevelIndex < Levels.Count
            ? Levels[CurrentLevelIndex]
            : null;

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
            if (CurrentLevel == null) return;

            foreach (var point in CurrentLevel.Points)
            {
                point.IsConnected = false;
            }

            CurrentLevel.Lines.Clear();
            CurrentLevel.Paths.Clear();
            ResetPathState();
        }

        public void ResetPathState()
        {
            LastSelectedPoint = null;
            CurrentPathColor = null;
            CurrentPath.Clear();
            CurrentPathId = null;
        }

        public void CheckCompletedPaths()
        {
            if (CurrentLevel == null) return;

            bool allCompleted = CurrentLevel.Points
                .Where(p => p.HasColor)
                .GroupBy(p => p.Color.ToString())
                .All(group => CurrentLevel.IsPathComplete(group.First().Color));

            bool wasCompleted = CurrentLevel.IsCompleted;
            CurrentLevel.IsCompleted = allCompleted;

            if (allCompleted && !wasCompleted)
            {
                if (HasNextLevel && !Levels[CurrentLevelIndex + 1].IsCompleted)
                {
                    Levels[CurrentLevelIndex + 1].IsCompleted = false; // Ensure next level remains unlocked
                }
                SaveProgress();
            }
            else if (!allCompleted && HasNextLevel)
            {
                // Do not lock the next level even if the current level is incomplete after reset
                Levels[CurrentLevelIndex + 1].IsCompleted = true;
            }
        }

        public void StartNewPath(ModelPoint point)
        {
            ResetPathState();
            LastSelectedPoint = point;
            CurrentPathColor = point.Color;
            CurrentPathId = $"{point.Color}-{point.Row}-{point.Column}-path";
            point.IsConnected = true;
            CurrentPath.Add(point);

            Console.WriteLine($"Started new path with ID: {CurrentPathId}");
        }

        public bool IsPointInCurrentPath(ModelPoint point)
        {
            return CurrentPath.Any(p => p.Row == point.Row && p.Column == point.Column);
        }

        public bool RemoveLastPointFromPath()
        {
            if (CurrentPath.Count <= 1)
                return false;

            CurrentPath.RemoveAt(CurrentPath.Count - 1);
            LastSelectedPoint = CurrentPath.LastOrDefault();

            return true;
        }
    }
}