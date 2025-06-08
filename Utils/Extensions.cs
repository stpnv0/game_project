using Avalonia.Media;

namespace ConnectDotsGame.Utils
{
    public static class BrushExtensions
    {
        public static bool AreBrushesEqual(this IBrush? brush1, IBrush? brush2)
        {
            if (ReferenceEquals(brush1, brush2)) return true;
            if (brush1 == null || brush2 == null) return false;
            if (brush1 is SolidColorBrush solid1 && brush2 is SolidColorBrush solid2)
                return solid1.Color == solid2.Color;
            return brush1.Equals(brush2);
        }
    }
} 