using Avalonia.Media;

namespace ConnectDotsGame.Models
{
    // Представляет точку на игровом поле с координатами, цветом и состоянием соединения
    public class Point
    {
        public int Id { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public bool IsConnected { get; set; }
        public IBrush Color { get; set; } = Brushes.Transparent;
        public bool HasColor => Color != Brushes.Transparent;

        public Point(int id, int row, int column, IBrush? color = null)
        {
            Id = id;
            Row = row;
            Column = column;
            Color = color ?? Brushes.Transparent;
        }
    }
}