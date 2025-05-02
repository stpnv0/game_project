using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia.Media;
using ConnectDotsGame.Commands;
using ConnectDotsGame.Models;
using ConnectDotsGame.Navigation;
using ConnectDotsGame.Utils;
using ModelPoint = ConnectDotsGame.Models.Point;

namespace ConnectDotsGame.ViewModels
{
    public class GameViewModel : INotifyPropertyChanged
    {
        private readonly GameState _gameState;
        private readonly INavigation _navigation;
        private string _statusMessage = string.Empty;
        private bool _isLevelCompleted;
        private bool _hasNextLevel;
        private bool _isDrawingPath;
        private ModelPoint? _activePoint;
        private IBrush? _activeColor;
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

        public ModelPoint? ActivePoint => _activePoint;
        
        public IBrush? ActiveColor => _activeColor;
        
        public List<ModelPoint> CurrentPath 
        { 
            get 
            {
                if (_activeColor != null && _currentPaths.TryGetValue(_activeColor.ToString(), out var path))
                {
                    return path;
                }
                return new List<ModelPoint>();
            }
        }
        
        public IBrush? CurrentPathColor => ActiveColor;
        
        public ICommand NextLevelCommand { get; }
        public ICommand ResetLevelCommand { get; }
        public ICommand BackToMenuCommand { get; }
        public ICommand BackToLevelsCommand { get; }
        
        public GameViewModel(INavigation navigation, GameState gameState)
        {
            _navigation = navigation;
            _gameState = gameState;
            
            NextLevelCommand = new RelayCommand(NextLevel, () => IsLevelCompleted && HasNextLevel);
            ResetLevelCommand = new RelayCommand(ResetLevel, () => _gameState.CurrentLevel != null);
            BackToMenuCommand = new RelayCommand(() => _navigation.GoBack());
            BackToLevelsCommand = new RelayCommand(() => _navigation.NavigateTo<LevelSelectViewModel>());
            
            StatusMessage = "Соедините точки одинаковых цветов!";
            UpdateGameState();
        }
        
        // Конструктор для режима дизайна
        public GameViewModel() : this(new DummyNavigation(), new GameState()) { }
        
        // Начало пути от точки
        public void StartPath(ModelPoint point)
        {
            if (CurrentLevel == null || IsLevelCompleted || !point.HasColor)
                return;
                
            string colorKey = point.Color.ToString();
                
            // Сбрасываем предыдущий незавершенный путь того же цвета
            if (_currentPaths.ContainsKey(colorKey))
            {
                // Удаляем пути этого цвета с поля
                CurrentLevel.ClearPath($"{colorKey}-path");
                // И из нашего словаря
                _currentPaths.Remove(colorKey);
            }
            
            // Начинаем новый путь
            _activePoint = point;
            _activeColor = point.Color;
            IsDrawingPath = true;
            
            // Добавляем точку в текущий путь
            var path = new List<ModelPoint> { point };
            _currentPaths[colorKey] = path;
            
            // Отмечаем точку как активную
            point.IsConnected = true;
            
            OnPropertyChanged(nameof(ActivePoint));
            OnPropertyChanged(nameof(ActiveColor));
            OnPropertyChanged(nameof(CurrentLevel));
        }
        
        // Продолжение пути через точку
        public void ContinuePath(ModelPoint point)
        {
            if (CurrentLevel == null || !IsDrawingPath || _activePoint == null || _activeColor == null)
                return;
                
            string colorKey = _activeColor.ToString();
            if (!_currentPaths.TryGetValue(colorKey, out var currentPath) || currentPath.Count == 0)
                return;
                
            // Получаем последнюю точку пути
            var lastPoint = currentPath.Last();
            
            // Если это та же самая точка, ничего не делаем
            if (lastPoint.Row == point.Row && lastPoint.Column == point.Column)
                return;
                
            // Если точка уже в пути, проверяем, не движемся ли мы назад
            bool isBacktrack = currentPath.Count >= 2 && 
                              currentPath[currentPath.Count - 2].Row == point.Row && 
                              currentPath[currentPath.Count - 2].Column == point.Column;
                
            if (isBacktrack)
            {
                // Удаляем последнюю точку из пути (движение назад)
                currentPath.RemoveAt(currentPath.Count - 1);
                _activePoint = point;
                
                // Обновляем линии на UI
                RedrawCurrentPath(colorKey, currentPath);
                return;
            }
            
            // Проверка, находится ли точка рядом с последней
            int rowDiff = Math.Abs(lastPoint.Row - point.Row);
            int colDiff = Math.Abs(lastPoint.Column - point.Column);
            
            // Точки должны быть соседними только по вертикали или горизонтали (не по диагонали)
            bool isAdjacent = (rowDiff == 0 && colDiff == 1) || (rowDiff == 1 && colDiff == 0);
            
            if (!isAdjacent)
                return;
                
            // Проверка, не занята ли точка другим путем
            var targetPath = GameLogic.CheckForCrossingPath(CurrentLevel, point);
            if (targetPath != null)
            {
                // Если пересекаем путь того же цвета, запрещаем ход
                if (targetPath.StartsWith(colorKey))
                {
                    Console.WriteLine($"Недопустимый ход: Точка {point.Row},{point.Column} уже занята линией того же цвета.");
                    return;
                }
                // Если пересекаем путь другого цвета, очищаем его
                else
                {
                    CurrentLevel.ClearPath(targetPath);
                    _currentPaths.Remove(targetPath.Replace("-path", ""));
                }
            }
            
            // Если это цветная точка и не того же цвета, нельзя проходить через неё
            if (point.HasColor && point.Color != _activeColor)
                return;
                
            // Если это точка того же цвета, проверяем, завершаем ли мы путь
            if (point.HasColor && point.Color == _activeColor && point != currentPath[0])
            {
                // Добавляем конечную точку в путь
                currentPath.Add(point);
                
                // Создаем все линии пути
                CreatePathLines(colorKey, currentPath);
                
                // Отмечаем ВСЕ точки в пути как соединенные
                foreach (var pathPoint in currentPath)
                {
                    if (pathPoint.HasColor && pathPoint.Color == _activeColor)
                    {
                        pathPoint.IsConnected = true;
                    }
                }
                
                // Завершаем рисование
                _activePoint = null;
                _activeColor = null;
                IsDrawingPath = false;
                
                // Проверяем, завершён ли уровень
                CheckLevelCompletion();
                
                // Обновляем UI
                OnPropertyChanged(nameof(ActivePoint));
                OnPropertyChanged(nameof(ActiveColor));
                OnPropertyChanged(nameof(CurrentLevel));
                
                return;
            }
            
            // Добавляем точку в путь
            currentPath.Add(point);
            _activePoint = point;
            
            // Обновляем линии на UI
            RedrawCurrentPath(colorKey, currentPath);
            
            OnPropertyChanged(nameof(ActivePoint));
            OnPropertyChanged(nameof(CurrentLevel));
        }
        
        // Отмена текущего пути
        public void CancelPath()
        {
            if (_activeColor != null)
            {
                string colorKey = _activeColor.ToString();
                if (_currentPaths.TryGetValue(colorKey, out var path))
                {
                    // Удаляем временные линии пути
                    CurrentLevel?.ClearPath($"{colorKey}-path");
                    
                    // Сбрасываем соединение для начальной и конечной точек
                    foreach (var point in path.Where(p => p.HasColor && p.Color == _activeColor))
                    {
                        point.IsConnected = false;
                    }
                    
                    // Удаляем путь из словаря
                    _currentPaths.Remove(colorKey);
                }
            }
            
            // Сбрасываем состояние рисования
            _activePoint = null;
            _activeColor = null;
            IsDrawingPath = false;
            
            OnPropertyChanged(nameof(ActivePoint));
            OnPropertyChanged(nameof(ActiveColor));
            OnPropertyChanged(nameof(CurrentLevel));
        }
        
        // Создание линий для завершенного пути
        private void CreatePathLines(string colorKey, List<ModelPoint> pathPoints)
        {
            if (CurrentLevel == null || pathPoints.Count < 2)
                return;
                
            // Создаем линии между всеми точками в пути
            for (int i = 0; i < pathPoints.Count - 1; i++)
            {
                var startPoint = pathPoints[i];
                var endPoint = pathPoints[i + 1];
                
                var line = new Line(startPoint, endPoint, GetBrushByName(colorKey), $"{colorKey}-path");
                line.IsVisible = true;
                line.PathId = $"{colorKey}-path";
                
                CurrentLevel.Lines.Add(line);
                CurrentLevel.AddLineToPaths(line);
            }
        }
        
        // Перерисовка временного пути
        private void RedrawCurrentPath(string colorKey, List<ModelPoint> pathPoints)
        {
            if (CurrentLevel == null || pathPoints.Count < 2)
                return;
                
            // Сначала удаляем все старые линии этого пути
            CurrentLevel.ClearPath($"{colorKey}-path");
            
            // Затем создаем новые линии
            for (int i = 0; i < pathPoints.Count - 1; i++)
            {
                var startPoint = pathPoints[i];
                var endPoint = pathPoints[i + 1];
                
                var line = new Line(startPoint, endPoint, GetBrushByName(colorKey), $"{colorKey}-path");
                line.IsVisible = true;
                line.PathId = $"{colorKey}-path";
                
                CurrentLevel.Lines.Add(line);
                CurrentLevel.AddLineToPaths(line);
            }
            
            OnPropertyChanged(nameof(CurrentLevel));
        }
        
        // Получение Brush по имени цвета
        private IBrush GetBrushByName(string colorName)
        {
            // Используем словарь цветов вместо метода Parse
            switch (colorName)
            {
                case "Red": return Brushes.Red;
                case "Blue": return Brushes.Blue;
                case "Green": return Brushes.Green;
                case "Yellow": return Brushes.Yellow;
                case "Orange": return Brushes.Orange;
                case "Purple": return Brushes.Purple;
                case "Cyan": return Brushes.Cyan;
                default: return Brushes.Gray;
            }
        }
        
        // Проверка, завершен ли уровень
        private void CheckLevelCompletion()
        {
            if (CurrentLevel == null)
                return;
                
            // Проверяем все цветные точки на уровне
            var colorGroups = CurrentLevel.Points.Where(p => p.HasColor)
                                                .GroupBy(p => p.Color.ToString())
                                                .ToList();
            
            bool allCompleted = true;
            
            foreach (var group in colorGroups)
            {
                string colorKey = group.Key;
                
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
                _gameState.SaveProgress();
            }
            
            // Обновляем статус игры
            UpdateGameState();
        }
        
        private void NextLevel()
        {
            if (_gameState.HasNextLevel)
            {
                _gameState.CurrentLevelIndex++;
                IsLevelCompleted = false;
                _currentPaths.Clear();
                UpdateGameState();
            }
        }
        
        private void ResetLevel()
        {
            if (CurrentLevel != null)
            {
                // Сбрасываем состояние всех точек
                foreach (var point in CurrentLevel.Points)
                {
                    point.IsConnected = false;
                }
                
                // Очищаем все линии и пути
                CurrentLevel.Lines.Clear();
                CurrentLevel.Paths.Clear();
                
                // Сбрасываем состояние рисования
                _activePoint = null;
                _activeColor = null;
                IsDrawingPath = false;
                _currentPaths.Clear();
                
                // Сбрасываем состояние уровня
                CurrentLevel.IsCompleted = false;
                IsLevelCompleted = false;
                
                UpdateGameState();
                
                OnPropertyChanged(nameof(ActivePoint));
                OnPropertyChanged(nameof(ActiveColor));
                OnPropertyChanged(nameof(CurrentLevel));
            }
        }
        
        private void UpdateGameState()
        {
            HasNextLevel = _gameState.HasNextLevel;
            
            if (IsLevelCompleted)
            {
                StatusMessage = HasNextLevel
                    ? "Уровень пройден! Нажмите 'Следующий уровень'"
                    : "Поздравляем! Вы прошли все уровни!";
            }
            else
            {
                StatusMessage = "Соедините точки одинаковых цветов!";
            }
            
            OnPropertyChanged(nameof(LevelName));
            OnPropertyChanged(nameof(CurrentLevel));
        }
        
        // Завершение пути
        public void EndPath()
        {
            EndPath(null);
        }
        
        // Завершение пути на указанной точке
        public void EndPath(ModelPoint? endPoint)
        {
            if (!IsDrawingPath) return;
            
            var colorKey = _activeColor?.ToString() ?? "";
            var currentPath = _currentPaths.ContainsKey(colorKey) ? _currentPaths[colorKey] : new List<ModelPoint>();
            
            // Если указана конечная точка, добавляем её в путь
            if (endPoint != null && _activePoint != endPoint && 
                endPoint.HasColor && endPoint.Color != null && 
                endPoint.Color.Equals(_activeColor))
            {
                // Проверим, что точка имеет тот же цвет и не находится уже в пути
                if (!currentPath.Contains(endPoint))
                {
                    currentPath.Add(endPoint);
                    if (_currentPaths.ContainsKey(colorKey))
                    {
                        _currentPaths[colorKey] = currentPath;
                    }
                }
            }
            
            if (currentPath.Count > 1)
            {
                // Создаем все линии пути
                CreatePathLines(colorKey, currentPath);
                
                // Отмечаем ВСЕ точки в пути как соединенные
                foreach (var pathPoint in currentPath)
                {
                    if (pathPoint.HasColor && pathPoint.Color != null && 
                        pathPoint.Color.Equals(_activeColor))
                    {
                        pathPoint.IsConnected = true;
                    }
                }
            }
            
            // Сбрасываем состояние рисования
            _activePoint = null;
            _activeColor = null;
            IsDrawingPath = false;
            
            // Проверяем, завершён ли уровень
            CheckLevelCompletion();
            
            // Обновляем UI
            OnPropertyChanged(nameof(ActivePoint));
            OnPropertyChanged(nameof(ActiveColor));
            OnPropertyChanged(nameof(CurrentLevel));
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