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
using ConnectDotsGame.Services;

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

                // Инициализация сервисов
                var modalService = new ModalService(contentArea);
                var navigationService = new NavigationService(contentArea, modalService);
                var gameService = new GameService(navigationService, modalService);

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
