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
                    Console.WriteLine("Подписались на PointerCaptureLost");
                }
                
                // Инициализируем тестовый уровень в отладочном режиме
                if (_debugModeEnabled)
                {
                    Console.WriteLine("Creating debug level for testing");
                    CreateDebugLevel();
                }
                
                _canvasInitialized = _drawingCanvas != null;
                if (!_canvasInitialized)
                {
                    Console.WriteLine("ERROR: DrawingCanvas not found during initialization!");
                    UpdateDebugText("ОШИБКА: DrawingCanvas не найден!");
                }
                
                // Принудительно вызываем обновление после полной инициализации
                Dispatcher.UIThread.Post(() => {
                    Console.WriteLine("GameCanvas initialized, forcing update");
                    UpdateDebugText("GameCanvas инициализирован");
                    DrawLevel();
                }, DispatcherPriority.Loaded);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during initialization: {ex.Message}");
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
            
            Console.WriteLine($"OnDataContextChanged: {DataContext?.GetType().Name ?? "null"}");
            UpdateDebugText($"DataContext изменен: {DataContext?.GetType().Name ?? "null"}");
            
            // Сохраняем ViewModel
            if (DataContext is GameViewModel viewModel)
            {
                _viewModel = viewModel;
                _viewModelType = _viewModel.GetType().Name;
                Console.WriteLine($"ViewModel set: {_viewModelType}");
                UpdateDebugText($"ViewModel установлен: {_viewModelType}");
                
                try
                {
                    // Получаем уровень из ViewModel
                    var level = _viewModel.CurrentLevel;
                    Console.WriteLine($"CurrentLevel: {level?.Name ?? "null"}, Points: {level?.Points.Count.ToString() ?? "0"}");
                    UpdateDebugText($"Уровень: {level?.Name ?? "null"}");
                    
                    // Подписываемся на изменения свойств
                    _viewModel.PropertyChanged += ViewModel_PropertyChanged;
                    
                    // Принудительно вызываем отрисовку после смены контекста
                    DrawLevel();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error setting up ViewModel: {ex.Message}");
                    UpdateDebugText($"Ошибка настройки ViewModel: {ex.Message}");
                }
            }
            else
            {
                _viewModel = null;
                Console.WriteLine("ERROR: DataContext is not GameViewModel");
                UpdateDebugText("Ошибка: DataContext - не GameViewModel");
            }
        }
        
        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs args)
        {
            Console.WriteLine($"Property changed: {args.PropertyName}");
            
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
            Console.WriteLine($"DEBUG: {message}");
        }
        
        private void Canvas_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            Console.WriteLine(">>> Canvas_PointerPressed ВЫЗВАН <<<");
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
                    Console.WriteLine($"Click outside game field: {position.X},{position.Y}, canvas size: {_drawingCanvas?.Width}x{_drawingCanvas?.Height}");
                    UpdateDebugText($"Клик вне игрового поля: {position.X:F1},{position.Y:F1}");
                    return;
                }
                
                Console.WriteLine($"Pointer pressed at position {position.X},{position.Y}");
                
                // Проверяем, есть ли уровень 
                var level = _viewModel?.CurrentLevel ?? _debugLevel;
                if (level == null)
                {
                    Console.WriteLine("ERROR: level is null");
                    UpdateDebugText("Ошибка: level is null");
                    return;
                }
                    
                // Проверяем, завершен ли уровень
                bool isCompleted = _viewModel?.IsLevelCompleted ?? false;
                
                if (isCompleted)
                {
                    Console.WriteLine("Level is already completed");
                    UpdateDebugText("Уровень уже завершен");
                    return;
                }
                    
                // Находим точку в сетке по координатам
                var clickedPoint = GameLogic.FindPointAtPosition(level, position.X, position.Y, CellSize);
                
                if (clickedPoint != null)
                {
                    Console.WriteLine($"Clicked on point at {clickedPoint.Row},{clickedPoint.Column}, HasColor: {clickedPoint.HasColor}");
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
                    Console.WriteLine("No point was clicked");
                    UpdateDebugText("Не найдена точка под курсором");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PointerPressed: {ex.Message}");
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
        Console.WriteLine("PointerMoved: Drawing is not active. Ignoring pointer movement.");
        return;
    }

    if (!EnsureCanvasAndViewModel())
    {
        Console.WriteLine("PointerMoved: ViewModel or Canvas is not initialized. Ignoring pointer movement.");
        return;
    }

    try
    {
        var position = e.GetPosition(_drawingCanvas);
        Console.WriteLine($"Pointer moved to position X={position.X}, Y={position.Y}");

        var level = _viewModel?.CurrentLevel ?? _debugLevel;
        if (level == null)
        {
            Console.WriteLine("ERROR in PointerMoved: CurrentLevel is null.");
            UpdateDebugText("ERROR in PointerMoved: CurrentLevel is null.");
            return;
        }

        var hoveredPoint = GameLogic.FindPointAtPosition(level, position.X, position.Y, CellSize);

        if (hoveredPoint != null)
        {
            Console.WriteLine($"PointerMoved: Hovered point detected at Row={hoveredPoint.Row}, Column={hoveredPoint.Column}.");

            var lastSelectedPoint = _viewModel?.CurrentPath?.LastOrDefault();
            if (lastSelectedPoint != null)
            {
                int rowDiff = Math.Abs(hoveredPoint.Row - lastSelectedPoint.Row);
                int colDiff = Math.Abs(hoveredPoint.Column - lastSelectedPoint.Column);

                Console.WriteLine($"PointerMoved: LastSelectedPoint at Row={lastSelectedPoint.Row}, Column={lastSelectedPoint.Column}.");
                Console.WriteLine($"PointerMoved: Calculated rowDiff={rowDiff}, colDiff={colDiff}.");

                // Проверяем недопустимые движения
                if ((rowDiff == 1 && colDiff == 1) || rowDiff > 1 || colDiff > 1)
                {
                    Console.WriteLine($"PointerMoved: Invalid movement to Row={hoveredPoint.Row}, Column={hoveredPoint.Column}. Stopping execution.");
                    return;
                }
            }

            Console.WriteLine($"PointerMoved: Continuing path to point Row={hoveredPoint.Row}, Column={hoveredPoint.Column}.");
            _viewModel?.ContinuePath(hoveredPoint);
            DrawLevel();
        }
        else
        {
            Console.WriteLine("PointerMoved: Hovered point is null. Ignoring pointer movement.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR in PointerMoved: {ex.Message}");
        Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        UpdateDebugText($"ERROR in PointerMoved: {ex.Message}");
    }
}
        
      private void Canvas_PointerReleased(object? sender, PointerReleasedEventArgs e)
{
    if (!_isDrawing)
    {
        Console.WriteLine("PointerReleased: Drawing is not active. Ignoring pointer release.");
        return;
    }

    if (!EnsureCanvasAndViewModel())
    {
        Console.WriteLine("PointerReleased: ViewModel or Canvas is not initialized. Ignoring pointer release.");
        return;
    }

    try
    {
        var position = e.GetPosition(_drawingCanvas);
        Console.WriteLine($"Pointer released at position X={position.X}, Y={position.Y}");

        var level = _viewModel?.CurrentLevel ?? _debugLevel;
        if (level == null)
        {
            Console.WriteLine("ERROR in PointerReleased: CurrentLevel is null.");
            UpdateDebugText("ERROR in PointerReleased: CurrentLevel is null.");
            return;
        }

        var hoveredPoint = GameLogic.FindPointAtPosition(level, position.X, position.Y, CellSize);
        if (hoveredPoint != null)
        {
            Console.WriteLine($"PointerReleased: Hovered point detected at Row={hoveredPoint.Row}, Column={hoveredPoint.Column}.");

            var lastSelectedPoint = _viewModel?.CurrentPath?.LastOrDefault();
            if (lastSelectedPoint != null)
            {
                int rowDiff = Math.Abs(hoveredPoint.Row - lastSelectedPoint.Row);
                int colDiff = Math.Abs(hoveredPoint.Column - lastSelectedPoint.Column);

                Console.WriteLine($"PointerReleased: LastSelectedPoint at Row={lastSelectedPoint.Row}, Column={lastSelectedPoint.Column}.");
                Console.WriteLine($"PointerReleased: Calculated rowDiff={rowDiff}, colDiff={colDiff}.");

                if ((rowDiff == 1 && colDiff == 1) || rowDiff > 1 || colDiff > 1)
                {
                    Console.WriteLine($"PointerReleased: Invalid movement to Row={hoveredPoint.Row}, Column={hoveredPoint.Column}. Stopping execution.");
                    return;
                }
            }

            Console.WriteLine($"PointerReleased: Attempting to end path at point Row={hoveredPoint.Row}, Column={hoveredPoint.Column}.");
            _viewModel?.EndPath(hoveredPoint);
        }
        else
        {
            Console.WriteLine("PointerReleased: Hovered point is null. Ignoring pointer release.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR in PointerReleased: {ex.Message}");
        Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        UpdateDebugText($"ERROR in PointerReleased: {ex.Message}");
    }
    finally
    {
        _isDrawing = false; // Завершаем рисование
        Console.WriteLine("PointerReleased: Drawing has been deactivated.");
    }
}
        
        // Обработчик события потери захвата указателя
        private void Canvas_PointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
        {
            Console.WriteLine(">>> Canvas_PointerCaptureLost ВЫЗВАН <<<");
            
            // При потере захвата указателя - сбрасываем путь
            if (_isDrawing)
            {
                _isDrawing = false; // Сбрасываем флаг рисования
                
                if (EnsureCanvasAndViewModel())
                {
                    try
                    {
                        Console.WriteLine("Canceling path due to pointer capture lost");
                        UpdateDebugText("Отмена пути - потеря захвата указателя");
                        
                        // Отменяем текущий путь
                        _viewModel?.CancelPath();
                        DrawLevel();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in PointerCaptureLost: {ex.Message}");
                        Console.WriteLine($"Stack trace: {ex.StackTrace}");
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
                Console.WriteLine("ERROR: Canvas is not initialized");
                UpdateDebugText("Ошибка: Canvas не инициализирован");
                return false;
            }
            
            // Проверка и восстановление ViewModel
            if (_viewModel == null && DataContext != null)
            {
                _viewModel = DataContext as GameViewModel;
                _viewModelType = _viewModel?.GetType().Name ?? "unknown";
                Console.WriteLine($"Восстановлено ViewModel из DataContext: {_viewModelType}");
                UpdateDebugText($"Восстановлен ViewModel: {_viewModelType}");
            }
            
            if (_viewModel == null)
            {
                Console.WriteLine("ERROR: ViewModel is null");
                UpdateDebugText("Ошибка: ViewModel отсутствует");
                return false;
            }
            
            return true;
        }
        
        private void DrawLevel()
        {
            if (_drawingCanvas == null)
            {
                Console.WriteLine("ERROR: _drawingCanvas is null");
                return;
            }
            
            try
            {
                // Используем уровень из ViewModel или отладочный уровень
                var level = (_viewModel?.CurrentLevel ?? _debugLevel);
                if (level == null)
                {
                    Console.WriteLine("ERROR: No level available (neither from ViewModel nor debug level)");
                    return;
                }
                    
                Console.WriteLine($"Drawing level: {level.Name}, Points: {level.Points.Count}, Size: {level.Rows}x{level.Columns}");
                
                // Очищаем канвас
                _drawingCanvas.Children.Clear();
                
                // Устанавливаем размер канваса в соответствии с размером сетки
                double canvasWidth = level.Columns * CellSize;
                double canvasHeight = level.Rows * CellSize;
                
                // Проверяем, что размеры положительные
                if (canvasWidth <= 0 || canvasHeight <= 0)
                {
                    Console.WriteLine($"ERROR: Invalid canvas size: {canvasWidth}x{canvasHeight}");
                    return;
                }
                
                _drawingCanvas.Width = canvasWidth;
                _drawingCanvas.Height = canvasHeight;
                
                Console.WriteLine($"Canvas size set to {_drawingCanvas.Width}x{_drawingCanvas.Height}");
                
                // Рисуем фон и сетку
                _drawingCanvas.Background = Brushes.Black;
                DrawGrid(level.Rows, level.Columns);
                
                // Отрисовка линий
                foreach (var line in level.Lines)
                {
                    if (line.IsVisible && line.StartPoint != null && line.EndPoint != null)
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
                
                Console.WriteLine($"Total colored points drawn: {pointsDrawn}");
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
                
            Console.WriteLine($"Drawing grid: {rows}x{columns}");
                
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
            
            Console.WriteLine("Grid drawing completed");
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
            
            Console.WriteLine($"Debug level created with {_debugLevel.Points.Count} points, {CountColoredPoints(_debugLevel)} colored");
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
            Console.WriteLine("GameCanvas attached to visual tree");
            UpdateDebugText("GameCanvas прикреплен к дереву");
            
            // Проверяем, что все критичные компоненты найдены
            if (_drawingCanvas == null)
            {
                _drawingCanvas = this.FindControl<Canvas>("DrawingCanvas");
                _canvasInitialized = _drawingCanvas != null;
                
                if (_canvasInitialized)
                {
                    Console.WriteLine("DrawingCanvas found after attachment");
                }
                else
                {
                    Console.WriteLine("ERROR: DrawingCanvas still not found after attachment!");
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
                Console.WriteLine($"ERROR in {eventType}: Canvas or ViewModel is null");
                return false;
            }
            
            // Ограничиваем координаты размером игрового поля
            if (position.X < 0 || position.X > _drawingCanvas.Width || 
                position.Y < 0 || position.Y > _drawingCanvas.Height)
            {
                Console.WriteLine($"{eventType} outside game field: {position.X},{position.Y}");
                return false;
            }
            
            var level = _viewModel.CurrentLevel ?? _debugLevel;
            if (level == null)
            {
                Console.WriteLine($"ERROR in {eventType}: Level is null");
                return false;
            }
            
            return true;
        }
        
        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            
            Console.WriteLine("GameCanvas detached from visual tree");
            
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