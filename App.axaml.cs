using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ConnectDotsGame.ViewModels;
using ConnectDotsGame.Views;
using ConnectDotsGame.Services;
using ConnectDotsGame.Models;
using ConnectDotsGame.Levels;

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
                var window = new MainWindow();
                var contentControl = window.FindControl<ContentControl>("ContentArea");
                if (contentControl == null)
                {
                    throw new InvalidOperationException("ContentArea не найден в MainWindow");
                }

                // Создаем базовые сервисы
                var modalService = new ModalService(contentControl);
                var gameStorage = new GameStorageService();
                var pathManager = new PathManager();
                var gameState = new GameState(gameStorage, pathManager);

                // Загружаем уровни
                var levelLoader = new LevelLoader();
                gameState.Levels = levelLoader.LoadLevels();
                gameState.LoadProgress();

                // Создаем сервисы
                var gameService = new GameService(modalService);

                // Создаем NavigationService
                var navigationService = new NavigationService(contentControl, modalService, gameState, gameService, pathManager);

                // Устанавливаем навигацию
                gameService.SetNavigation(navigationService);

                // Регистрация представлений
                navigationService.RegisterView<MainPageViewModel, MainPage>();
                navigationService.RegisterView<LevelSelectViewModel, LevelSelectPage>();
                navigationService.RegisterView<GameViewModel, GamePage>();
                navigationService.RegisterView<AboutPageViewModel, AboutPage>();

                // Задать начальное представление
                navigationService.NavigateTo<MainPageViewModel>();

                desktop.MainWindow = window;
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
