using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Avalonia.Media;
using ConnectDotsGame.Models;

namespace ConnectDotsGame.Levels
{
    public class LevelLoader
    {
        public List<Level> LoadLevels()
        {
            var levels = new List<Level>();
            
            try
            {
                // Загружаем все уровни
                levels.Add(CreateLevel1());
                levels.Add(CreateLevel2());
                levels.Add(CreateLevel3());
                levels.Add(CreateLevel4());
                levels.Add(CreateLevel5());
                levels.Add(CreateLevel6());
                levels.Add(CreateLevel7());
                levels.Add(CreateLevel8());
                levels.Add(CreateLevel9());
                levels.Add(CreateLevel10());
                
                Console.WriteLine($"Загружено {levels.Count} уровней");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке уровней: {ex.Message}");
            }
            
            return levels;
        }

        // Вспомогательный метод для получения точки по координатам
        private Point GetPointAt(Level level, int row, int col)
        {
            return level.Points.Find(p => p.Row == row && p.Column == col)
                ?? throw new InvalidOperationException($"Точка с координатами {row},{col} не найдена");
        }

        private Level CreateLevel1()
        {
            var level = new Level
            {
                Id = 1,
                Name = "Уровень 1",
                Rows = 5,
                Columns = 5,
                Points = new List<Point>(),
                Lines = new List<Line>()
            };
            
            int id = 1;
            for (int row = 0; row < 5; row++)
            {
                for (int col = 0; col < 5; col++)
                {
                    level.Points.Add(new Point(id++, row, col));
                }
            }

            GetPointAt(level, 0, 0).Color = Brushes.Red;
            GetPointAt(level, 0, 4).Color = Brushes.Red;

            GetPointAt(level, 4, 0).Color = Brushes.Blue;
            GetPointAt(level, 4, 4).Color = Brushes.Blue;
            return level;
        }
        
        private Level CreateLevel2()
        {
            var level = new Level
            {
                Id = 2,
                Name = "Уровень 2",
                Rows = 5,
                Columns = 5,
                Points = new List<Point>(),
                Lines = new List<Line>()
            };
            
            int id = 1;
            for (int row = 0; row < 5; row++)
            {
                for (int col = 0; col < 5; col++)
                {
                    level.Points.Add(new Point(id++, row, col));
                }
            }

            // Добавляем цветные точки для проходимого уровня
            GetPointAt(level, 0, 0).Color = Brushes.Red;
            GetPointAt(level, 0, 4).Color = Brushes.Red;

            GetPointAt(level, 4, 0).Color = Brushes.Blue;
            GetPointAt(level, 4, 4).Color = Brushes.Blue;

            GetPointAt(level, 2, 2).Color = Brushes.Green;
            GetPointAt(level, 3, 3).Color = Brushes.Green;
            
            return level;
        }
        
        private Level CreateLevel3()
        {
            var level = new Level
            {
                Id = 3,
                Name = "Уровень 3",
                Rows = 7,
                Columns = 7,
                Points = new List<Point>(),
                Lines = new List<Line>()
            };
            
            int id = 1;
            for (int row = 0; row < 7; row++)
            {
                for (int col = 0; col < 7; col++)
                {
                    level.Points.Add(new Point(id++, row, col));
                }
            }

            GetPointAt(level, 0, 1).Color = Brushes.Red;
            GetPointAt(level, 0, 5).Color = Brushes.Red;

            GetPointAt(level, 6, 1).Color = Brushes.Blue;
            GetPointAt(level, 6, 5).Color = Brushes.Blue;

            GetPointAt(level, 1, 0).Color = Brushes.Green;
            GetPointAt(level, 5, 0).Color = Brushes.Green;

            GetPointAt(level, 1, 6).Color = Brushes.Yellow;
            GetPointAt(level, 5, 6).Color = Brushes.Yellow;
            
            
            
            return level;
        }
        
        // Уровень 4: Сложный (8x8)
        private Level CreateLevel4()
        {
            var level = new Level
            {
                Id = 4,
                Name = "Сложный",
                Rows = 8,
                Columns = 8,
                Points = new List<Point>(),
                Lines = new List<Line>()
            };
            
            int id = 1;
            for (int row = 0; row < level.Rows; row++)
            {
                for (int col = 0; col < level.Columns; col++)
                {
                    var point = new Point(id++, row, col);
                    level.Points.Add(point);
                }
            }
            
            GetPointAt(level, 0, 1).Color = Brushes.Red;
            GetPointAt(level, 0, 6).Color = Brushes.Red;
            
            GetPointAt(level, 1, 0).Color = Brushes.Blue;
            GetPointAt(level, 6, 0).Color = Brushes.Blue;
            
            GetPointAt(level, 1, 7).Color = Brushes.Green;
            GetPointAt(level, 6, 7).Color = Brushes.Green;
            
            GetPointAt(level, 7, 1).Color = Brushes.Yellow;
            GetPointAt(level, 7, 6).Color = Brushes.Yellow;
            
            GetPointAt(level, 3, 3).Color = Brushes.Orange;
            GetPointAt(level, 4, 2).Color = Brushes.Orange;
            
            GetPointAt(level, 3, 4).Color = Brushes.Purple;
            GetPointAt(level, 4, 5).Color = Brushes.Purple;
            
            return level;
        }
        
        // Уровень 5: Эксперт (9x9)
        private Level CreateLevel5()
        {
            var level = new Level
            {
                Id = 5,
                Name = "Эксперт",
                Rows = 9,
                Columns = 9,
                Points = new List<Point>(),
                Lines = new List<Line>()
            };
            
            // Создаем все точки сетки
            int id = 1;
            for (int row = 0; row < level.Rows; row++)
            {
                for (int col = 0; col < level.Columns; col++)
                {
                    var point = new Point(id++, row, col);
                    level.Points.Add(point);
                }
            }
            
            // Добавляем цветные точки для проходимого уровня
            // Красные - по верхнему краю
            GetPointAt(level, 0, 2).Color = Brushes.Red;
            GetPointAt(level, 0, 6).Color = Brushes.Red;
            
            // Синие - левая верхняя область
            GetPointAt(level, 2, 0).Color = Brushes.Blue;
            GetPointAt(level, 2, 3).Color = Brushes.Blue;
            
            // Зеленые - правая верхняя область
            GetPointAt(level, 2, 5).Color = Brushes.Green;
            GetPointAt(level, 2, 8).Color = Brushes.Green;
            
            // Желтые - левая нижняя область
            GetPointAt(level, 6, 0).Color = Brushes.Yellow;
            GetPointAt(level, 6, 3).Color = Brushes.Yellow;
            
            // Оранжевые - правая нижняя область
            GetPointAt(level, 6, 5).Color = Brushes.Orange;
            GetPointAt(level, 6, 8).Color = Brushes.Orange;
            
            // Пурпурные - по нижнему краю
            GetPointAt(level, 8, 2).Color = Brushes.Purple;
            GetPointAt(level, 8, 6).Color = Brushes.Purple;
            
            // Бирюзовые - по центру
            GetPointAt(level, 4, 3).Color = Brushes.Cyan;
            GetPointAt(level, 4, 5).Color = Brushes.Cyan;
            
            return level;
        }
        
        
        
        // Уровень 6: Мастер (9x9)
        private Level CreateLevel6()
        {
            var level = new Level
            {
                Id = 6,
                Name = "Мастер",
                Rows = 9,
                Columns = 9,
                Points = new List<Point>(),
                Lines = new List<Line>()
            };
            
            // Создаем все точки сетки
            int id = 1;
            for (int row = 0; row < level.Rows; row++)
            {
                for (int col = 0; col < level.Columns; col++)
                {
                    var point = new Point(id++, row, col);
                    level.Points.Add(point);
                }
            }
            
            // Добавляем цветные точки для проходимого уровня
            // Красные - верхний ряд
            GetPointAt(level, 0, 1).Color = Brushes.Red;
            GetPointAt(level, 0, 7).Color = Brushes.Red;
            
            // Синие - правый край
            GetPointAt(level, 1, 8).Color = Brushes.Blue;
            GetPointAt(level, 7, 8).Color = Brushes.Blue;
            
            // Зеленые - нижний ряд
            GetPointAt(level, 8, 1).Color = Brushes.Green;
            GetPointAt(level, 8, 7).Color = Brushes.Green;
            
            // Фиолетовые - левый край
            GetPointAt(level, 1, 0).Color = Brushes.Purple;
            GetPointAt(level, 7, 0).Color = Brushes.Purple;
            
            // Желтые - внутренний квадрат (верх)
            GetPointAt(level, 2, 2).Color = Brushes.Yellow;
            GetPointAt(level, 2, 6).Color = Brushes.Yellow;
            
            // Бирюзовые - внутренний квадрат (низ)
            GetPointAt(level, 6, 2).Color = Brushes.Cyan;
            GetPointAt(level, 6, 6).Color = Brushes.Cyan;
            
            return level;
        }
        
        // Уровень 7: Профи (10x10)
        private Level CreateLevel7()
        {
            var level = new Level
            {
                Id = 7,
                Name = "Профи",
                Rows = 10,
                Columns = 10,
                Points = new List<Point>(),
                Lines = new List<Line>()
            };
            
            // Создаем все точки сетки
            int id = 1;
            for (int row = 0; row < level.Rows; row++)
            {
                for (int col = 0; col < level.Columns; col++)
                {
                    var point = new Point(id++, row, col);
                    level.Points.Add(point);
                }
            }
            
            // Добавляем цветные точки для проходимого уровня
            // Красные - верхний ряд
            GetPointAt(level, 0, 2).Color = Brushes.Red;
            GetPointAt(level, 0, 7).Color = Brushes.Red;
            
            // Синие - второй ряд
            GetPointAt(level, 1, 1).Color = Brushes.Blue;
            GetPointAt(level, 1, 8).Color = Brushes.Blue;
            
            // Зеленые - третий ряд
            GetPointAt(level, 2, 0).Color = Brushes.Green;
            GetPointAt(level, 2, 9).Color = Brushes.Green;
            
            // Фиолетовые - левый центр
            GetPointAt(level, 4, 1).Color = Brushes.Purple;
            GetPointAt(level, 4, 9).Color = Brushes.Purple;
            
            // Желтые - правый центр
            GetPointAt(level, 4, 8).Color = Brushes.Yellow;
            GetPointAt(level, 5, 0).Color = Brushes.Yellow;
            
            // Оранжевые - седьмой ряд
            GetPointAt(level, 7, 0).Color = Brushes.Orange;
            GetPointAt(level, 7, 9).Color = Brushes.Orange;
            
            // Бирюзовые - нижние ряды
            GetPointAt(level, 9, 2).Color = Brushes.Cyan;
            GetPointAt(level, 9, 7).Color = Brushes.Cyan;
            
            return level;
        }
        
        // Уровень 8: Вызов (10x10)
        private Level CreateLevel8()
        {
            var level = new Level
            {
                Id = 8,
                Name = "Вызов",
                Rows = 10,
                Columns = 10,
                Points = new List<Point>(),
                Lines = new List<Line>()
            };
            
            // Создаем все точки сетки
            int id = 1;
            for (int row = 0; row < level.Rows; row++)
            {
                for (int col = 0; col < level.Columns; col++)
                {
                    var point = new Point(id++, row, col);
                    level.Points.Add(point);
                }
            }
            
            // Добавляем цветные точки для проходимого уровня
            // Красные - верхний левый угол
            GetPointAt(level, 0, 0).Color = Brushes.Red;
            GetPointAt(level, 1, 1).Color = Brushes.Red;
            
            // Синие - верхний правый угол
            GetPointAt(level, 0, 9).Color = Brushes.Blue;
            GetPointAt(level, 1, 8).Color = Brushes.Blue;
            
            // Зеленые - нижний левый угол
            GetPointAt(level, 9, 0).Color = Brushes.Green;
            GetPointAt(level, 8, 1).Color = Brushes.Green;
            
            // Фиолетовые - нижний правый угол
            GetPointAt(level, 9, 9).Color = Brushes.Purple;
            GetPointAt(level, 8, 8).Color = Brushes.Purple;
            
            // Желтые - верхний центр
            GetPointAt(level, 1, 4).Color = Brushes.Yellow;
            GetPointAt(level, 4, 4).Color = Brushes.Yellow;
            
            // Оранжевые - нижний центр
            GetPointAt(level, 8, 4).Color = Brushes.Orange;
            GetPointAt(level, 5, 4).Color = Brushes.Orange;
            
            // Серые - левый центр
            GetPointAt(level, 4, 1).Color = Brushes.Gray;
            GetPointAt(level, 5, 9).Color = Brushes.Gray;
            
            // Бирюзовые - правый центр
            GetPointAt(level, 4, 0).Color = Brushes.Cyan;
            GetPointAt(level, 5, 8).Color = Brushes.Cyan;
            
            return level;
        }
        
        // Уровень 9: Гуру (11x11)
        private Level CreateLevel9()
        {
            var level = new Level
            {
                Id = 9,
                Name = "Гуру",
                Rows = 11,
                Columns = 11,
                Points = new List<Point>(),
                Lines = new List<Line>()
            };
            
            // Создаем все точки сетки
            int id = 1;
            for (int row = 0; row < level.Rows; row++)
            {
                for (int col = 0; col < level.Columns; col++)
                {
                    var point = new Point(id++, row, col);
                    level.Points.Add(point);
                }
            }
            
            // Добавляем цветные точки для проходимого уровня
            // Красные - верхний ряд
            GetPointAt(level, 0, 1).Color = Brushes.Red;
            GetPointAt(level, 0, 9).Color = Brushes.Red;
            
            // Синие - правая колонка
            GetPointAt(level, 1, 10).Color = Brushes.Blue;
            GetPointAt(level, 9, 10).Color = Brushes.Blue;
            
            // Зеленые - нижний ряд
            GetPointAt(level, 10, 1).Color = Brushes.Green;
            GetPointAt(level, 10, 9).Color = Brushes.Green;
            
            // Фиолетовые - левая колонка
            GetPointAt(level, 1, 0).Color = Brushes.Purple;
            GetPointAt(level, 9, 0).Color = Brushes.Purple;
            
            // Желтые - первое кольцо (верх и низ)
            GetPointAt(level, 2, 2).Color = Brushes.Yellow;
            GetPointAt(level, 2, 8).Color = Brushes.Yellow;
            
            // Оранжевые - первое кольцо (лево и право)
            GetPointAt(level, 8, 2).Color = Brushes.Orange;
            GetPointAt(level, 8, 8).Color = Brushes.Orange;
            
            // Серые - внутреннее кольцо (верх)
            GetPointAt(level, 4, 4).Color = Brushes.Gray;
            GetPointAt(level, 4, 6).Color = Brushes.Gray;
            
            // Бирюзовые - внутреннее кольцо (низ)
            GetPointAt(level, 6, 4).Color = Brushes.Cyan;
            GetPointAt(level, 6, 6).Color = Brushes.Cyan;
            
            return level;
        }
        
        // Уровень 10: Невозможный (12x12)
        private Level CreateLevel10()
        {
            var level = new Level
            {
                Id = 10,
                Name = "Невозможный",
                Rows = 12,
                Columns = 12,
                Points = new List<Point>(),
                Lines = new List<Line>()
            };
            
            // Создаем все точки сетки
            int id = 1;
            for (int row = 0; row < level.Rows; row++)
            {
                for (int col = 0; col < level.Columns; col++)
                {
                    var point = new Point(id++, row, col);
                    level.Points.Add(point);
                }
            }
            
            // Добавляем цветные точки для проходимого уровня
            // Красные - верхний край
            GetPointAt(level, 0, 2).Color = Brushes.Red;
            GetPointAt(level, 0, 9).Color = Brushes.Red;
            
            // Синие - правый край
            GetPointAt(level, 2, 11).Color = Brushes.Blue;
            GetPointAt(level, 9, 11).Color = Brushes.Blue;
            
            // Зеленые - нижний край
            GetPointAt(level, 11, 2).Color = Brushes.Green;
            GetPointAt(level, 11, 9).Color = Brushes.Green;
            
            // Фиолетовые - левый край
            GetPointAt(level, 2, 0).Color = Brushes.Purple;
            GetPointAt(level, 9, 0).Color = Brushes.Purple;
            
            // Желтые - внешнее кольцо (верх)
            GetPointAt(level, 1, 4).Color = Brushes.Yellow;
            GetPointAt(level, 1, 7).Color = Brushes.Yellow;
            
            // Оранжевые - внешнее кольцо (право)
            GetPointAt(level, 4, 10).Color = Brushes.Orange;
            GetPointAt(level, 7, 10).Color = Brushes.Orange;
            
            // Серые - внешнее кольцо (низ)
            GetPointAt(level, 10, 4).Color = Brushes.Gray;
            GetPointAt(level, 10, 7).Color = Brushes.Gray;
            
            // Бирюзовые - внешнее кольцо (лево)
            GetPointAt(level, 4, 1).Color = Brushes.Cyan;
            GetPointAt(level, 7, 1).Color = Brushes.Cyan;
            
            // Розовые - внутреннее кольцо (верх)
            GetPointAt(level, 3, 4).Color = Brushes.Pink;
            GetPointAt(level, 3, 7).Color = Brushes.Pink;
            
            return level;
        }
    }
} 