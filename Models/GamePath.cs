using System;
using System.Collections.Generic;
using Avalonia.Media;

namespace ConnectDotsGame.Models
{
    /// <summary>
    /// Класс, представляющий путь между точками
    /// </summary>
    public class GamePath
    {
        /// <summary>
        /// Уникальный идентификатор пути
        /// </summary>
        public int PathId { get; set; }
        
        /// <summary>
        /// Цвет пути
        /// </summary>
        public IBrush Color { get; set; }
        
        /// <summary>
        /// Список точек, образующих путь
        /// </summary>
        public List<Point> Points { get; set; } = new List<Point>();
        
        /// <summary>
        /// Флаг, указывающий, завершен ли путь
        /// </summary>
        public bool IsComplete { get; set; }
        
        /// <summary>
        /// Флаг видимости пути
        /// </summary>
        public bool IsVisible { get; set; } = true;
        
        /// <summary>
        /// Толщина линии пути
        /// </summary>
        public double Thickness { get; set; } = 4.0;
        
        /// <summary>
        /// Конструктор пути
        /// </summary>
        /// <param name="pathId">ID пути</param>
        /// <param name="color">Цвет пути</param>
        public GamePath(int pathId, IBrush color)
        {
            PathId = pathId;
            Color = color;
            IsComplete = false;
        }
        
        /// <summary>
        /// Добавляет точку в путь
        /// </summary>
        /// <param name="point">Точка для добавления</param>
        public void AddPoint(Point point)
        {
            if (!Points.Contains(point))
            {
                Points.Add(point);
            }
        }
        
        /// <summary>
        /// Удаляет последнюю точку из пути
        /// </summary>
        /// <returns>Удаленная точка или null</returns>
        public Point RemoveLastPoint()
        {
            if (Points.Count == 0)
            {
                return null;
            }
            
            var lastPoint = Points[Points.Count - 1];
            Points.RemoveAt(Points.Count - 1);
            return lastPoint;
        }
        
        /// <summary>
        /// Проверяет, содержит ли путь указанную точку
        /// </summary>
        /// <param name="point">Проверяемая точка</param>
        /// <returns>true, если точка входит в путь, иначе false</returns>
        public bool ContainsPoint(Point point)
        {
            return Points.Contains(point);
        }
        
        /// <summary>
        /// Сбрасывает путь, удаляя все точки
        /// </summary>
        public void Reset()
        {
            Points.Clear();
            IsComplete = false;
        }
        
        /// <summary>
        /// Создает копию пути
        /// </summary>
        /// <returns>Новый объект GamePath, являющийся копией текущего</returns>
        public GamePath Clone()
        {
            var clonedPath = new GamePath(PathId, Color)
            {
                IsComplete = this.IsComplete,
                IsVisible = this.IsVisible,
                Thickness = this.Thickness
            };
            
            // Копируем список точек
            foreach (var point in Points)
            {
                clonedPath.Points.Add(point.Clone());
            }
            
            return clonedPath;
        }
    }
} 