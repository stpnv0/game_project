using Avalonia;
using Avalonia.Media;

namespace ConnectDotsGame.Models
{
    /// <summary>
    /// Представляет линию, соединяющую две точки в игре.
    /// </summary>
    public class Line
    {
        /// <summary>
        /// Начальная точка линии.
        /// </summary>
        public Point StartPoint { get; set; } = null!;
        
        /// <summary>
        /// Конечная точка линии.
        /// </summary>
        public Point EndPoint { get; set; } = null!;
        
        /// <summary>
        /// Определяет, видима ли линия.
        /// </summary>
        public bool IsVisible { get; set; }
        
        /// <summary>
        /// Цвет линии.
        /// </summary>
        public IBrush Color { get; set; } = Brushes.Black;
        
        /// <summary>
        /// Идентификатор пути, к которому принадлежит линия.
        /// </summary>
        public string PathId { get; set; } = string.Empty;
        
        /// <summary>
        /// Создает новую линию с указанными параметрами.
        /// </summary>
        /// <param name="startPoint">Начальная точка.</param>
        /// <param name="endPoint">Конечная точка.</param>
        /// <param name="color">Цвет линии.</param>
        /// <param name="pathId">Идентификатор пути.</param>
        public Line(Point startPoint, Point endPoint, IBrush color, string pathId)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            IsVisible = false;
            Color = color;
            PathId = pathId;
        }
        
        /// <summary>
        /// Создает новую линию с параметрами по умолчанию.
        /// </summary>
        public Line()
        {
            // Значения по умолчанию устанавливаются в объявлении свойств
        }
        
        /// <summary>
        /// Создает копию текущей линии.
        /// </summary>
        /// <returns>Новый экземпляр линии с теми же свойствами.</returns>
        public Line Clone()
        {
            return new Line
            {
                StartPoint = this.StartPoint?.Clone(),
                EndPoint = this.EndPoint?.Clone(),
                IsVisible = this.IsVisible,
                Color = this.Color,
                PathId = this.PathId
            };
        }
    }
}