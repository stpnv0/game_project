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
                Console.WriteLine($"GamePage: Updating GameCanvas DataContext to {DataContext.GetType().Name}");
                
                // Приводим GameViewModel к MainViewModel для GameCanvas
                if (DataContext is GameViewModel gameVM)
                {
                    Console.WriteLine("Explicitly setting DataContext on GameCanvas");
                    Dispatcher.UIThread.Post(() => {
                        _gameCanvas.DataContext = DataContext;
                        Console.WriteLine("GamePage: Successfully passed GameViewModel to GameCanvas");
                    }, DispatcherPriority.Normal);
                }
            }
            else
            {
                Console.WriteLine($"GamePage: DataContext changed to {DataContext?.GetType().Name ?? "null"}, but _gameCanvas is {(_gameCanvas == null ? "null" : "not null")}");
            }
        }
        
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
} 