using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ConnectDotsGame.Commands;
using ConnectDotsGame.Models;
using ConnectDotsGame.Navigation;
using ConnectDotsGame.Utils;

namespace ConnectDotsGame.ViewModels
{
    public class LevelSelectViewModel : INotifyPropertyChanged
    {
        private readonly INavigation _navigation;
        private readonly GameState _gameState;

        public ObservableCollection<LevelInfo> Levels { get; } = new ObservableCollection<LevelInfo>();

        public ICommand SelectLevelCommand { get; }
        public ICommand BackToMainCommand { get; }

        public LevelSelectViewModel(INavigation navigation, GameState gameState)
        {
            _navigation = navigation;
            _gameState = gameState;

            // Заполняем список уровней
            UpdateLevels();

            SelectLevelCommand = new RelayCommand<int>(SelectLevel, CanSelectLevel);
            BackToMainCommand = new RelayCommand(() => _navigation.GoBack());
        }

        // Обновляет список уровней на основе текущего состояния игры
        public void UpdateLevels()
        {
            Levels.Clear();

            // Найти максимальный разблокированный уровень
            int maxUnlocked = 0;
            for (int i = 0; i < _gameState.Levels.Count; i++)
            {
                if (_gameState.Levels[i].IsCompleted)
                {
                    maxUnlocked = i + 1; // Разблокирован следующий
                }
            }

            for (int i = 0; i < _gameState.Levels.Count; i++)
            {
                var level = _gameState.Levels[i];
                bool isLocked = i > maxUnlocked;

                // Первый уровень всегда открыт
                if (i == 0) isLocked = false;

                Levels.Add(new LevelInfo
                {
                    Id = i + 1,
                    Name = level.Name,
                    IsCompleted = level.IsCompleted,
                    IsLocked = isLocked
                });
            }
        }
        // Конструктор для режима дизайна
        public LevelSelectViewModel() : this(new DummyNavigation(), new GameState()) 
        {
            // Добавляем несколько тестовых уровней для режима дизайна
            if (Levels.Count == 0)
            {
                Levels.Add(new LevelInfo { Id = 1, Name = "Простой", IsCompleted = true, IsLocked = false });
                Levels.Add(new LevelInfo { Id = 2, Name = "Лёгкий", IsCompleted = false, IsLocked = false });
                Levels.Add(new LevelInfo { Id = 3, Name = "Средний", IsCompleted = false, IsLocked = true });
                Levels.Add(new LevelInfo { Id = 4, Name = "Сложный", IsCompleted = false, IsLocked = true });
                Levels.Add(new LevelInfo { Id = 5, Name = "Эксперт", IsCompleted = false, IsLocked = true });
            }
        }

        private void SelectLevel(int levelId)
        {
            if (levelId >= 1 && levelId <= _gameState.Levels.Count && !IsLevelLocked(levelId))
            {
                _gameState.CurrentLevelIndex = levelId - 1;
                _navigation.NavigateTo<GameViewModel>();
            }
        }
        
        private bool CanSelectLevel(int levelId)
        {
            return !IsLevelLocked(levelId);
        }
        
        private bool IsLevelLocked(int levelId)
        {
            if (levelId <= 0 || levelId > Levels.Count) return true;
            return Levels[levelId - 1].IsLocked;
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    public class LevelInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public bool IsLocked { get; set; }
    }
} 