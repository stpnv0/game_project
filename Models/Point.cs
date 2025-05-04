using Avalonia;
using Avalonia.Media;

namespace ConnectDotsGame.Models
{
    /// <summary>
    /// Представляет точку на игровом поле.
    /// </summary>
    public class Point
    {
        /// <summary>
        /// Уникальный идентификатор точки.
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Номер строки (координата Y в сетке).
        /// </summary>
        public int Row { get; set; }
        
        /// <summary>
        /// Номер столбца (координата X в сетке).
        /// </summary>
        public int Column { get; set; }
        
        /// <summary>
        /// Координата X на канвасе.
        /// </summary>
        public double X { get; set; }
        
        /// <summary>
        /// Координата Y на канвасе.
        /// </summary>
        public double Y { get; set; }
        
        /// <summary>
        /// Указывает, соединена ли точка с другой точкой.
        /// </summary>
        public bool IsConnected { get; set; }
        
        /// <summary>
        /// Цвет точки.
        /// </summary>
        public IBrush Color { get; set; } = Brushes.Transparent;
        
        /// <summary>
        /// Определяет, имеет ли точка цвет (не прозрачная).
        /// </summary>
        public bool HasColor => Color != Brushes.Transparent;
        
        /// <summary>
        /// Создает новую точку с указанными параметрами и цветом.
        /// </summary>
        /// <param name="id">Идентификатор точки.</param>
        /// <param name="row">Номер строки.</param>
        /// <param name="column">Номер столбца.</param>
        /// <param name="color">Цвет точки.</param>
        public Point(int id, int row, int column, IBrush color)
        {
            Id = id;
            Row = row;
            Column = column;
            Color = color;
            IsConnected = false;
        }
        
        /// <summary>
        /// Создает новую точку с указанными параметрами без цвета.
        /// </summary>
        /// <param name="id">Идентификатор точки.</param>
        /// <param name="row">Номер строки.</param>
        /// <param name="column">Номер столбца.</param>
        public Point(int id, int row, int column)
        {
            Id = id;
            Row = row;
            Column = column;
            IsConnected = false;
        }
        
        /// <summary>
        /// Создает новую точку с параметрами по умолчанию.
        /// </summary>
        public Point() { }
        
        /// <summary>
        /// Создает копию текущей точки.
        /// </summary>
        /// <returns>Новый экземпляр точки с теми же свойствами.</returns>
        public Point Clone()
        {
            return new Point
            {
                Id = this.Id,
                Row = this.Row,
                Column = this.Column,
                X = this.X,
                Y = this.Y,
                IsConnected = this.IsConnected,
                Color = this.Color
            };
        }
    }
}