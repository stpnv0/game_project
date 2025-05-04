using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using ConnectDotsGame.Models;
using System.Linq;

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
                // Формируем список данных о прогрессе для сохранения.
                var progressData = levels.Select(level => new LevelProgressData
                {
                    Id = level.Id, // ID уровня.
                    IsCompleted = level.IsCompleted // Завершенность уровня.
                }).ToList();

                // Сериализуем данные прогресса в формат JSON.
                string json = JsonSerializer.Serialize(progressData);

                // Сохраняем данные в файл.
                File.WriteAllText(_savePath, json);
            }
            catch (Exception ex)
            {
                // Логируем ошибку в случае возникновения исключения.
                Console.WriteLine($"Ошибка сохранения прогресса: {ex.Message}");
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
                // Применяем загруженные данные прогресса к текущему состоянию уровней.
                foreach (var progress in progressData)
                {
                    var level = levels.FirstOrDefault(l => l.Id == progress.Id);
                    if (level != null)
                    {
                        level.IsCompleted = progress.IsCompleted; // Восстанавливаем завершенность уровня.
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