using System;
using System.Collections.Generic;
using Avalonia.Controls;
using ConnectDotsGame.Models;
using ConnectDotsGame.Navigation;
using ConnectDotsGame.ViewModels;
using System.Reflection;
using System.Linq;

namespace ConnectDotsGame.Utils
{
    public class NavigationService : INavigation
    {
        private readonly ContentControl _contentControl;
        private readonly Dictionary<Type, Type> _viewModelViewMappings = new();
        private readonly Stack<object> _navigationStack = new();
        private GameState? _gameState;

        public NavigationService(ContentControl contentControl)
        {
            _contentControl = contentControl ?? throw new ArgumentNullException(nameof(contentControl));
        }

        public void RegisterView<TViewModel, TView>()
        {
            _viewModelViewMappings[typeof(TViewModel)] = typeof(TView);
        }

        public void NavigateTo<TViewModel>(object? parameter = null)
        {
            var viewModelType = typeof(TViewModel);
            
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

        public void GoBack()
        {
            if (_navigationStack.Count > 0)
            {
                var previousContent = _navigationStack.Pop();
                _contentControl.Content = previousContent;
                
                // Если возвращаемся к экрану выбора уровней, обновляем список
                if (previousContent is Control control && 
                    control.DataContext is LevelSelectViewModel levelSelectViewModel)
                {
                    levelSelectViewModel.UpdateLevels();
                }
            }
            else if (_gameState != null)
            {
                // Если стек навигации пуст, но у нас есть GameState, 
                // то идем на главную страницу
                var mainViewModel = new MainPageViewModel(this);
                var mainView = (Control)Activator.CreateInstance(_viewModelViewMappings[typeof(MainPageViewModel)])!;
                mainView.DataContext = mainViewModel;
                _contentControl.Content = mainView;
            }
        }

        private object CreateViewModel(Type viewModelType, object? parameter)
        {
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
                    // Конструктор принимает INavigation и GameState
                    return Activator.CreateInstance(viewModelType, this, _gameState ?? parameter)!;
                }
            }

            // Если не нашли подходящий конструктор, пробуем создать по умолчанию
            return Activator.CreateInstance(viewModelType)!;
        }
    }
} 