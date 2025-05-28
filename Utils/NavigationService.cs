using System;
using System.Collections.Generic;
using Avalonia.Controls;
using ConnectDotsGame.Models;
using ConnectDotsGame.Navigation;
using ConnectDotsGame.ViewModels;
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
            NavigateToViewModel(typeof(TViewModel), parameter);
        }
        
        private void NavigateToViewModel(Type viewModelType, object? parameter = null)
        {
            if (!_viewModelViewMappings.TryGetValue(viewModelType, out var viewType))
            {
                throw new InvalidOperationException($"Представление для {viewModelType.Name} не зарегистрировано.");
            }

            
            if (parameter is GameState gameState)
            {
                _gameState = gameState;
            }

            
            var viewModel = CreateViewModel(viewModelType, parameter);
            
            
            if (_contentControl.Content != null)
            {
                _navigationStack.Push(_contentControl.Content);
            }
            
            
            var view = (Control)Activator.CreateInstance(viewType)!;
            view.DataContext = viewModel;
            
            
            if (viewModel is LevelSelectViewModel levelSelectViewModel)
            {
                levelSelectViewModel.UpdateLevels();
            }
            
           
            _contentControl.Content = view;
        }

        public void GoBack()
        {
            if (_navigationStack.Count > 0)
            {
                var previousContent = _navigationStack.Pop();
                _contentControl.Content = previousContent;
                
               
                if (previousContent is Control control && 
                    control.DataContext is LevelSelectViewModel levelSelectViewModel)
                {
                    levelSelectViewModel.UpdateLevels();
                }
            }
            else if (_gameState != null)
            {
                NavigateToViewModel(typeof(MainPageViewModel), _gameState);
            }
        }

        private object CreateViewModel(Type viewModelType, object? parameter)
        {
            if (viewModelType == null)
                throw new ArgumentNullException(nameof(viewModelType));
                
            
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
                
                if (parameters.Length == 1)
                {
                    
                    return Activator.CreateInstance(viewModelType, this)!;
                }
                else if (parameters.Length == 2 && parameters[1].ParameterType == typeof(GameState))
                {
                   
                    var gameState = _gameState ?? parameter as GameState;
                    if (gameState == null)
                    {
                        throw new InvalidOperationException($"Для создания {viewModelType.Name} требуется GameState, но он не был предоставлен.");
                    }
                    
                    
                    return Activator.CreateInstance(viewModelType, this, gameState)!;
                }
            }

            
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