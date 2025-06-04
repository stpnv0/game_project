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
        private readonly PathModel _pathModel = new PathModel();
        private string _statusMessage = string.Empty;
        private bool _isLevelCompleted;
        private bool _hasNextLevel;
        private bool _isDrawingPath;
        private ModelPoint? _activePoint;
        private IBrush? _activeColor;

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
                return !string.IsNullOrEmpty(colorKey)
                    ? _pathModel.GetPath(colorKey)
                    : new List<ModelPoint>();
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

            _gameState.LoadProgress();

            NextLevelCommand = new RelayCommand(NextLevel, () => CurrentLevel?.WasEverCompleted == true && HasNextLevel);
            ResetLevelCommand = new RelayCommand(ResetLevel, () => _gameState.CurrentLevel != null);
            BackToMenuCommand = new RelayCommand(() => _navigation.GoBack());
            BackToLevelsCommand = new RelayCommand(() => _navigation.NavigateTo<LevelSelectViewModel>());

            HasNextLevel = _gameState.HasNextLevel;
            IsLevelCompleted = CurrentLevel?.IsCompleted ?? false;

            UpdateGameState();
            ((RelayCommand)NextLevelCommand).RaiseCanExecuteChanged();

            StatusMessage = CurrentLevel?.WasEverCompleted == true
                ? HasNextLevel ? "Уровень был пройден! Можете перейти к следующему уровню"
                               : "Поздравляем! Вы прошли все уровни!"
                : "Соедините точки одинаковых цветов!";
        }

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

            // Сброс предыдущего пути такого цвета
            _pathModel.ResetPath(colorKey);
            CurrentLevel.ClearPath($"{colorKey}-path");

            _activePoint = point;
            _activeColor = point.Color;
            IsDrawingPath = true;

            _pathModel.StartPath(colorKey, point);
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
            var currentPath = _pathModel.GetPath(colorKey);

            var lastPoint = currentPath.LastOrDefault();
            if (lastPoint == null) return;

            if (lastPoint.Row == point.Row && lastPoint.Column == point.Column)
                return;

            if (!IsNeighbor(lastPoint, point))
                return;

            foreach (var path in CurrentLevel.Paths.ToList())
            {
                if (path.Key == $"{colorKey}-path")
                    continue;

                bool pointInPath = path.Value.Any(line =>
                    (line.StartPoint.Row == point.Row && line.StartPoint.Column == point.Column) ||
                    (line.EndPoint.Row == point.Row && line.EndPoint.Column == point.Column));

                if (pointInPath)
                {
                    string intersectedPathColor = path.Key.Replace("-path", "");
                    CurrentLevel.ClearPath(path.Key);

                    var pointsToReset = CurrentLevel.Points
                        .Where(p => p.HasColor && p.Color?.ToString() == intersectedPathColor)
                        .ToList();

                    foreach (var pointToReset in pointsToReset)
                        pointToReset.IsConnected = false;

                    _pathModel.ResetPath(intersectedPathColor);
                }
            }

            int returnIndex = currentPath.FindIndex(p => p.Row == point.Row && p.Column == point.Column);
            if (returnIndex != -1)
            {
                while (currentPath.Count > returnIndex + 1)
                {
                    var removedPoint = currentPath[currentPath.Count - 1];
                    if (removedPoint.HasColor && removedPoint.Color == _activeColor && removedPoint != currentPath[0])
                        removedPoint.IsConnected = false;
                    currentPath.RemoveAt(currentPath.Count - 1);
                }
                _activePoint = point;
                RedrawCurrentPath(colorKey, currentPath);
                return;
            }

            if (point.HasColor && point.Color != _activeColor)
                return;

            var crossingPath = GameLogic.CheckForCrossingPath(CurrentLevel, point);
            if (crossingPath != null)
                return;

            if (point.HasColor && point.Color == _activeColor && point != currentPath[0])
            {
                currentPath.Add(point);
                CreatePathLines(colorKey, currentPath);

                foreach (var pathPoint in currentPath)
                {
                    if (pathPoint.HasColor && pathPoint.Color == _activeColor)
                        pathPoint.IsConnected = true;
                }

                _activePoint = null;
                _activeColor = null;
                IsDrawingPath = false;

                CheckLevelCompletion();

                OnPropertyChanged(nameof(ActivePoint));
                OnPropertyChanged(nameof(ActiveColor));
                OnPropertyChanged(nameof(CurrentLevel));
                return;
            }

            currentPath.Add(point);
            _activePoint = point;
            RedrawCurrentPath(colorKey, currentPath);

            OnPropertyChanged(nameof(ActivePoint));
            OnPropertyChanged(nameof(CurrentLevel));
        }

        public void CancelPath()
        {
            if (_activeColor != null)
            {
                string colorKey = _activeColor.ToString() ?? "";
                var path = _pathModel.GetPath(colorKey);

                CurrentLevel?.ClearPath($"{colorKey}-path");

                foreach (var point in path.Where(p => p.HasColor && p.Color == _activeColor))
                    point.IsConnected = false;

                _pathModel.ResetPath(colorKey);
            }
            _activePoint = null;
            _activeColor = null;
            IsDrawingPath = false;

            OnPropertyChanged(nameof(ActivePoint));
            OnPropertyChanged(nameof(ActiveColor));
            OnPropertyChanged(nameof(CurrentLevel));
        }

        private void CreatePathLines(string colorKey, List<ModelPoint> pathPoints)
        {
            if (CurrentLevel == null || pathPoints.Count < 2)
                return;

            for (int i = 0; i < pathPoints.Count - 1; i++)
            {
                var startPoint = pathPoints[i];
                var endPoint = pathPoints[i + 1];

                var line = new Line(startPoint, endPoint, GetBrushByName(colorKey), $"{colorKey}-path");
                line.PathId = $"{colorKey}-path";

                CurrentLevel.Lines.Add(line);
                CurrentLevel.AddLineToPaths(line);
            }
        }

        private void RedrawCurrentPath(string colorKey, List<ModelPoint> pathPoints)
        {
            if (CurrentLevel == null || pathPoints.Count < 2)
                return;

            CurrentLevel.ClearPath($"{colorKey}-path");

            for (int i = 0; i < pathPoints.Count - 1; i++)
            {
                var startPoint = pathPoints[i];
                var endPoint = pathPoints[i + 1];

                var line = new Line(startPoint, endPoint, GetBrushByName(colorKey), $"{colorKey}-path");
                line.PathId = $"{colorKey}-path";

                CurrentLevel.Lines.Add(line);
                CurrentLevel.AddLineToPaths(line);
            }
            OnPropertyChanged(nameof(CurrentLevel));
        }

        private IBrush GetBrushByName(string colorName)
        {
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

        private void CheckLevelCompletion()
        {
            if (CurrentLevel == null)
                return;

            bool allCompleted = GameLogic.CheckLevelCompleted(CurrentLevel, _pathModel);

            CurrentLevel.IsCompleted = allCompleted;
            IsLevelCompleted = allCompleted;

            if (allCompleted)
            {
                CurrentLevel.WasEverCompleted = true;
                _gameState.SaveProgress();
            }
            UpdateGameState();
        }

        private void NextLevel()
        {
            if (_gameState.HasNextLevel)
            {
                _gameState.CurrentLevelIndex++;
                IsLevelCompleted = true;
                _pathModel.ResetAll();
                UpdateGameState();
            }
        }

        private void ResetLevel()
        {
            if (CurrentLevel != null)
            {
                GameLogic.ResetLevel(CurrentLevel, _pathModel);
                _activePoint = null;
                _activeColor = null;
                IsDrawingPath = false;
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

        public void EndPath()
        {
            EndPath(null);
        }

        public void EndPath(ModelPoint? endPoint)
        {
            if (!IsDrawingPath)
                return;

            var colorKey = _activeColor?.ToString() ?? "";
            var currentPath = _pathModel.GetPath(colorKey);

            if (currentPath.Count < 2 || endPoint == null)
            {
                CancelPath();
                return;
            }

            if (!endPoint.HasColor || endPoint.Color != _activeColor)
            {
                CancelPath();
                return;
            }

            if (currentPath[0] == endPoint)
            {
                CancelPath();
                return;
            }

            var lastPoint = currentPath.Last();
            if (!IsNeighbor(lastPoint, endPoint))
            {
                CancelPath();
                return;
            }

            if (!currentPath.Contains(endPoint))
            {
                currentPath.Add(endPoint);
                CreatePathLines(colorKey, currentPath);

                foreach (var pathPoint in currentPath)
                {
                    if (pathPoint.HasColor && pathPoint.Color == _activeColor)
                    {
                        pathPoint.IsConnected = true;
                    }
                }
            }

            _activePoint = null;
            _activeColor = null;
            IsDrawingPath = false;

            CheckLevelCompletion();

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