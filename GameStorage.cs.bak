using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using ConnectDotsGame.Models;

namespace ConnectDotsGame.Utils
{
    public class GameStorage
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
                        IsCompleted = level.IsCompleted
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
                        Console.WriteLine($"Загружен прогресс для уровня {level.Id}: пройден = {level.IsCompleted}");
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
        }
    }
} 