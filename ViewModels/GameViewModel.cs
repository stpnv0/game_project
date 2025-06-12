using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia.Media;
using ConnectDotsGame.Models;
using ConnectDotsGame.Services;
using ConnectDotsGame.Utils;
using ModelPoint = ConnectDotsGame.Models.Point;

namespace ConnectDotsGame.ViewModels
{
    public class GameViewModel : INotifyPropertyChanged
    {
        private readonly GameState _gameState;
        private readonly INavigation _navigation;
        private readonly IGameService _gameService;
        private string _statusMessage = string.Empty;
        private bool _isLevelCompleted;
        private bool _hasNextLevel;
        private bool _isDrawingPath;
        private readonly Dictionary<string, List<ModelPoint>> _currentPaths = new Dictionary<string, List<ModelPoint>>();
        
        public string LevelName => _gameState.CurrentLevel?.Name ?? "Нет уровня";
        
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public bool IsLevelCompleted
        {
            get => _isLevelCompleted;
            set
            {
                if (_isLevelCompleted != value)
                {
                    _isLevelCompleted = value;
                    OnPropertyChanged();
                    ((RelayCommand)NextLevelCommand).RaiseCanExecuteChanged();
                }
            }
        }
        
        public bool HasNextLevel
        {
            get => _hasNextLevel;
            set
            {
                if (_hasNextLevel != value)
                {
                    _hasNextLevel = value;
                    OnPropertyChanged();
                    ((RelayCommand)NextLevelCommand).RaiseCanExecuteChanged();
                }
            }
        }
        
        public bool IsDrawingPath
        {
            get => _isDrawingPath;
            set
            {
                if (_isDrawingPath != value)
                {
                    _isDrawingPath = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public Level? CurrentLevel => _gameState.CurrentLevel;
        
        public List<ModelPoint> CurrentPath 
        { 
            get 
            {
                return new List<ModelPoint>();
            }
        }
        
        public IBrush? CurrentPathColor => null;
        
        public ICommand NextLevelCommand { get; }
        public ICommand ResetLevelCommand { get; }
        public ICommand BackToMenuCommand { get; }
        public ICommand BackToLevelsCommand { get; }
        public ICommand PrevLevelCommand { get; }
        
        public GameViewModel(INavigation navigation, GameState gameState, IModalService modalService)
        {
            _navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
            _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
            _gameService = new GameService(navigation, modalService);

            _gameState.LoadProgress();

            NextLevelCommand = new RelayCommand(NextLevel, () => CurrentLevel?.WasEverCompleted == true && HasNextLevel);
            ResetLevelCommand = new RelayCommand(ResetLevel, () => _gameState.CurrentLevel != null);
            BackToMenuCommand = new RelayCommand(() => {
                _gameState.ResetAllPathsAndLines();
                _navigation.NavigateTo<LevelSelectViewModel>();
            });
            BackToLevelsCommand = new RelayCommand(() => {
                _gameState.ResetAllPathsAndLines();
                _navigation.NavigateTo<LevelSelectViewModel>();
            });
            PrevLevelCommand = new RelayCommand(PrevLevel, () => _gameState.CurrentLevelIndex > 0);

            HasNextLevel = _gameState.HasNextLevel;

            UpdateGameState();
            ((RelayCommand)NextLevelCommand).RaiseCanExecuteChanged();
        }
        
        // Начало пути от точки
        public void StartPath(ModelPoint point)
        {
            _gameService.StartPath(_gameState, point);
            UpdateGameState();
            OnPropertyChanged(nameof(CurrentPath));
            OnPropertyChanged(nameof(CurrentPathColor));
        }

        // Продолжение пути через точку
        public void ContinuePath(ModelPoint point)
        {
            _gameService.ContinuePath(_gameState, point);
            UpdateGameState();
            OnPropertyChanged(nameof(CurrentPath));
            OnPropertyChanged(nameof(CurrentPathColor));
        }

        // Отмена текущего пути
        public void CancelPath()
        {
            _gameService.CancelPath(_gameState);
            UpdateGameState();
            OnPropertyChanged(nameof(CurrentPath));
            OnPropertyChanged(nameof(CurrentPathColor));
        }
        
        // Проверка, завершен ли уровень
        private void CheckLevelCompletion()
        {
            if (CurrentLevel == null)
                return;
                
            // Проверяем все цветные точки на уровне
            var colorGroups = CurrentLevel.Points
                .Where(p => p.HasColor && p.Color != null)
                .GroupBy(p => p.Color.ToString())
                .ToList();
            
            bool allCompleted = true;
            
            foreach (var group in colorGroups)
            {
                string colorKey = group.Key ?? "";
                
                // Проверяем, есть ли путь для этого цвета
                if (!CurrentLevel.Paths.ContainsKey($"{colorKey}-path"))
                {
                    allCompleted = false;
                    break;
                }
                
                // И все ли точки этого цвета соединены
                if (!group.All(p => p.IsConnected))
                {
                    allCompleted = false;
                    break;
                }
            }
            
            // Если все пути завершены, отмечаем уровень как завершенный
            CurrentLevel.IsCompleted = allCompleted;
            IsLevelCompleted = allCompleted;
            
            // Если уровень завершен, сохраняем состояние игры
            if (allCompleted)
            {
                CurrentLevel.WasEverCompleted = true;
                _gameState.SaveProgress();
            }
            
            // Обновляем статус игры
            UpdateGameState();
        }
        
        private void NextLevel()
        {
            if (_gameState.HasNextLevel)
            {
                _gameService.NextLevel(_gameState);
                _gameState.ResetCurrentLevel();
                IsLevelCompleted = true;
                UpdateGameState();
                OnPropertyChanged(nameof(CurrentPath));
                OnPropertyChanged(nameof(CurrentPathColor));
            }
        }
        
        private void ResetLevel()
        {
            _gameService.ResetLevel(_gameState);
            IsLevelCompleted = false;
            UpdateGameState();
            OnPropertyChanged(nameof(CurrentPath));
            OnPropertyChanged(nameof(CurrentPathColor));
        }
        
        private void UpdateGameState()
        {
            HasNextLevel = _gameState.HasNextLevel;
            
            OnPropertyChanged(nameof(LevelName));
            OnPropertyChanged(nameof(CurrentLevel));
            ((RelayCommand)NextLevelCommand).RaiseCanExecuteChanged();
        }
        
        // Завершение пути
        public void EndPath()
        {
            EndPath(null);
        }
        
        // Завершение пути на указанной точке
        public void EndPath(ModelPoint? endPoint)
        {
            _gameService.EndPath(_gameState, endPoint);
            UpdateGameState();
            OnPropertyChanged(nameof(CurrentPath));
            OnPropertyChanged(nameof(CurrentPathColor));
        }
        
        private void PrevLevel()
        {
            if (_gameState.CurrentLevelIndex > 0)
            {
                _gameState.CurrentLevelIndex--;
                _gameState.ResetCurrentLevel();
                UpdateGameState();
                OnPropertyChanged(nameof(CurrentPath));
                OnPropertyChanged(nameof(CurrentPathColor));
            }
        }
        
        #region INotifyPropertyChanged
        
        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        #endregion
    }
}