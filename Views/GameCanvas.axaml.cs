using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using ConnectDotsGame.Models;
using ConnectDotsGame.Utils;
using ConnectDotsGame.ViewModels;
using ConnectDotsGame.Levels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using Point = Avalonia.Point;
using ModelPoint = ConnectDotsGame.Models.Point;

namespace ConnectDotsGame.Views
{
    public partial class GameCanvas : UserControl
    {
        private Canvas? _drawingCanvas;
        private GameViewModel? _viewModel;
        private const double CellSize = 60; // Размер ячейки сетки
        private bool _isDrawing = false;
        private bool _debugModeEnabled = false; // Отключаем отладочный режим по умолчанию
        private Level? _debugLevel; // Тестовый уровень для отладки
        private string _viewModelType = "unknown";
        private TextBlock? _debugText;
        private bool _canvasInitialized = false; // Флаг инициализации канваса
        
        public GameCanvas()
        {
            InitializeComponent();
            InitializeControls();
        }
        
        private void InitializeControls()
        {
            try
            {
                _drawingCanvas = this.FindControl<Canvas>("DrawingCanvas");
                _debugText = this.FindControl<TextBlock>("DebugText");
                
                if (_debugText != null)
                {
                    _debugText.Text = "Инициализация GameCanvas...";
                }
                
                // Подписываемся на событие PointerCaptureLost через код
                if (_drawingCanvas != null)
                {
                    _drawingCanvas.PointerCaptureLost += Canvas_PointerCaptureLost;
                }
                
                // Инициализируем тестовый уровень в отладочном режиме
                if (_debugModeEnabled)
                {
                    CreateDebugLevel();
                }
                
                _canvasInitialized = _drawingCanvas != null;
                if (!_canvasInitialized)
                {
                    UpdateDebugText("ОШИБКА: DrawingCanvas не найден!");
                }
                
                // Принудительно вызываем обновление после полной инициализации
                Dispatcher.UIThread.Post(() => {
                    UpdateDebugText("GameCanvas инициализирован");
                    DrawLevel();
                }, DispatcherPriority.Loaded);
            }
            catch (Exception ex)
            {
                UpdateDebugText($"Ошибка инициализации: {ex.Message}");
            }
        }
        
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            
            UpdateDebugText($"DataContext изменен: {DataContext?.GetType().Name ?? "null"}");
            
            // Сохраняем ViewModel
            if (DataContext is GameViewModel viewModel)
            {
                _viewModel = viewModel;
                _viewModelType = _viewModel.GetType().Name;
                UpdateDebugText($"ViewModel установлен: {_viewModelType}");
                
                try
                {
                    // Получаем уровень из ViewModel
                    var level = _viewModel.CurrentLevel;
                    UpdateDebugText($"Уровень: {level?.Name ?? "null"}");
                    
                    // Подписываемся на изменения свойств
                    _viewModel.PropertyChanged += ViewModel_PropertyChanged;
                    
                    // Принудительно вызываем отрисовку после смены контекста
                    DrawLevel();
                }
                catch (Exception ex)
                {
                    UpdateDebugText($"Ошибка настройки ViewModel: {ex.Message}");
                }
            }
            else
            {
                _viewModel = null;
                UpdateDebugText("Ошибка: DataContext - не GameViewModel");
            }
        }
        
        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs args)
        {
            
            // Перерисовываем канвас при изменении свойств
            if (args.PropertyName == "CurrentLevel" || 
                args.PropertyName == "IsLevelCompleted" ||
                args.PropertyName == "CurrentPath")
            {
                DrawLevel();
            }
        }
        
        private void UpdateDebugText(string message)
        {
            if (_debugText != null)
            {
                _debugText.Text = message;
            }
        }
        
        private void Canvas_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            UpdateDebugText("Начало пути");
            
            if (!EnsureCanvasAndViewModel())
            {
                return;
            }
            
            try
            {
                // Получаем координаты относительно Canvas
                var position = e.GetPosition(_drawingCanvas);
                
                // Ограничиваем координаты размером игрового поля
                if (_drawingCanvas == null || 
                    position.X < 0 || position.X > _drawingCanvas.Width || 
                    position.Y < 0 || position.Y > _drawingCanvas.Height)
                {
                    UpdateDebugText($"Клик вне игрового поля: {position.X:F1},{position.Y:F1}");
                    return;
                }
                
                // Проверяем, есть ли уровень 
                var level = _viewModel?.CurrentLevel ?? _debugLevel;
                if (level == null)
                {
                    UpdateDebugText("Ошибка: level is null");
                    return;
                }
                    
                // Находим точку в сетке по координатам
                var clickedPoint = GameLogic.FindPointAtPosition(level, position.X, position.Y, CellSize);
                
                if (clickedPoint != null)
                {
                    UpdateDebugText($"Нажатие на точку [{clickedPoint.Row},{clickedPoint.Column}], HasColor: {clickedPoint.HasColor}");
                    
                    // Начинаем рисование, если кликнули на цветную точку
                    if (clickedPoint.HasColor && _viewModel != null)
                    {
                        _viewModel.StartPath(clickedPoint);
                        _isDrawing = true;
                    }
                }
                else
                {
                    UpdateDebugText("Не найдена точка под курсором");
                }
            }
            catch (Exception ex)
            {
                UpdateDebugText($"Ошибка: {ex.Message}");
            }
            
            // Сохраняем захват указателя для обработки движения
            if (_isDrawing && _drawingCanvas != null)
            {
                e.Pointer.Capture(_drawingCanvas);
            }
        }
        
        private void Canvas_PointerMoved(object? sender, PointerEventArgs e)
        {
            if (!_isDrawing)
            {
                return;
            }
            
            if (!EnsureCanvasAndViewModel())
            {
                return;
            }
            
            try
            {
                // Получаем координаты относительно Canvas
                var position = e.GetPosition(_drawingCanvas);
                
                var level = _viewModel?.CurrentLevel ?? _debugLevel;
                if (level == null)
                {
                    UpdateDebugText("Ошибка PointerMoved: level is null");
                    return;
                }
                
                // Находим точку под курсором
                var hoveredPoint = GameLogic.FindPointAtPosition(level, position.X, position.Y, CellSize);
                
                if (hoveredPoint != null)
                {
                    UpdateDebugText($"Continuing path to point [{hoveredPoint.Row},{hoveredPoint.Column}]");
                    
                    try
                    {
                        // Продолжаем путь
                        _viewModel?.ContinuePath(hoveredPoint);
                        DrawLevel();
                    }
                    catch (Exception ex)
                    {
                        UpdateDebugText($"Ошибка ContinuePath: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateDebugText($"Ошибка PointerMoved: {ex.Message}");
            }
        }
        
        private void Canvas_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (!_isDrawing)
            {
                return;
            }
            
            if (!EnsureCanvasAndViewModel())
            {
                _isDrawing = false; // Сбрасываем флаг в любом случае
                return;
            }
            
            try
            {
                // Получаем координаты относительно Canvas
                var position = e.GetPosition(_drawingCanvas);
                // Сбрасываем флаг рисования
                _isDrawing = false;
                
                var level = _viewModel?.CurrentLevel ?? _debugLevel;
                if (level == null)
                {
                    UpdateDebugText("Ошибка PointerReleased: level is null");
                    return;
                }
                
                // Находим точку под курсором
                var releasedPoint = GameLogic.FindPointAtPosition(level, position.X, position.Y, CellSize);
                
                if (releasedPoint != null && releasedPoint.HasColor)
                {
                    UpdateDebugText($"Ending path at colored point [{releasedPoint.Row},{releasedPoint.Column}]");
                    
                    try
                    {
                        // Завершаем путь
                        _viewModel?.EndPath(releasedPoint);
                    }
                    catch (Exception ex)
                    {
                        UpdateDebugText($"Ошибка EndPath: {ex.Message}");
                    }
                }
                else
                {
                    UpdateDebugText("Отмена пути - нет подходящей конечной точки");
                    
                    try
                    {
                        // Отменяем путь
                        _viewModel?.CancelPath();
                    }
                    catch (Exception ex)
                    {
                        UpdateDebugText($"Ошибка CancelPath: {ex.Message}");
                    }
                }
                
                DrawLevel();
            }
            catch (Exception ex)
            {
                UpdateDebugText($"Ошибка PointerReleased: {ex.Message}");
            }
        }
        
        // Обработчик события потери захвата указателя
        private void Canvas_PointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
        {
            // При потере захвата указателя - сбрасываем путь
            if (_isDrawing)
            {
                _isDrawing = false; // Сбрасываем флаг рисования
                
                if (EnsureCanvasAndViewModel())
                {
                    try
                    {
                        UpdateDebugText("Отмена пути - потеря захвата указателя");
                        
                        // Отменяем текущий путь
                        _viewModel?.CancelPath();
                        DrawLevel();
                    }
                    catch (Exception ex)
                    {
                        UpdateDebugText($"Ошибка PointerCaptureLost: {ex.Message}");
                    }
                }
            }
        }
        
        // Вспомогательный метод для проверки канваса и ViewModel
        private bool EnsureCanvasAndViewModel()
        {
            if (!_canvasInitialized || _drawingCanvas == null)
            {
                UpdateDebugText("Ошибка: Canvas не инициализирован");
                return false;
            }
            
            // Проверка и восстановление ViewModel
            if (_viewModel == null && DataContext != null)
            {
                _viewModel = DataContext as GameViewModel;
                _viewModelType = _viewModel?.GetType().Name ?? "unknown";
                UpdateDebugText($"Восстановлен ViewModel: {_viewModelType}");
            }
            
            if (_viewModel == null)
            {
                UpdateDebugText("Ошибка: ViewModel отсутствует");
                return false;
            }
            
            return true;
        }
        
        private void DrawLevel()
        {
            if (_drawingCanvas == null)
            {
                return;
            }
            
            try
            {
                // Используем уровень из ViewModel или отладочный уровень
                var level = (_viewModel?.CurrentLevel ?? _debugLevel);
                if (level == null)
                {
                    return;
                }
                    
                // Очищаем канвас
                _drawingCanvas.Children.Clear();
                
                // Устанавливаем размер канваса в соответствии с размером сетки
                double canvasWidth = level.Columns * CellSize;
                double canvasHeight = level.Rows * CellSize;
                
                // Проверяем, что размеры положительные
                if (canvasWidth <= 0 || canvasHeight <= 0)
                {
                    return;
                }
                
                _drawingCanvas.Width = canvasWidth;
                _drawingCanvas.Height = canvasHeight;
                
                // Рисуем фон и сетку
                _drawingCanvas.Background = Brushes.Black;
                DrawGrid(level.Rows, level.Columns);
                
                // Отрисовка линий
                foreach (var line in level.Lines)
                {
                    if (line.StartPoint != null && line.EndPoint != null)
                    {
                        var startX = line.StartPoint.Column * CellSize + CellSize / 2;
                        var startY = line.StartPoint.Row * CellSize + CellSize / 2;
                        var endX = line.EndPoint.Column * CellSize + CellSize / 2;
                        var endY = line.EndPoint.Row * CellSize + CellSize / 2;
                        
                        var lineShape = new Avalonia.Controls.Shapes.Line
                        {
                            StartPoint = new Point(startX, startY),
                            EndPoint = new Point(endX, endY),
                            Stroke = line.Color,
                            StrokeThickness = 20,
                            StrokeLineCap = PenLineCap.Round
                        };
                        
                        _drawingCanvas.Children.Add(lineShape);
                    }
                }
                
                // Отрисовка временного пути 
                if (_viewModel != null)
                {
                    try
                    {
                        // Пробуем получить активный путь
                        var isDrawingPath = false;
                        List<ModelPoint>? currentPath = null;
                        IBrush? pathColor = null;
                        
                        try { isDrawingPath = _viewModel.IsDrawingPath; } catch {}
                        
                        if (isDrawingPath)
                        {
                            try 
                            { 
                                currentPath = _viewModel.CurrentPath; 
                                pathColor = _viewModel.CurrentPathColor ?? _viewModel.ActiveColor; 
                            } 
                            catch {}
                            
                            if (currentPath != null && currentPath.Count > 1 && pathColor != null)
                            {
                                for (int i = 0; i < currentPath.Count - 1; i++)
                                {
                                    var startPoint = currentPath[i];
                                    var endPoint = currentPath[i + 1];
                                    
                                    var startX = startPoint.Column * CellSize + CellSize / 2;
                                    var startY = startPoint.Row * CellSize + CellSize / 2;
                                    var endX = endPoint.Column * CellSize + CellSize / 2;
                                    var endY = endPoint.Row * CellSize + CellSize / 2;
                                    
                                    var lineShape = new Avalonia.Controls.Shapes.Line
                                    {
                                        StartPoint = new Point(startX, startY),
                                        EndPoint = new Point(endX, endY),
                                        Stroke = pathColor,
                                        StrokeThickness = 20,
                                        StrokeLineCap = PenLineCap.Round
                                    };
                                    
                                    _drawingCanvas.Children.Add(lineShape);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error drawing temp path: {ex.Message}");
                    }
                }
                
                int pointsDrawn = 0;
                
                // Отрисовка точек
                foreach (var point in level.Points)
                {
                    // Рассчитываем координаты для отображения
                    point.X = point.Column * CellSize + CellSize / 2;
                    point.Y = point.Row * CellSize + CellSize / 2;
                    
                    // Рисуем только цветные точки
                    if (point.HasColor)
                    {
                        // Создаем внешний круг (белая обводка)
                        var outerCircle = new Avalonia.Controls.Shapes.Ellipse
                        {
                            Width = CellSize * 0.8,
                            Height = CellSize * 0.8,
                            Fill = Brushes.White
                        };
                        
                        Canvas.SetLeft(outerCircle, point.X - outerCircle.Width / 2);
                        Canvas.SetTop(outerCircle, point.Y - outerCircle.Height / 2);
                        
                        _drawingCanvas.Children.Add(outerCircle);
                        
                        // Создаем внутренний круг (цветная точка)
                        var innerCircle = new Avalonia.Controls.Shapes.Ellipse
                        {
                            Width = CellSize * 0.7,
                            Height = CellSize * 0.7,
                            Fill = point.Color
                        };
                        
                        Canvas.SetLeft(innerCircle, point.X - innerCircle.Width / 2);
                        Canvas.SetTop(innerCircle, point.Y - innerCircle.Height / 2);
                        
                        _drawingCanvas.Children.Add(innerCircle);
                        
                        pointsDrawn++;
                    }
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DrawLevel: {ex.Message}");
            }
        }
        
        private void DrawGrid(int rows, int columns)
        {
            if (_drawingCanvas == null)
                return;
                
            // Рисуем горизонтальные линии
            for (int row = 0; row <= rows; row++)
            {
                var line = new Avalonia.Controls.Shapes.Line
                {
                    StartPoint = new Point(0, row * CellSize),
                    EndPoint = new Point(columns * CellSize, row * CellSize),
                    Stroke = Brushes.DarkGray,
                    StrokeThickness = 1
                };
                
                _drawingCanvas.Children.Add(line);
            }
            
            // Рисуем вертикальные линии
            for (int col = 0; col <= columns; col++)
            {
                var line = new Avalonia.Controls.Shapes.Line
                {
                    StartPoint = new Point(col * CellSize, 0),
                    EndPoint = new Point(col * CellSize, rows * CellSize),
                    Stroke = Brushes.DarkGray,
                    StrokeThickness = 1
                };
                
                _drawingCanvas.Children.Add(line);
            }
            
        }
        
        private void CreateDebugLevel()
        {
            // Создаем тестовый уровень, примерно как в LevelLoader.CreateLevel1()
            _debugLevel = new Level
            {
                Id = 1,
                Name = "Debug Level",
                Rows = 5,
                Columns = 5,
                Points = new List<ModelPoint>(),
                Lines = new List<Line>()
            };
            
            // Создаем все точки сетки
            int id = 1;
            for (int row = 0; row < _debugLevel.Rows; row++)
            {
                for (int col = 0; col < _debugLevel.Columns; col++)
                {
                    var point = new ModelPoint(id++, row, col);
                    _debugLevel.Points.Add(point);
                }
            }
            
            // Добавляем цветные точки (2 цвета - 4 точки для простоты)
            // Красные
            GetPointAt(_debugLevel, 0, 0).Color = Brushes.Red;
            GetPointAt(_debugLevel, 4, 4).Color = Brushes.Red;
            
            // Синие
            GetPointAt(_debugLevel, 0, 4).Color = Brushes.Blue;
            GetPointAt(_debugLevel, 4, 0).Color = Brushes.Blue;
            
        }
        
        private ModelPoint GetPointAt(Level level, int row, int col)
        {
            return level.Points.First(p => p.Row == row && p.Column == col);
        }
        
        private int CountColoredPoints(Level level)
        {
            return level.Points.Count(p => p.HasColor);
        }
        
        // Обработчик события при присоединении к визуальному дереву
        private void UserControl_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e)
        {
            UpdateDebugText("GameCanvas прикреплен к дереву");
            
            // Проверяем, что все критичные компоненты найдены
            if (_drawingCanvas == null)
            {
                _drawingCanvas = this.FindControl<Canvas>("DrawingCanvas");
                _canvasInitialized = _drawingCanvas != null;
                
                if (_canvasInitialized)
                {
                }
                else
                {
                    UpdateDebugText("ОШИБКА: DrawingCanvas не найден после прикрепления!");
                }
            }
            
            // Принудительно вызываем отрисовку после прикрепления к дереву
            Dispatcher.UIThread.Post(() => {
                DrawLevel();
            }, DispatcherPriority.Normal);
        }
        
        // Обобщенный метод для обработки событий нажатия и перемещения
        private bool HandlePointerEvent(Point position, string eventType)
        {
            if (_drawingCanvas == null || _viewModel == null)
            {
                return false;
            }
            
            // Ограничиваем координаты размером игрового поля
            if (position.X < 0 || position.X > _drawingCanvas.Width || 
                position.Y < 0 || position.Y > _drawingCanvas.Height)
            {
                return false;
            }
            
            var level = _viewModel.CurrentLevel ?? _debugLevel;
            if (level == null)
            {
                return false;
            }
            
            return true;
        }
        
        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            
            // Отписываемся от событий для предотвращения утечек памяти
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }
            
            if (_drawingCanvas != null)
            {
                _drawingCanvas.PointerCaptureLost -= Canvas_PointerCaptureLost;
            }
        }
    }
} 