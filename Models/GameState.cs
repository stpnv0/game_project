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
        
        // Загрузка прогресса игры
        public void LoadProgress()
        {
            _gameStorage.LoadProgress(Levels);
        }
        
        // Сохранение прогресса игры
        public void SaveProgress()
        {
            _gameStorage.SaveProgress(Levels);
        }

        // Встроенный класс GameStorage для сохранения и загрузки прогресса
        private class GameStorage
        {
            private readonly string _savePath;

            public GameStorage()
            {
                // Путь к файлу сохранения в папке с данными приложения
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
                    // Создаем упрощенную версию прогресса для сохранения
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
                    
                    // Применяем сохраненное состояние к уровням
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
                    // Загружаем актуальный прогресс при каждом обращении к текущему уровню
                    LoadProgress();
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
            CurrentPathId = null; // Сбрасываем ID
        }
        
        public void StartNewPath(ModelPoint point)
        {
            ResetPathState();
            LastSelectedPoint = point;
            CurrentPathColor = point.Color;
            // Генерируем УНИКАЛЬНЫЙ PathId
            CurrentPathId = $"{point.Color}-{point.Row}-{point.Column}-path"; // Пример уникального ID
            point.IsConnected = true;
            CurrentPath.Add(point);
            Console.WriteLine($"Started new path with ID: {CurrentPathId}"); // Для отладки
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
            
            LastSelectedPoint = CurrentPath.Last();
            
            return true;
        }
        
        public void CheckCompletedPaths()
        {
            if (CurrentLevel == null)
                return;
                
            bool allCompleted = true;
            
            var colorGroups = CurrentLevel.Points
                .Where(p => p.HasColor)
                .GroupBy(p => p.Color.ToString())
                .ToList();
                
            foreach (var group in colorGroups)
            {
                bool colorCompleted = CurrentLevel.IsPathComplete(group.First().Color);
                if (!colorCompleted)
                {
                    allCompleted = false;
                }
            }
            
            bool wasCompleted = CurrentLevel.IsCompleted;
            CurrentLevel.IsCompleted = allCompleted;
            
            // Если уровень завершен
            if (allCompleted)
            {
                CurrentLevel.WasEverCompleted = true; // Отмечаем как пройденный хотя бы раз
                
                // Сохраняем прогресс в любом случае, чтобы обновить WasEverCompleted
                SaveProgress();
            }
        }
    }
}