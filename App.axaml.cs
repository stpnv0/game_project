using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ConnectDotsGame.Models;
using ConnectDotsGame.Services;
using ConnectDotsGame.ViewModels;
using ConnectDotsGame.Views;
using System;
using System.Collections.Generic;
using Avalonia.Controls;

namespace ConnectDotsGame
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainWindow = new MainWindow();
                var gameState = new GameState();

                // Загружаем уровни
                var levelLoader = new Levels.LevelLoader();
                gameState.Levels = levelLoader.LoadLevels();

                // Загружаем сохраненные данные
                gameState.LoadProgress();

                // Находим ContentArea
                var contentArea = mainWindow.FindControl<ContentControl>("ContentArea");
                if (contentArea == null)
                {
                    throw new InvalidOperationException("ContentArea не найден в MainWindow");
                }

                // Инициализация навигационной службы
                var navigationService = new NavigationService(contentArea);

                // Создание GameService с передачей navigationService
                var gameService = new GameService(navigationService);

                // Регистрация всех страниц
                navigationService.RegisterView<MainPageViewModel, MainPage>();
                navigationService.RegisterView<LevelSelectViewModel, LevelSelectPage>();
                navigationService.RegisterView<GameViewModel, GamePage>();
                navigationService.RegisterView<AboutPageViewModel, AboutPage>();

                // Задать начальное представление
                navigationService.NavigateTo<MainPageViewModel>(gameState);

                desktop.MainWindow = mainWindow;
                mainWindow.Show();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
