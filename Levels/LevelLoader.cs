using System;
using System.Collections.Generic;
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
            GetPointAt(level, 2, 3).Color = Brushes.Red;

            GetPointAt(level, 0, 4).Color = Brushes.Green;
            GetPointAt(level, 1, 2).Color = Brushes.Green;

            GetPointAt(level, 1, 0).Color = Brushes.Yellow;
            GetPointAt(level, 1, 3).Color = Brushes.Yellow;

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

            
            GetPointAt(level, 0, 0).Color = Brushes.Red;
            GetPointAt(level, 3, 0).Color = Brushes.Red;

            GetPointAt(level, 4, 0).Color = Brushes.Blue;
            GetPointAt(level, 4, 4).Color = Brushes.Blue;

            GetPointAt(level, 2, 2).Color = Brushes.Green;
            GetPointAt(level, 3, 3).Color = Brushes.Green;

            GetPointAt(level, 1, 2).Color = Brushes.Yellow;
            GetPointAt(level, 0, 3).Color = Brushes.Yellow;

            GetPointAt(level, 0, 2).Color = Brushes.Purple;
            GetPointAt(level, 3, 2).Color = Brushes.Purple;

            GetPointAt(level, 0, 4).Color = Brushes.Orange;
            GetPointAt(level, 3, 4).Color = Brushes.Orange;

            
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

            GetPointAt(level, 1, 1).Color = Brushes.Purple;
            GetPointAt(level, 5, 5).Color = Brushes.Purple;

            GetPointAt(level, 2, 4).Color = Brushes.Gray;
            GetPointAt(level, 4, 2).Color = Brushes.Gray;

            GetPointAt(level, 2, 3).Color = Brushes.Orange;
            GetPointAt(level, 3, 2).Color = Brushes.Orange;

            GetPointAt(level, 2, 2).Color = Brushes.Pink;
            GetPointAt(level, 4, 5).Color = Brushes.Pink;


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
        
        private Level CreateLevel4()
        {
            var level = new Level
            {
                Id = 4,
                Name = "Уровень 4",
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

            GetPointAt(level, 5, 2).Color = Brushes.Cyan;
            GetPointAt(level, 5, 6).Color = Brushes.Cyan;

            GetPointAt(level, 4 ,6).Color = Brushes.Gray;
            GetPointAt(level, 6, 6).Color = Brushes.Gray;

            GetPointAt(level, 3, 2).Color = Brushes.Pink;
            GetPointAt(level, 3, 5).Color = Brushes.Pink;

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
        
        private Level CreateLevel5()
        {
            var level = new Level
            {
                Id = 5,
                Name = "Уровень 5",
                Rows = 9,
                Columns = 9,
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

            
            GetPointAt(level, 0, 0).Color = Brushes.Red;
            GetPointAt(level, 5, 0).Color = Brushes.Red;

            
            GetPointAt(level, 0, 8).Color = Brushes.Blue;
            GetPointAt(level, 8, 0).Color = Brushes.Blue;

           
            GetPointAt(level, 4, 0).Color = Brushes.Green;
            GetPointAt(level, 3, 6).Color = Brushes.Green;


            
            GetPointAt(level, 4, 2).Color = Brushes.Pink;
            GetPointAt(level, 4, 6).Color = Brushes.Pink;

           
            GetPointAt(level, 1, 1).Color = Brushes.Orange;
            GetPointAt(level, 1, 7).Color = Brushes.Orange;

            
            GetPointAt(level, 7, 1).Color = Brushes.Cyan;
            GetPointAt(level, 7, 7).Color = Brushes.Cyan;

            return level;
        }
        
        private Level CreateLevel6()
        {
            var level = new Level
            {
                Id = 6,
                Name = "Уровень 6",
                Rows = 9,
                Columns = 9,
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

            
            GetPointAt(level, 0, 2).Color = Brushes.Red;
            GetPointAt(level, 8, 4).Color = Brushes.Red;
           

            GetPointAt(level, 0, 8).Color = Brushes.Blue;
            GetPointAt(level, 5, 8).Color = Brushes.Blue;
            
          
            GetPointAt(level, 0, 0).Color = Brushes.Green;
            GetPointAt(level, 6, 1).Color = Brushes.Green;
            
            
            GetPointAt(level, 0, 5).Color = Brushes.Purple;
            GetPointAt(level, 5, 7).Color = Brushes.Purple;
            
            
            GetPointAt(level, 0, 1).Color = Brushes.Yellow;
            GetPointAt(level, 8, 3).Color = Brushes.Yellow;
            
            
            GetPointAt(level, 2, 2).Color = Brushes.Cyan;
            GetPointAt(level, 7, 3).Color = Brushes.Cyan;


            GetPointAt(level, 6, 7).Color = Brushes.Pink;
            GetPointAt(level, 8, 8).Color = Brushes.Pink;

            return level;
        }
        
        private Level CreateLevel7()
        {
            var level = new Level
            {
                Id = 7,
                Name = "Уровень 7",
                Rows = 10,
                Columns = 10,
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
            
            
            GetPointAt(level, 0, 0).Color = Brushes.Red;
            GetPointAt(level, 5, 0).Color = Brushes.Red;
            
           
            GetPointAt(level, 0, 7).Color = Brushes.Blue;
            GetPointAt(level, 8, 8).Color = Brushes.Blue;
            
            
            GetPointAt(level, 0, 1).Color = Brushes.Green;
            GetPointAt(level, 4, 1).Color = Brushes.Green;
            
            
            GetPointAt(level, 9, 9).Color = Brushes.Purple;
            GetPointAt(level, 7, 8).Color = Brushes.Purple;
            
            
            GetPointAt(level, 2, 8).Color = Brushes.Yellow;
            GetPointAt(level, 3, 6).Color = Brushes.Yellow;
            
            
            GetPointAt(level, 5, 1).Color = Brushes.Orange;
            GetPointAt(level, 8, 1).Color = Brushes.Orange;
            
         
            GetPointAt(level, 3, 7).Color = Brushes.Cyan;
            GetPointAt(level, 6, 9).Color = Brushes.Cyan;

            GetPointAt(level, 5, 8).Color = Brushes.Pink;
            GetPointAt(level, 8, 7).Color = Brushes.Pink;

            GetPointAt(level, 3, 4).Color = Brushes.Gray;
            GetPointAt(level, 7, 7).Color = Brushes.Gray;

            return level;
        }
        
        private Level CreateLevel8()
        {
            var level = new Level
            {
                Id = 8,
                Name = "Уровень 8",
                Rows = 10,
                Columns = 10,
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

            
            GetPointAt(level, 1, 1).Color = Brushes.Gray;
            GetPointAt(level, 5, 4).Color = Brushes.Gray;

            
            GetPointAt(level, 8, 1).Color = Brushes.Blue;
            GetPointAt(level, 8, 8).Color = Brushes.Blue;

            
            GetPointAt(level, 2, 2).Color = Brushes.Cyan;
            GetPointAt(level, 4, 3).Color = Brushes.Cyan;

          
            GetPointAt(level, 3, 3).Color = Brushes.Yellow;
            GetPointAt(level, 4, 9).Color = Brushes.Yellow;


            GetPointAt(level, 9, 1).Color = Brushes.Pink;
            GetPointAt(level, 9, 9).Color = Brushes.Pink;

            
            GetPointAt(level, 0, 0).Color = Brushes.Orange;
            GetPointAt(level, 5, 9).Color = Brushes.Orange;

            
            GetPointAt(level, 1, 8).Color = Brushes.Red;
            GetPointAt(level, 2, 5).Color = Brushes.Red;

            
            GetPointAt(level, 2, 7).Color = Brushes.Purple;
            GetPointAt(level, 2, 4).Color = Brushes.Purple;

            GetPointAt(level, 6, 9).Color = Brushes.Green;
            GetPointAt(level, 9, 0).Color = Brushes.Green;

            return level;
        }
        
        private Level CreateLevel9()
        {
            var level = new Level
            {
                Id = 9,
                Name = "Уровень 9",
                Rows = 11,
                Columns = 11,
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

            
            GetPointAt(level, 6, 0).Color = Brushes.Red;
            GetPointAt(level, 10, 0).Color = Brushes.Red;

        
            GetPointAt(level, 0, 1).Color = Brushes.Blue;
            GetPointAt(level, 4, 0).Color = Brushes.Blue;

            
            GetPointAt(level, 7, 3).Color = Brushes.Cyan;
            GetPointAt(level, 8, 8).Color = Brushes.Cyan;

            
            GetPointAt(level, 6, 5).Color = Brushes.Green;
            GetPointAt(level, 9, 2).Color = Brushes.Green;

           
            GetPointAt(level, 3, 5).Color = Brushes.Yellow;
            GetPointAt(level, 7, 7).Color = Brushes.Yellow;

            
            GetPointAt(level, 0, 2).Color = Brushes.Orange;
            GetPointAt(level, 0, 10).Color = Brushes.Orange;

           
            GetPointAt(level, 5, 0).Color = Brushes.Gray;
            GetPointAt(level, 1, 2).Color = Brushes.Gray;

           
            GetPointAt(level, 4, 10).Color = Brushes.Purple;
            GetPointAt(level, 6, 1).Color = Brushes.Purple;

            GetPointAt(level, 10, 3).Color = Brushes.Pink;
            GetPointAt(level, 8, 2).Color = Brushes.Pink;

            

            return level;
        }
        
        private Level CreateLevel10()
        {
            var level = new Level
            {
                Id = 10,
                Name = "Уровень 10",
                Rows = 12,
                Columns = 12,
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


            GetPointAt(level, 0, 0).Color = Brushes.Red;
            GetPointAt(level, 11, 0).Color = Brushes.Red;

            
            GetPointAt(level, 0, 11).Color = Brushes.Blue;
            GetPointAt(level, 10, 10).Color = Brushes.Blue;

          
            GetPointAt(level, 1, 2).Color = Brushes.Green;
            GetPointAt(level, 7, 6).Color = Brushes.Green;

            
            GetPointAt(level, 2, 10).Color = Brushes.Purple;
            GetPointAt(level, 7, 11).Color = Brushes.Purple;

         
            GetPointAt(level, 3, 4).Color = Brushes.Yellow;
            GetPointAt(level, 8, 11).Color = Brushes.Yellow;

         
            GetPointAt(level, 9, 10).Color = Brushes.Orange;
            GetPointAt(level, 11, 1).Color = Brushes.Orange;


        
            GetPointAt(level, 5, 5).Color = Brushes.Gray;
            GetPointAt(level, 6, 6).Color = Brushes.Gray;

  
            GetPointAt(level, 4, 8).Color = Brushes.Cyan;
            GetPointAt(level, 8, 4).Color = Brushes.Cyan;


            GetPointAt(level, 2, 2).Color = Brushes.Pink;
            GetPointAt(level, 9, 9).Color = Brushes.Pink;

            return level;
        }
    }
} 