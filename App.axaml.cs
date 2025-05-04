using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ConnectDotsGame.Models;
using ConnectDotsGame.Utils;
using ConnectDotsGame.ViewModels;
using ConnectDotsGame.Views;
using System;
using System.Collections.Generic;
using Avalonia.Controls;

namespace ConnectDotsGame;

public partial class App : Application
{
    private NavigationService? _navigationService;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        // Запуск фоновой музыки
        AudioService.Instance.PlayBackgroundMusic("Resources/Audio/background_music.wav");
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
            
            // Теперь находим ContentArea
            var contentArea = mainWindow.FindControl<ContentControl>("ContentArea");
            if (contentArea == null)
            {
                throw new InvalidOperationException("ContentArea не найден в MainWindow");
            }
            
            // Инициализация навигационной службы
            _navigationService = new NavigationService(contentArea);
            
            // Зарегистрировать все страницы
            _navigationService.RegisterView<MainPageViewModel, MainPage>();
            _navigationService.RegisterView<LevelSelectViewModel, LevelSelectPage>();
            _navigationService.RegisterView<GameViewModel, GamePage>();
            
            // Задать начальное представление
            _navigationService.NavigateTo<MainPageViewModel>(gameState);
            
            desktop.MainWindow = mainWindow;

            // Подписываемся на событие завершения приложения
            desktop.Exit += OnExit;

            // Необходимо сначала показать окно, иначе элементы управления могут быть не инициализированы
            mainWindow.Show();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        // Останавливаем музыку перед выходом
        AudioService.Instance.StopBackgroundMusic();
    }
}