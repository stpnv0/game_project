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
        private bool _hitWrongColor; // Новый флаг
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
                string colorKey = _activeColor?.ToString() ?? "";
                if (!string.IsNullOrEmpty(colorKey) && _currentPaths.TryGetValue(colorKey, out var path))
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
            
            // Загружаем сохраненный прогресс
            _gameState.LoadProgress();
            
            // Кнопка следующего уровня доступна, если уровень был когда-либо пройден
            NextLevelCommand = new RelayCommand(NextLevel, () => CurrentLevel?.WasEverCompleted == true && HasNextLevel);
            ResetLevelCommand = new RelayCommand(ResetLevel, () => _gameState.CurrentLevel != null);
            BackToMenuCommand = new RelayCommand(() => _navigation.GoBack());
            BackToLevelsCommand = new RelayCommand(() => _navigation.NavigateTo<LevelSelectViewModel>());
            
            // Устанавливаем начальное состояние
            HasNextLevel = _gameState.HasNextLevel;
            IsLevelCompleted = CurrentLevel?.IsCompleted ?? false;
            
            // Обновляем статус и состояние кнопок
            UpdateGameState();
            ((RelayCommand)NextLevelCommand).RaiseCanExecuteChanged();
            
            // Устанавливаем начальное сообщение
            if (CurrentLevel?.WasEverCompleted == true)
            {
                StatusMessage = HasNextLevel
                    ? "Уровень был пройден! Можете перейти к следующему уровню"
                    : "Поздравляем! Вы прошли все уровни!";
            }
            else
            {
                StatusMessage = "Соедините точки одинаковых цветов!";
            }
        }
        
        // Конструктор для режима дизайна
        public GameViewModel() : this(new DummyNavigation(), new GameState()) { }
        

        // Проверяет, являются ли точки соседями по вертикали или горизонтали
         private bool IsNeighbor(ModelPoint a, ModelPoint b)
         {
            return (Math.Abs(a.Row - b.Row) == 1 && a.Column == b.Column) ||
                   (Math.Abs(a.Column - b.Column) == 1 && a.Row == b.Row);
         }
        // Начало пути от точки
        public void StartPath(ModelPoint point)
        {
            if (CurrentLevel == null || !point.HasColor || point.Color == null)
                return;
                
            string colorKey = point.Color.ToString() ?? "";
                
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
            _hitWrongColor = false; // Сбрасываем флаг при начале нового пути
            
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

            string colorKey = _activeColor.ToString() ?? "";
            if (!_currentPaths.TryGetValue(colorKey, out var currentPath) || currentPath.Count == 0)
                return;

            var lastPoint = currentPath.Last();

            // Если это та же самая точка, ничего не делаем
            if (lastPoint.Row == point.Row && lastPoint.Column == point.Column)
                return;

            // Проверяем, что точки являются соседними
            if (!IsNeighbor(lastPoint, point))
                return;

            foreach (var path in CurrentLevel.Paths.ToList())
            {
                // Пропускаем проверку для нашего собственного пути
                if (path.Key == $"{colorKey}-path")
                    continue;

                // Проверяем, проходит ли путь через текущую точку
                bool pointInPath = path.Value.Any(line => 
                    (line.StartPoint.Row == point.Row && line.StartPoint.Column == point.Column) ||
                    (line.EndPoint.Row == point.Row && line.EndPoint.Column == point.Column));

                if (pointInPath)
                {
                    // Получаем цвет пересекаемого пути
                    string intersectedPathColor = path.Key.Replace("-path", "");
                    
                    // Удаляем пересекаемый путь
                    CurrentLevel.ClearPath(path.Key);
                    
                    // Сбрасываем соединение точек пересекаемого пути
                    var pointsToReset = CurrentLevel.Points
                        .Where(p => p.HasColor && p.Color?.ToString() == intersectedPathColor)
                        .ToList();
                        
                    foreach (var pointToReset in pointsToReset)
                    {
                        pointToReset.IsConnected = false;
                    }
                    
                    // Удаляем путь из словаря текущих путей
                    _currentPaths.Remove(intersectedPathColor);
                }
            }

            // Проверяем, не возвращаемся ли мы к какой-либо точке пути
            int returnIndex = currentPath.FindIndex(p => p.Row == point.Row && p.Column == point.Column);
            if (returnIndex != -1)
            {
                // Удаляем все точки после точки возврата
                while (currentPath.Count > returnIndex + 1)
                {
                    var removedPoint = currentPath[currentPath.Count - 1];
                    // Если удаляемая точка цветная, снимаем с неё отметку соединения
                    if (removedPoint.HasColor && removedPoint.Color == _activeColor && removedPoint != currentPath[0])
                    {
                        removedPoint.IsConnected = false;
                    }
                    currentPath.RemoveAt(currentPath.Count - 1);
                }
                
                _activePoint = point;

                // Обновляем линии на UI
                RedrawCurrentPath(colorKey, currentPath);
                return;
            }

            // Если точка уже имеет другой цвет (не наш цвет), запрещаем движение
            if (point.HasColor && point.Color != _activeColor)
                return;

            // Проверяем, не пересекаем ли мы другие пути
            var crossingPath = GameLogic.CheckForCrossingPath(CurrentLevel, point);
            if (crossingPath != null)
            {
                // Запрещаем пересечение с любыми путями
                return;
            }

            // Если это конечная точка нашего цвета
            if (point.HasColor && point.Color == _activeColor && point != currentPath[0])
            {
                // Добавляем точку в путь и завершаем его
                currentPath.Add(point);
                CreatePathLines(colorKey, currentPath);

                // Отмечаем точки как соединенные
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

                // Проверяем завершение уровня
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
                string colorKey = _activeColor.ToString() ?? "";
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
                case "Aqua": return Brushes.Cyan;
                case "Pink": return Brushes.Pink;
                default: return Brushes.Gray;
            }
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
                _gameState.CurrentLevelIndex++;
                IsLevelCompleted = true;
                _currentPaths.Clear();
                UpdateGameState();
            }
        }
        
        private void ResetLevel()
        {
            if (CurrentLevel != null)
            {
                bool wasEverCompleted = CurrentLevel.WasEverCompleted; // Сохраняем состояние WasEverCompleted
                
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
                
                // Сбрасываем состояние уровня, но сохраняем WasEverCompleted
                CurrentLevel.IsCompleted = false;
                CurrentLevel.WasEverCompleted = wasEverCompleted;
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
                    ? "Уровень пройден! Нажмите 'Следующий уровень' или начните заново"
                    : "Поздравляем! Вы прошли все уровни!";
            }
            else if (CurrentLevel?.WasEverCompleted == true)
            {
                StatusMessage = "Повторное прохождение уровня. Соедините точки одинаковых цветов!";
            }
            else
            {
                StatusMessage = "Соедините точки одинаковых цветов!";
            }
            
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
            if (!IsDrawingPath) 
                return;
            
            var colorKey = _activeColor?.ToString() ?? "";
            var currentPath = _currentPaths.ContainsKey(colorKey) ? _currentPaths[colorKey] : new List<ModelPoint>();
            
            // Если путь слишком короткий или нет конечной точки
            if (currentPath.Count < 2 || endPoint == null)
            {
                // Отменяем путь
                CancelPath();
                return;
            }

            // Проверяем, что конечная точка правильного цвета
            if (!endPoint.HasColor || endPoint.Color != _activeColor)
            {
                CancelPath();
                return;
            }

            // Проверяем, что конечная точка не является начальной
            if (currentPath[0] == endPoint)
            {
                CancelPath();
                return;
            }

            // Если конечная точка не является соседней с последней точкой пути
            var lastPoint = currentPath.Last();
            if (!IsNeighbor(lastPoint, endPoint))
            {
                CancelPath();
                return;
            }

            // Добавляем конечную точку и завершаем путь
            if (!currentPath.Contains(endPoint))
            {
                currentPath.Add(endPoint);
                CreatePathLines(colorKey, currentPath);
                
                // Отмечаем только начальную и конечную точки как соединенные
                foreach (var pathPoint in currentPath)
                {
                    if (pathPoint.HasColor && pathPoint.Color == _activeColor)
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