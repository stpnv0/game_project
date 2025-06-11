using System;
using System.Collections.Generic;
using Avalonia.Controls;
using ConnectDotsGame.Models;
using ConnectDotsGame.ViewModels;
using System.Reflection;
using System.Linq;
using ConnectDotsGame.Services;
using Avalonia.Controls; 
using Avalonia.Layout;  
using Avalonia;
using Avalonia.Media;
using Avalonia.Controls;
using Avalonia.Platform;

namespace ConnectDotsGame.Services
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
        
        public void ShowModal(string title, string message, string buttonText, Action onButtonClick)
        {
            var modalWindow = new Window
            {
                Title = title,
                Width = 395,
                Height = 175,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                SystemDecorations = SystemDecorations.None, // Убираем рамку Windows
                Background = new SolidColorBrush(Colors.Black),
                CornerRadius = new CornerRadius(150),
                Content = new StackPanel
                {
                    Spacing = 20,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = message,
                            FontSize = 20,
                            FontWeight = FontWeight.Bold,
                            Foreground = Brushes.White,
                            TextAlignment = TextAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center
                        },
                        new Button
                        {
                            Content = buttonText,
                            Width = 197,
                            Height = 35,
                            FontSize = 17,
                            FontWeight = FontWeight.Bold,
                            Margin = new Thickness(0, 10, 0, 10),
                            Background = new SolidColorBrush(Color.Parse("#E106D9")),
                            Foreground = Brushes.White,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            CornerRadius = new CornerRadius(10)
                        }
                    }
                }
            };

            var button = ((StackPanel)modalWindow.Content).Children.OfType<Button>().First();
            button.Click += (_, _) =>
            {
                modalWindow.Close();
                onButtonClick?.Invoke();
            };

            modalWindow.ShowDialog((Window)_contentControl.Parent);
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
                NavigateToViewModel(typeof(MainPageViewModel), _gameState);
            }
        }

        private object CreateViewModel(Type viewModelType, object? parameter)
        {
            if (viewModelType == null)
                throw new ArgumentNullException(nameof(viewModelType));
                
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

