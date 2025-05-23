using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Avalonia.Media;
using ModelPoint = ConnectDotsGame.Models.Point;

namespace ConnectDotsGame.Models
{
    public class GameState
    {
        private readonly GameStorage _gameStorage;

        public string? CurrentPathId { get; private set; } // Сделаем private set
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
                
                // Создаем директорию, если она не существует
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
                    // версия прогресса для сохранения
                    var progressData = new List<LevelProgressData>();
                    
                    foreach (var level in levels)
                    {
                        progressData.Add(new LevelProgressData
                        {
                            Id = level.Id,
                            IsCompleted = level.IsCompleted,
                            WasEverCompleted = level.WasEverCompleted
                        });
                    }
                    
                    // Сериализуем и сохраняем
                    string json = JsonSerializer.Serialize(progressData);
                    File.WriteAllText(_savePath, json);
                    
                    Console.WriteLine($"Прогресс успешно сохранен: {_savePath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при сохранении прогресса: {ex.Message}");
                }
            }
            
            public void LoadProgress(List<Level> levels)
            {
                try
                {
                    if (!File.Exists(_savePath))
                    {
                        Console.WriteLine("Файл сохранения не найден. Используем начальное состояние.");
                        return;
                    }
                    
                    string json = File.ReadAllText(_savePath);
                    var progressData = JsonSerializer.Deserialize<List<LevelProgressData>>(json);
                    
                    if (progressData == null)
                    {
                        Console.WriteLine("Ошибка десериализации. Используем начальное состояние.");
                        return;
                    }
                    
                    //сохранения прменяются к уровням
                    foreach (var levelProgress in progressData)
                    {
                        var level = levels.Find(l => l.Id == levelProgress.Id);
                        if (level != null)
                        {
                            level.IsCompleted = levelProgress.IsCompleted;
                            level.WasEverCompleted = levelProgress.WasEverCompleted;
                            Console.WriteLine($"Загружен прогресс для уровня {level.Id}: пройден = {level.IsCompleted}, был пройден = {level.WasEverCompleted}");
                        }
                    }
                    
                    Console.WriteLine($"Прогресс успешно загружен из {_savePath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при загрузке прогресса: {ex.Message}");
                }
            }
            
            // Класс для сериализации данных о прогрессе
            private class LevelProgressData
            {
                public int Id { get; set; }
                public bool IsCompleted { get; set; }
                public bool WasEverCompleted { get; set; }
            }
        }
        
        public Level? CurrentLevel
        {
            get
            {
                if (CurrentLevelIndex >= 0 && CurrentLevelIndex < Levels.Count)
                {
                    return Levels[CurrentLevelIndex];
                }
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
                CurrentLevel.WasEverCompleted = wasEverCompleted; // Preserve WasEverCompleted state
            }
        }
        
        public void ResetPathState()
        {
            LastSelectedPoint = null;
            CurrentPathColor = null;
            CurrentPath.Clear();
            CurrentPathId = null; 
        }
        
        public void StartNewPath(ModelPoint point)
        {

            ResetPathState();
            LastSelectedPoint = point;
            CurrentPathColor = point.Color;
            CurrentPathId = $"{point.Color}-path";
            point.IsConnected = true;
            CurrentPath.Add(point);
        }

        public (bool isInPath, int index) CheckPointInPath(ModelPoint point)
        {            
            int index = CurrentPath.FindIndex(p => p.Row == point.Row && p.Column == point.Column);
            return (index != -1, index);
        }
        
        public bool RemoveLastPointFromPath()
        {            
            if (CurrentPath.Count <= 1)
                return false;
                
            CurrentPath.RemoveAt(CurrentPath.Count - 1);
            LastSelectedPoint = CurrentPath.Count > 0 ? CurrentPath.Last() : null;
            
            return true;
        }
        
        public bool RemovePointsAfter(int index)
        {            
            if (index < 0 || index >= CurrentPath.Count - 1)
                return false;
                
            CurrentPath.RemoveRange(index + 1, CurrentPath.Count - index - 1);
            LastSelectedPoint = CurrentPath.Count > 0 ? CurrentPath.Last() : null;
            
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