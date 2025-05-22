using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using ConnectDotsGame.Levels;
using ConnectDotsGame.Models;
using ConnectDotsGame.Utils;
using ConnectDotsGame.ViewModels;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ModelPoint = ConnectDotsGame.Models.Point;

namespace ConnectDotsGame.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly GameState _gameState;
        private string _statusMessage = string.Empty;
        private bool _isLevelCompleted;
        private bool _hasNextLevel;
        private bool _isDrawingPath;
        
        public string LevelName => _gameState.CurrentLevel?.Name ?? "Нет уровня";
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
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
        
        public List<ModelPoint> CurrentPath => _gameState.CurrentPath;
        public IBrush? CurrentPathColor => _gameState.CurrentPathColor;
        public ModelPoint? LastSelectedPoint => _gameState.LastSelectedPoint;
        
        public ICommand NextLevelCommand { get; }
        public ICommand ResetLevelCommand { get; }
        
        public MainViewModel()
        {
            _gameState = new GameState();
            
            // Загружаем уровни
            var levelLoader = new LevelLoader();
            _gameState.Levels = levelLoader.LoadLevels();
            
            NextLevelCommand = new RelayCommand(NextLevel, () => IsLevelCompleted && HasNextLevel);
            ResetLevelCommand = new RelayCommand(ResetLevel, () => _gameState.CurrentLevel != null);
            
            StatusMessage = "Соедините точки одинаковых цветов!";
            UpdateGameState();
        }
        
        public void StartPath(ModelPoint startPoint)
        {
            if (_gameState.CurrentLevel == null || IsLevelCompleted)
                return;
                
            if (startPoint.HasColor)
            {
                _gameState.StartNewPath(startPoint);
                IsDrawingPath = true;
                UpdateGameState();
            }
        }
        
        public void ContinuePath(ModelPoint point)
{
    if (_gameState.CurrentLevel == null || !IsDrawingPath || 
        _gameState.LastSelectedPoint == null || _gameState.CurrentPathColor == null)
    {
        Console.WriteLine("ContinuePath: Cannot continue path. Missing required game state data.");
        return;
    }

    Console.WriteLine($"ContinuePath: Attempting to add point Row={point.Row}, Column={point.Column}.");
    Console.WriteLine($"ContinuePath: LastSelectedPoint at Row={_gameState.LastSelectedPoint.Row}, Column={_gameState.LastSelectedPoint.Column}.");

    int rowDiff = Math.Abs(point.Row - _gameState.LastSelectedPoint.Row);
    int colDiff = Math.Abs(point.Column - _gameState.LastSelectedPoint.Column);

    Console.WriteLine($"ContinuePath: Calculated rowDiff={rowDiff}, colDiff={colDiff}.");

    // Блокируем диагональные движения и движения на большое расстояние
    if ((rowDiff == 1 && colDiff == 1) || rowDiff > 1 || colDiff > 1)
    {
        Console.WriteLine($"ContinuePath: Invalid movement to Row={point.Row}, Column={point.Column}. Stopping execution.");
        return;
    }

    if (_gameState.IsPointInCurrentPath(point))
    {
        if (_gameState.CurrentPath.Count >= 2 &&
            _gameState.CurrentPath[_gameState.CurrentPath.Count - 2].Equals(point))
        {
            Console.WriteLine("ContinuePath: Moving back to previous point. Removing last point from path.");
            _gameState.RemoveLastPointFromPath();
            UpdateGameState();
        }
        else
        {
            Console.WriteLine("ContinuePath: Point already exists in current path. Ignoring.");
        }
        return;
    }

    if (GameLogic.CanConnectPoints(_gameState.CurrentLevel, _gameState.LastSelectedPoint, point))
    {
        Console.WriteLine($"ContinuePath: Adding point Row={point.Row}, Column={point.Column} to path.");
        _gameState.CurrentPath.Add(point);
        _gameState.LastSelectedPoint = point;
        UpdateGameState();
    }
    else
    {
        Console.WriteLine($"ContinuePath: Cannot connect to Row={point.Row}, Column={point.Column}. Ignoring.");
    }

    // Если нажали на точку того же цвета - завершаем путь
    if (point.HasColor && point.Color == _gameState.CurrentPathColor)
    {
        if (GameLogic.CanConnectPoints(_gameState.CurrentLevel, _gameState.LastSelectedPoint, point))
        {
            Console.WriteLine($"ContinuePath: Завершаем путь на точке {point.Row},{point.Column}.");
            EndPath(point);
        }
        return;
    }

    // Если это пустая точка, проверяем, можно ли добавить к пути
    if (!point.HasColor)
    {
        bool sameRow = _gameState.LastSelectedPoint.Row == point.Row;
        bool sameColumn = _gameState.LastSelectedPoint.Column == point.Column;

        if ((rowDiff == 0 && colDiff == 1) || (colDiff == 0 && rowDiff == 1))
        {
            var crossingPath = GameLogic.CheckForCrossingPath(_gameState.CurrentLevel, point);
            if (crossingPath != null)
            {
                Console.WriteLine($"ContinuePath: Пересечение пути найдено, очищаем пересеченный путь.");
                _gameState.CurrentLevel.ClearPath(crossingPath);
            }

            Console.WriteLine($"ContinuePath: Добавляем точку {point.Row},{point.Column} к пути.");
            _gameState.LastSelectedPoint = point;
            _gameState.CurrentPath.Add(point);
            UpdateGameState();
        }
    }
}
        
        public void EndPath(ModelPoint point)
        {
            if (_gameState.CurrentLevel == null || _gameState.LastSelectedPoint == null || _gameState.CurrentPathColor == null)
            {
                Console.WriteLine("EndPath: Cannot end path. Missing required game state data.");
                return;
            }

            Console.WriteLine($"EndPath: Attempting to end path at Row={point.Row}, Column={point.Column}.");
            Console.WriteLine($"EndPath: LastSelectedPoint at Row={_gameState.LastSelectedPoint.Row}, Column={_gameState.LastSelectedPoint.Column}.");

            int rowDiff = Math.Abs(point.Row - _gameState.LastSelectedPoint.Row);
            int colDiff = Math.Abs(point.Column - _gameState.LastSelectedPoint.Column);

            if ((rowDiff == 1 && colDiff == 1) || rowDiff > 1 || colDiff > 1)
            {
                Console.WriteLine($"EndPath: Invalid movement to Row={point.Row}, Column={point.Column}. Stopping execution.");
                return;
            }

            if (point.HasColor && point.Color == _gameState.CurrentPathColor)
            {
                Console.WriteLine($"EndPath: Successfully ended path at point Row={point.Row}, Column={point.Column}.");
                _gameState.CurrentPath.Add(point);
                _gameState.LastSelectedPoint = null;
                UpdateGameState();
            }
            else
            {
                Console.WriteLine($"EndPath: Point Row={point.Row}, Column={point.Column} is not valid for ending. Ignoring.");
            }
        }
        
        private void CompletePath(ModelPoint endPoint)
        {
            if (_gameState.CurrentLevel == null || _gameState.CurrentPathColor == null || _gameState.CurrentPath.Count == 0)
                return;
                
            // Добавляем конечную точку в путь, если её еще нет
            if (_gameState.LastSelectedPoint != endPoint)
            {
                _gameState.CurrentPath.Add(endPoint);
            }
            
            // Отмечаем конечную точку как соединенную
            endPoint.IsConnected = true;
            
            // Создаем линии между всеми точками в пути
            for (int i = 0; i < _gameState.CurrentPath.Count - 1; i++)
            {
                var startPoint = _gameState.CurrentPath[i];
                var nextPoint = _gameState.CurrentPath[i + 1];
                
                var line = new Line(startPoint, nextPoint, _gameState.CurrentPathColor, _gameState.CurrentPathId);
                line.IsVisible = true;
                
                _gameState.CurrentLevel.Lines.Add(line);
                _gameState.CurrentLevel.AddLineToPaths(line);
            }
            
            // Проверяем, завершен ли уровень
            _gameState.CheckCompletedPaths();
            
            // Сбрасываем состояние пути
            _gameState.ResetPathState();
        }
        
        public void CancelPath()
        {
            if (_gameState.CurrentLevel == null)
                return;
                
            // Сбрасываем состояние пути
            _gameState.ResetPathState();
            IsDrawingPath = false;
            
            UpdateGameState();
        }
        
        private void UpdateGameState()
        {
            HasNextLevel = _gameState.HasNextLevel;
            
            if (_gameState.CurrentLevel != null)
            {
                IsLevelCompleted = _gameState.CurrentLevel.IsCompleted;
                
                if (IsLevelCompleted)
                {
                    StatusMessage = HasNextLevel
                        ? "Уровень пройден! Нажмите 'Следующий уровень'"
                        : "Поздравляем! Вы прошли все уровни!";
                }
                else
                {
                    if (_gameState.CurrentPathColor != null)
                    {
                        StatusMessage = "Соедините точки одинаковых цветов";
                    }
                    else
                    {
                        StatusMessage = "Соедините точки одинаковых цветов!";
                    }
                }
                
                OnPropertyChanged(nameof(LevelName));
                OnPropertyChanged(nameof(CurrentPath));
                OnPropertyChanged(nameof(CurrentPathColor));
            }
        }
        
        private void NextLevel()
        {
            if (_gameState.GoToNextLevel())
            {
                IsLevelCompleted = false;
                UpdateGameState();
            }
        }
        
        private void ResetLevel()
        {
            _gameState.ResetCurrentLevel();
            IsLevelCompleted = false;
            IsDrawingPath = false;
            UpdateGameState();
        }
        
        public Level? CurrentLevel => _gameState.CurrentLevel;
        
        public bool IsPointInCurrentPath(ModelPoint point)
        {
            return _gameState.IsPointInCurrentPath(point);
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