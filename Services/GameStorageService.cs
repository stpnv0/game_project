using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;
using ConnectDotsGame.Models;

namespace ConnectDotsGame.Services
{
    public class GameStorageService : IGameStorageService
    {
        private readonly string _savePath;
        private record LevelProgress(int Id, bool WasEverCompleted);

        public GameStorageService()
        {
            var appDataPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                "ConnectDotsGame"
            );
            Directory.CreateDirectory(appDataPath);
            _savePath = System.IO.Path.Combine(appDataPath, "progress.json");
        }
        
        // Сохраняет прогресс
        public void SaveProgress(List<Level> levels)
        {
            var progressList = levels.Select(level => new LevelProgress(level.Id, level.WasEverCompleted)).ToList();
            File.WriteAllText(_savePath, JsonSerializer.Serialize(progressList));
            Console.WriteLine($"Прогресс сохранен в файл: {_savePath}");
        }
        
        // Загружает прогресс
        public void LoadProgress(List<Level> levels)
        {
            if (!File.Exists(_savePath)) return;
            
            var progress = JsonSerializer.Deserialize<List<LevelProgress>>(File.ReadAllText(_savePath));
            foreach (var p in progress!)
            {
                var level = levels.Find(l => l.Id == p.Id);
                if (level != null) level.WasEverCompleted = p.WasEverCompleted;
            }
        }
    }
} 