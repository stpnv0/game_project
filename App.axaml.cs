using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ConnectDotsGame.Models;
using ConnectDotsGame.Services;
using ConnectDotsGame.ViewModels;
using ConnectDotsGame.Views;
using System;
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

                // Инициализация сервисов
                var gameStorageService = new GameStorageService();
                var gameState = new GameState(gameStorageService);

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

                // Инициализация сервисов
                var modalService = new ModalService(contentArea);
                var navigationService = new NavigationService(contentArea, modalService);
                var pathManager = new PathManager();
                var levelManager = new LevelManager(navigationService, modalService);
                var gameService = new GameService(navigationService, modalService, pathManager, levelManager);

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
