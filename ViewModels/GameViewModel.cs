using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia.Media;
using ConnectDotsGame.Models;
using ConnectDotsGame.Services;

namespace ConnectDotsGame.ViewModels
{
    public class GameViewModel : INotifyPropertyChanged
    {
        private readonly GameState _gameState;
        private readonly INavigation _navigation;
        private readonly IGameService _gameService;
        private readonly IPathManager _pathManager;
        
        public string LevelName => _gameState.CurrentLevel?.Name ?? "Нет уровня";
        public Level? CurrentLevel => _gameState.CurrentLevel;
        public List<Point> CurrentPath => _gameState.CurrentPath;
        public IBrush? CurrentPathColor => _gameState.CurrentPathColor;
        
        public ICommand NextLevelCommand { get; }
        public ICommand ResetLevelCommand { get; }
        public ICommand BackToMenuCommand { get; }
        public ICommand PrevLevelCommand { get; }
        
        public GameViewModel(INavigation navigation, GameState gameState, IModalService modalService, 
            IGameService gameService, IPathManager pathManager)
        {
            _navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
            _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
            _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
            _pathManager = pathManager ?? throw new ArgumentNullException(nameof(pathManager));

            _gameState.LoadProgress();

            NextLevelCommand = new RelayCommand(NextLevel, () => CurrentLevel?.WasEverCompleted == true && _gameState.HasNextLevel);
            ResetLevelCommand = new RelayCommand(ResetLevel, () => CurrentLevel != null);
            BackToMenuCommand = new RelayCommand(NavigateToMenu);
            PrevLevelCommand = new RelayCommand(PrevLevel, () => _gameState.CurrentLevelIndex > 0);

            UpdateGameState();
        }

        private void NavigateToMenu()
        {
            _gameState.ResetAllPathsAndLines();
            _navigation.NavigateTo<LevelSelectViewModel>();
        }
        
        public void StartPath(Point point)
        {
            _pathManager.StartPath(_gameState, point);
            UpdatePathState();
        }

        public void ContinuePath(Point point)
        {
            _pathManager.ContinuePath(_gameState, point);
            UpdatePathState();
        }

        public void CancelPath()
        {
            _pathManager.CancelPath(_gameState);
            UpdatePathState();
        }
        
        public void EndPath(Point? endPoint = null)
        {
            _pathManager.EndPath(_gameState, endPoint);
            CheckLevelCompletion();
            UpdatePathState();
        }
        
        private void CheckLevelCompletion()
        {
            if (CurrentLevel == null) return;
                
            if (_gameService.CheckLevelCompletion(_gameState))
            {
                UpdateGameState();
                OnPropertyChanged(nameof(CurrentLevel));
            }
        }
        
        private void NextLevel()
        {
            _gameService.NextLevel(_gameState);
            UpdateLevelState();
        }
        
        private void PrevLevel()
        {
            if (_gameState.CurrentLevelIndex > 0)
            {
                _gameState.GoToPreviousLevel();
                UpdateLevelState();
            }
        }
        
        private void ResetLevel()
        {
            _gameService.ResetLevel(_gameState);
            UpdateLevelState();
        }
        
        private void UpdateGameState()
        {
            ((RelayCommand)PrevLevelCommand).RaiseCanExecuteChanged();
            ((RelayCommand)NextLevelCommand).RaiseCanExecuteChanged();
            OnPropertyChanged(nameof(LevelName));
        }

        private void UpdateLevelState()
        {
            UpdateGameState();
            OnPropertyChanged(nameof(CurrentLevel));
            OnPropertyChanged(nameof(LevelName));
        }

        private void UpdatePathState()
        {
            UpdateGameState();
            OnPropertyChanged(nameof(CurrentPath));
            OnPropertyChanged(nameof(CurrentPathColor));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}