using Avalonia.Controls;
using ConnectDotsGame.Utils;
using System;

namespace ConnectDotsGame
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            AudioService.Instance.PlayBackgroundMusic("Resources/Audio/background_music.wav");
        }

        protected override void OnClosed(EventArgs e)
        {
            AudioService.Instance.Dispose();
            base.OnClosed(e);
        }
    }
}