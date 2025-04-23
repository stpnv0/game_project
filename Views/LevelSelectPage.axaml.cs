using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ConnectDotsGame.Views
{
    public partial class LevelSelectPage : UserControl
    {
        public LevelSelectPage()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
} 