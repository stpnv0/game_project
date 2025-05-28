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
                return;
                
            // Если точка уже в пути, проверяем, не движение ли это назад
            var (isInPath, _) = _gameState.CheckPointInPath(point);
            if (isInPath)
            {
                // Если это предпоследняя точка пути, значит, мы движемся назад
                if (_gameState.CurrentPath.Count >= 2 && 
                    _gameState.CurrentPath[_gameState.CurrentPath.Count - 2].Row == point.Row && 
                    _gameState.CurrentPath[_gameState.CurrentPath.Count - 2].Column == point.Column)
                {
                    // Удаляем последнюю точку пути
                    _gameState.RemoveLastPointFromPath();
                    UpdateGameState();
                }
                return;
            }
            
            // Если нажали на точку того же цвета - завершаем путь
            if (point.HasColor && point.Color == _gameState.CurrentPathColor)
            {
                // Проверяем, можно ли соединить последнюю точку пути с текущей
                if (GameLogic.CanConnectPoints(_gameState.CurrentLevel, _gameState.LastSelectedPoint, point))
                {
                    EndPath(point);
                }
                return;
            }
            
            // Если это пустая точка, проверяем, можно ли добавить к пути
            if (!point.HasColor)
            {
                // Проверяем, находится ли точка на одной линии (строке или столбце) с последней выбранной точкой
                bool sameRow = _gameState.LastSelectedPoint.Row == point.Row;
                bool sameColumn = _gameState.LastSelectedPoint.Column == point.Column;
                
                // Точки должны быть соседними только по вертикали или горизонтали, но не по диагонали
                int rowDiff = Math.Abs(point.Row - _gameState.LastSelectedPoint.Row);
                int colDiff = Math.Abs(point.Column - _gameState.LastSelectedPoint.Column);
                
                if ((rowDiff == 0 && colDiff == 1) || (colDiff == 0 && rowDiff == 1))
                {
                    // Проверяем, пересекаем ли мы существующий путь
                    var crossingPath = GameLogic.CheckForCrossingPath(_gameState.CurrentLevel, point);
                    if (crossingPath != null)
                    {
                        // Удаляем пересеченный путь
                        _gameState.CurrentLevel.ClearPath(crossingPath);
                    }
                    
                    // Добавляем точку к текущему пути
                    _gameState.LastSelectedPoint = point;
                    _gameState.CurrentPath.Add(point);
                    
                    UpdateGameState();
                }
            }
        }
        
        public void EndPath(ModelPoint endPoint)
        {
            if (_gameState.CurrentLevel == null || !IsDrawingPath || 
                _gameState.LastSelectedPoint == null || _gameState.CurrentPathColor == null)
                return;
                
            // Если закончили на точке того же цвета, что и начальная
            if (endPoint.HasColor && endPoint.Color == _gameState.CurrentPathColor)
            {
                // Проверяем, это не та же самая начальная точка
                if (_gameState.CurrentPath.Count == 1 && 
                    _gameState.CurrentPath[0].Row == endPoint.Row && 
                    _gameState.CurrentPath[0].Column == endPoint.Column)
                {
                    CancelPath();
                    return;
                }
                
                // Всегда соединяем с конечной точкой того же цвета
                // Завершаем путь и создаем линии
                CompletePath(endPoint);
                
                // Сбрасываем состояние рисования
                IsDrawingPath = false;
                
                UpdateGameState();
                return;
            }
            
            // Если точка не цветная или не того же цвета
            CancelPath();
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
            var (isInPath, _) = _gameState.CheckPointInPath(point);
            return isInPath;
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