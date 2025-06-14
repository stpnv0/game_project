using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform;
using ConnectDotsGame.Models;
using ConnectDotsGame.ViewModels;
using ConnectDotsGame.Services;

namespace ConnectDotsGame.Services
{
    public class NavigationService : INavigation
    {
        private readonly ContentControl _contentControl;
        private readonly Dictionary<Type, Type> _viewModelViewMappings = new();
        private readonly Stack<object> _navigationStack = new();
        private GameState? _gameState;
        private readonly IModalService _modalService;
        private readonly IGameService? _gameService;
        private readonly IPathManager? _pathManager;

        public NavigationService(ContentControl contentControl, IModalService modalService, GameState gameState, 
            IGameService gameService, IPathManager pathManager)
        {
            _contentControl = contentControl ?? throw new ArgumentNullException(nameof(contentControl));
            _modalService = modalService ?? throw new ArgumentNullException(nameof(modalService));
            _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
            _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
            _pathManager = pathManager ?? throw new ArgumentNullException(nameof(pathManager));
        }

        public void RegisterView<TViewModel, TView>()
        {
            _viewModelViewMappings[typeof(TViewModel)] = typeof(TView);
        }

        public void NavigateTo<TViewModel>(object? parameter = null)
        {
            NavigateToViewModel(typeof(TViewModel), parameter);
        }
        
        private void NavigateToViewModel(Type viewModelType, object? parameter = null)
        {
            if (!_viewModelViewMappings.TryGetValue(viewModelType, out var viewType))
            {
                throw new InvalidOperationException($"Представление для {viewModelType.Name} не зарегистрировано.");
            }

            // Если параметр - это GameState, сохраняем его
            if (parameter is GameState gameState)
            {
                _gameState = gameState;
            }

            // Создаем экземпляр ViewModel
            var viewModel = CreateViewModel(viewModelType, parameter);
            
            // Сохраняем текущее состояние в стек навигации, если оно существует
            if (_contentControl.Content != null)
            {
                _navigationStack.Push(_contentControl.Content);
            }
            
            // Создаем экземпляр View и устанавливаем ViewModel в качестве DataContext
            var view = (Control)Activator.CreateInstance(viewType)!;
            view.DataContext = viewModel;
            
            // Если переходим на экран выбора уровней, обновляем список
            if (viewModel is LevelSelectViewModel levelSelectViewModel)
            {
                levelSelectViewModel.UpdateLevels();
            }
            
            // Устанавливаем View в качестве содержимого ContentControl
            _contentControl.Content = view;
        }

        

        private object CreateViewModel(Type viewModelType, object? parameter)
        {
            if (viewModelType == null)
                throw new ArgumentNullException(nameof(viewModelType));

            // Специальная обработка для GameViewModel
            if (viewModelType == typeof(GameViewModel))
            {
                if (_gameState == null || _gameService == null || _pathManager == null)
                {
                    throw new InvalidOperationException($"Для создания {viewModelType.Name} требуются все необходимые сервисы.");
                }
                return new GameViewModel(this, _gameState, _modalService, _gameService, _pathManager);
            }
                
            // Ищем конструктор, который принимает INavigation
            var constructor = viewModelType.GetConstructors()
                .FirstOrDefault(c => 
                {
                    var parameters = c.GetParameters();
                    return parameters.Length > 0 && 
                          (parameters[0].ParameterType == typeof(INavigation) ||
                           parameters[0].ParameterType == typeof(NavigationService));
                });

            if (constructor != null)
            {
                var parameters = constructor.GetParameters();
                
                // Проверяем количество параметров
                if (parameters.Length == 1)
                {
                    // Конструктор принимает только INavigation
                    return Activator.CreateInstance(viewModelType, this)!;
                }
                else if (parameters.Length == 2 && parameters[1].ParameterType == typeof(GameState))
                {
                    // Проверяем, что есть GameState
                    var gameState = _gameState ?? parameter as GameState;
                    if (gameState == null)
                    {
                        throw new InvalidOperationException($"Для создания {viewModelType.Name} требуется GameState, но он не был предоставлен.");
                    }
                    
                    // Конструктор принимает INavigation и GameState
                    return Activator.CreateInstance(viewModelType, this, gameState)!;
                }
            }

            // Если не нашли подходящий конструктор, пробуем создать по умолчанию
            try
            {
                return Activator.CreateInstance(viewModelType)!;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Не удалось создать экземпляр {viewModelType.Name}: {ex.Message}", ex);
            }
        }
    }
} 

