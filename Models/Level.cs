using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;

namespace ConnectDotsGame.Models
{
    /// <summary>
    /// Представляет уровень игры.
    /// </summary>
    public class Level
    {
        /// <summary>
        /// Уникальный идентификатор уровня.
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Название уровня.
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Количество строк в сетке уровня.
        /// </summary>
        public int Rows { get; set; }
        
        /// <summary>
        /// Количество столбцов в сетке уровня.
        /// </summary>
        public int Columns { get; set; }
        
        /// <summary>
        /// Список всех точек на уровне.
        /// </summary>
        public List<Point> Points { get; set; }
        
        /// <summary>
        /// Список всех линий на уровне.
        /// </summary>
        public List<Line> Lines { get; set; }
        
        /// <summary>
        /// Словарь путей, где ключ - идентификатор пути, а значение - список линий пути.
        /// </summary>
        public Dictionary<string, List<Line>> Paths { get; set; } = new Dictionary<string, List<Line>>();
        
        /// <summary>
        /// Путь к изображению фона уровня.
        /// </summary>
        public string ImagePath { get; set; } = string.Empty;
        
        /// <summary>
        /// Указывает, завершен ли уровень.
        /// </summary>
        public bool IsCompleted { get; set; }
        
        /// <summary>
        /// Указывает, был ли уровень когда-либо пройден.
        /// </summary>
        public bool WasEverCompleted { get; set; }
        
        /// <summary>
        /// Создает новый уровень с параметрами по умолчанию.
        /// </summary>
        public Level()
        {
            Points = new List<Point>();
            Lines = new List<Line>();
            IsCompleted = true;
            WasEverCompleted = false;
        }
        
        /// <summary>
        /// Создает копию текущего уровня.
        /// </summary>
        /// <returns>Новый экземпляр уровня с теми же свойствами.</returns>
        public Level Clone()
        {
            return new Level
            {
                Id = this.Id,
                Name = this.Name,
                Rows = this.Rows,
                Columns = this.Columns,
                Points = this.Points?.Select(p => p.Clone()).ToList() ?? new List<Point>(),
                Lines = this.Lines?.Select(l => l.Clone()).ToList() ?? new List<Line>(),
                Paths = this.Paths?.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Select(l => l.Clone()).ToList()
                ) ?? new Dictionary<string, List<Line>>(),
                ImagePath = this.ImagePath,
                IsCompleted = this.IsCompleted
            };
        }
        
        /// <summary>
        /// Проверяет, завершен ли уровень.
        /// </summary>
        /// <returns>True, если все цветные точки соединены; иначе false.</returns>
        public bool CheckCompletion()
        {
            return Points.Where(p => p.HasColor).All(p => p.IsConnected);
        }
        
        /// <summary>
        /// Получает точку по её позиции в сетке.
        /// </summary>
        /// <param name="row">Номер строки.</param>
        /// <param name="column">Номер столбца.</param>
        /// <returns>Точка с указанными координатами или null, если точка не найдена.</returns>
        public Point? GetPointByPosition(int row, int column)
        {
            return Points.FirstOrDefault(p => p.Row == row && p.Column == column);
        }
        
        /// <summary>
        /// Проверяет, завершен ли путь указанного цвета.
        /// </summary>
        /// <param name="color">Цвет пути.</param>
        /// <returns>True, если путь завершен; иначе false.</returns>
        public bool IsPathComplete(IBrush? color)
        {
            if (color == null)
                return false;
                
            string pathId = $"{color}-path";
            if (!Paths.ContainsKey(pathId))
                return false;
                
            var colorPoints = Points.Where(p => p.Color != null && p.Color.Equals(color)).ToList();
            
            if (colorPoints.Count < 2)
                return false;
                
            return colorPoints.All(p => p.IsConnected);
        }
        
        /// <summary>
        /// Добавляет линию в словарь путей.
        /// </summary>
        /// <param name="line">Линия для добавления.</param>
        public void AddLineToPaths(Line line)
        {
            if (!Paths.ContainsKey(line.PathId))
            {
                Paths[line.PathId] = new List<Line>();
            }
            
            Paths[line.PathId].Add(line);
        }
        
        /// <summary>
        /// Добавляет последнюю линию с указанным идентификатором пути в словарь путей.
        /// </summary>
        /// <param name="pathId">Идентификатор пути.</param>
        public void AddLineToPaths(string pathId)
        {
            if (!Paths.ContainsKey(pathId))
            {
                Paths[pathId] = new List<Line>();
            }
            
            // Находим последнюю добавленную линию с этим pathId
            var line = Lines.LastOrDefault(l => l.PathId == pathId);
            if (line != null)
            {
                Paths[pathId].Add(line);
            }
        }
        
        /// <summary>
        /// Очищает путь с указанным идентификатором.
        /// </summary>
        /// <param name="pathId">Идентификатор пути для очистки.</param>
        public void ClearPath(string pathId)
        {
            if (Paths.ContainsKey(pathId))
            {
                // Получаем все линии из пути перед удалением
                var linesToRemove = Paths[pathId].ToList();
                
                // Удаляем линии из общего списка линий уровня
                foreach (var line in linesToRemove)
                {
                    line.IsVisible = false;
                    Lines.Remove(line);
                }
                
                // Удаляем путь из словаря
                Paths.Remove(pathId);
                
                // Сброс статуса соединения для точек этого цвета
                var color = pathId.Replace("-path", "");
                var colorPoints = Points.Where(p => p.HasColor && p.Color != null && 
                                                p.Color.ToString() == color).ToList();
                foreach (var point in colorPoints)
                {
                    point.IsConnected = false;
                }
            }
        }
    }
}