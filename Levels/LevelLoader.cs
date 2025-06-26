using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Avalonia.Media;
using ConnectDotsGame.Models;

namespace ConnectDotsGame.Levels
{
    public class LevelLoader
    {
        private readonly string _levelsPath;
        private readonly Dictionary<string, IBrush> _colorMap;

        // Загрузчик уровней с указанным путем
        public LevelLoader(string levelsPath = "Levels/levels.json")
        {
            _levelsPath = System.IO.Path.GetFullPath(levelsPath);
            _colorMap = new Dictionary<string, IBrush>
            {
                { "Red", Brushes.Red },
                { "Blue", Brushes.Blue },
                { "Green", Brushes.Green },
                { "Yellow", Brushes.Yellow },
                { "Purple", Brushes.Purple },
                { "Orange", Brushes.Orange },
                { "Pink", Brushes.Pink },
                { "Cyan", Brushes.Cyan },
                { "Gray", Brushes.Gray },
                { "Lime", Brushes.Lime }
            };
        }

        // Загружает все уровни из JSON файла и преобразует их в доменные модели
        public List<Level> LoadLevels()
        {
            var levels = new List<Level>();
            
            try
            {
                if (!File.Exists(_levelsPath))
                {
                    throw new FileNotFoundException($"Файл уровней не найден по пути: {_levelsPath}");
                }

                // Читаем и десериализуем JSON
                var jsonString = File.ReadAllText(_levelsPath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip
                };
                
                var levelData = JsonSerializer.Deserialize<LevelData>(jsonString, options);

                // Проверяем успешность десериализации
                if (levelData?.Levels == null)
                {
                    throw new InvalidOperationException("Не удалось загрузить уровни из JSON файла: данные отсутствуют");
                }

                // Преобразуем каждый уровень из JSON в модель
                foreach (var levelDto in levelData.Levels)
                {
                    try 
                    {
                        levels.Add(levelDto.ToLevel(_colorMap));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при создании уровня {levelDto.Id}: {ex.Message}");
                    }
                }
                
                Console.WriteLine($"Загружено {levels.Count} уровней");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке уровней: {ex.Message}");
            }
            
            return levels;
        }
    }
} 