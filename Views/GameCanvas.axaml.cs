using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Controls.Shapes;
using System;
using ConnectDotsGame.Models;
using ConnectDotsGame.ViewModels;
using Point = ConnectDotsGame.Models.Point;
using Line = Avalonia.Controls.Shapes.Line;

namespace ConnectDotsGame.Views
{
    public partial class GameCanvas : UserControl
    {
        private const double CellSize = 60; // Размер одной клетки сетки
        private const double DotSize = 50; // Размер точки
        private const double PathThickness = 25; // Толщина линии пути
        private const double DotOffset = 5; // (CellSize - DotSize) / 2 - Отступ точки от края клетки
        private const double CenterOffset = 30; // CellSize / 2 - Отступ до центра клетки
        
        private Canvas? _canvas;
        private GameViewModel? _viewModel;
        private bool _drawing; // Флаг, указывающий идет ли сейчас рисование
        
        public GameCanvas()
        {
            InitializeComponent();
            _canvas = this.FindControl<Canvas>("DrawingCanvas") 
                ?? throw new InvalidOperationException("DrawingCanvas not found");
            _canvas.PointerPressed += (s, e) => OnPointer(e.GetPosition(_canvas), true);
            _canvas.PointerMoved += (s, e) => OnPointer(e.GetPosition(_canvas), false);
            _canvas.PointerReleased += (s, e) => OnPointerReleased(e.GetPosition(_canvas));
        }
        
        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            if (DataContext is GameViewModel viewModel)
            {
                _viewModel = viewModel;
                _viewModel.PropertyChanged += (s, e) => Draw();
                Draw();
            }
        }

        // Метод отрисовки игрового поля
        private void Draw()
        {
            if (_viewModel?.CurrentLevel == null || _canvas == null) return;
            
            var level = _viewModel.CurrentLevel;
            _canvas.Children.Clear(); // очищаем канвас
            //размеры канваса
            _canvas.Width = level.Columns * CellSize;
            _canvas.Height = level.Rows * CellSize;
            _canvas.Background = Brushes.Black;

            // Сетка
            for (int i = 0; i <= level.Rows; i++)
                Add(new Line { StartPoint = new(0, i * CellSize), EndPoint = new(level.Columns * CellSize, i * CellSize), Stroke = Brushes.Gray });
            for (int i = 0; i <= level.Columns; i++)
                Add(new Line { StartPoint = new(i * CellSize, 0), EndPoint = new(i * CellSize, level.Rows * CellSize), Stroke = Brushes.Gray });

            // Линии
            foreach (var line in level.Lines)
                Add(new Line 
                { 
                    StartPoint = new(line.StartPoint.Column * CellSize + CenterOffset, line.StartPoint.Row * CellSize + CenterOffset),
                    EndPoint = new(line.EndPoint.Column * CellSize + CenterOffset, line.EndPoint.Row * CellSize + CenterOffset),
                    Stroke = line.Color, 
                    StrokeThickness = PathThickness, 
                    StrokeLineCap = PenLineCap.Round 
                });

            // Точки
            foreach (var point in level.Points)
                if (point.HasColor)
                {
                    var dot = new Ellipse { Width = DotSize, Height = DotSize, Fill = point.Color };
                    Canvas.SetLeft(dot, point.Column * CellSize + DotOffset);
                    Canvas.SetTop(dot, point.Row * CellSize + DotOffset);
                    Add(dot);
                }
        }

        // Добавление элемента на канвас
        private void Add(Control control)
        {
            if (_canvas != null) _canvas.Children.Add(control);
        }

        // Получение точки по координатам
        private Point? GetPoint(Avalonia.Point pos) =>
            _viewModel?.CurrentLevel?.GetPointByPosition((int)(pos.Y / CellSize), (int)(pos.X / CellSize));

        // Обработка нажатия и движения
        private void OnPointer(Avalonia.Point pos, bool isPress)
        {
            var point = GetPoint(pos);
            if (point == null) return;

            if (isPress && point.HasColor) // Начало пути
            {
                _drawing = true;
                _viewModel?.StartPath(point);
            }
            else if (_drawing) // Продолжение пути
            {
                if (isPress) return; 
                _viewModel?.ContinuePath(point);
            }
        }

        // Обработка отпускания 
        private void OnPointerReleased(Avalonia.Point pos)
        {
            if (!_drawing) return;
            _drawing = false;
            
            var point = GetPoint(pos);
            _viewModel?.EndPath(point); // Завершение пути
            Draw(); // Перерисовка
        }
    }
} 