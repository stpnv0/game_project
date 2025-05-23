using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ConnectDotsGame.ViewModels;
using System;
using Avalonia.Threading;

namespace ConnectDotsGame.Views
{
    public partial class GamePage : UserControl
    {
        private GameCanvas? _gameCanvas;
        
        public GamePage()
        {
            InitializeComponent();
            
            // Находим GameCanvas и сохраняем ссылку
            _gameCanvas = this.FindControl<GameCanvas>("GameCanvas");
            
            // Подписываемся на изменение DataContext
            DataContextChanged += GamePage_DataContextChanged;
        }
        
        private void GamePage_DataContextChanged(object? sender, EventArgs e)
        {
            if (_gameCanvas != null && DataContext != null)
            {
                // Приводим GameViewModel к MainViewModel для GameCanvas
                if (DataContext is GameViewModel gameVM)
                {
                    Dispatcher.UIThread.Post(() => {
                        _gameCanvas.DataContext = DataContext;
                    }, DispatcherPriority.Normal);
                }
            }
        }
        
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
} 