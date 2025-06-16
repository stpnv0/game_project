using System;
using System.Collections.Generic;
using Avalonia.Controls;
using ConnectDotsGame.Models;
using ConnectDotsGame.ViewModels;

namespace ConnectDotsGame.Services
{
    // Сервис для навигации между экранами
    public class NavigationService : INavigation
    {
        private readonly ContentControl _contentControl;
        private readonly Dictionary<Type, Type> _viewMappings = new();
        private readonly GameState _gameState;
        private readonly IModalService _modalService;
        private readonly IGameService _gameService;
        private readonly IPathService _pathService;

        public NavigationService(ContentControl contentControl, IModalService modalService, 
            GameState gameState, IGameService gameService, IPathService pathService)
        {
            _contentControl = contentControl;
            _modalService = modalService;
            _gameState = gameState;
            _gameService = gameService;
            _pathService = pathService;
        }

        // Регистрация между ViewModel и View
        public void RegisterView<TViewModel, TView>() => 
            _viewMappings[typeof(TViewModel)] = typeof(TView);

        // Навигация к экрану
        public void NavigateTo<TViewModel>(object? parameter = null)
        {
            var viewType = _viewMappings[typeof(TViewModel)];
            
            var view = (Control)Activator.CreateInstance(viewType)!;
            
            // Создаем соответствующую ViewModel 
            view.DataContext = typeof(TViewModel).Name switch
            {
                nameof(GameViewModel) => 
                    new GameViewModel(this, _gameState, _modalService, _gameService, _pathService),
                nameof(LevelSelectViewModel) => 
                    CreateLevelSelectViewModel(),
                _ => 
                    Activator.CreateInstance(typeof(TViewModel), this)!
            };
            
            // Отображаем экран
            _contentControl.Content = view;
        }

        // Создание ViewModel для LevelSelectPage
        private LevelSelectViewModel CreateLevelSelectViewModel()
        {
            var vm = new LevelSelectViewModel(this, _gameState);
            vm.UpdateLevels();
            return vm;
        }
    }
} 