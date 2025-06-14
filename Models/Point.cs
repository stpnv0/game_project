using Avalonia;
using Avalonia.Media;

namespace ConnectDotsGame.Models
{
    public class Point
    {
        public int Id { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public bool IsConnected { get; set; }
        public IBrush Color { get; set; } = Brushes.Transparent;
        public bool HasColor => Color != Brushes.Transparent;

        public Point(int id, int row, int column, IBrush color)
        {
            Id = id;
            Row = row;
            Column = column;
            Color = color;
            IsConnected = false;
        }

        public Point(int id, int row, int column)
        {
            Id = id;
            Row = row;
            Column = column;
            IsConnected = false;
        }

        public Point() { }

        public Point Clone() => new()
        {
            Id = Id,
            Row = Row,
            Column = Column,
            X = X,
            Y = Y,
            IsConnected = IsConnected,
            Color = Color
        };
    }
}