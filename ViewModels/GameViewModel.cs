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
        private readonly IPathManager _pathManager;
        private string _statusMessage = string.Empty;
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
        
        public List<ModelPoint> CurrentPath => _gameState.CurrentPath;
        public IBrush? CurrentPathColor => _gameState.CurrentPathColor;
        
        public ICommand NextLevelCommand { get; }
        public ICommand ResetLevelCommand { get; }
        public ICommand BackToMenuCommand { get; }
        public ICommand BackToLevelsCommand { get; }
        public ICommand PrevLevelCommand { get; }
        
        public GameViewModel(INavigation navigation, GameState gameState, IModalService modalService, 
            IGameService gameService, IPathManager pathManager)
        {
            _navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
            _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
            _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
            _pathManager = pathManager ?? throw new ArgumentNullException(nameof(pathManager));

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
            _pathManager.StartPath(_gameState, point);
            UpdateGameState();
            OnPropertyChanged(nameof(CurrentPath));
            OnPropertyChanged(nameof(CurrentPathColor));
        }

        // Продолжение пути через точку
        public void ContinuePath(ModelPoint point)
        {
            _pathManager.ContinuePath(_gameState, point);
            UpdateGameState();
            OnPropertyChanged(nameof(CurrentPath));
            OnPropertyChanged(nameof(CurrentPathColor));
        }

        // Отмена текущего пути
        public void CancelPath()
        {
            _pathManager.CancelPath(_gameState);
            UpdateGameState();
            OnPropertyChanged(nameof(CurrentPath));
            OnPropertyChanged(nameof(CurrentPathColor));
        }
        
        // Проверка, завершен ли уровень
        private void CheckLevelCompletion()
        {
            if (CurrentLevel == null)
                return;
                
            bool isCompleted = _gameService.CheckLevelCompletion(_gameState);
            if (isCompleted)
            {
                UpdateGameState();
                OnPropertyChanged(nameof(CurrentLevel));
                OnPropertyChanged(nameof(HasNextLevel));
            }
        }
        
        private void NextLevel()
        {
            _gameService.NextLevel(_gameState);
            UpdateGameState();
            OnPropertyChanged(nameof(CurrentLevel));
            OnPropertyChanged(nameof(LevelName));
        }
        
        private void ResetLevel()
        {
            _gameService.ResetLevel(_gameState);
            UpdateGameState();
            OnPropertyChanged(nameof(CurrentLevel));
        }
        
        private void UpdateGameState()
        {
            HasNextLevel = _gameState.HasNextLevel;
            ((RelayCommand)PrevLevelCommand).RaiseCanExecuteChanged();
            ((RelayCommand)NextLevelCommand).RaiseCanExecuteChanged();
            OnPropertyChanged(nameof(LevelName));
        }
        
        public void EndPath()
        {
            _pathManager.EndPath(_gameState);
            CheckLevelCompletion();
            UpdateGameState();
            OnPropertyChanged(nameof(CurrentPath));
            OnPropertyChanged(nameof(CurrentPathColor));
        }
        
        public void EndPath(ModelPoint? endPoint)
        {
            _pathManager.EndPath(_gameState, endPoint);
            CheckLevelCompletion();
            UpdateGameState();
            OnPropertyChanged(nameof(CurrentPath));
            OnPropertyChanged(nameof(CurrentPathColor));
        }
        
        private void PrevLevel()
        {
            if (_gameState.CurrentLevelIndex > 0)
            {
                _gameState.GoToPreviousLevel();
                UpdateGameState();
                OnPropertyChanged(nameof(CurrentLevel));
                OnPropertyChanged(nameof(LevelName));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}