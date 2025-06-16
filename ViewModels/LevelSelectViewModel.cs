using System;
using System.Windows.Input;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ConnectDotsGame.Models;
using ConnectDotsGame.Services;

namespace ConnectDotsGame.ViewModels
{
    public class LevelSelectViewModel : INotifyPropertyChanged
    {
        private readonly INavigation _navigation;
        private readonly GameState _gameState;

        // Статические кисти для всех экземпляров класса
        private static readonly IBrush LockedLevelBrush = new SolidColorBrush(Color.Parse("#808080"));
        private static readonly IBrush UnlockedLevelBrush = new SolidColorBrush(Color.Parse("#A3079D"));

        public ObservableCollection<LevelDisplayInfo> Levels { get; } = new();

        public ICommand SelectLevelCommand { get; }
        public ICommand BackToMainCommand { get; }

        // Используем статические кисти в конвертере
        public static IValueConverter LevelToBrushConverter { get; } = 
            new FuncValueConverter<bool, IBrush>(isLocked => isLocked ? LockedLevelBrush : UnlockedLevelBrush);

        public LevelSelectViewModel(INavigation navigation, GameState gameState)
        {
            _navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
            _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));

            SelectLevelCommand = new RelayCommand<int>(SelectLevel, CanSelectLevel);
            BackToMainCommand = new RelayCommand(() => _navigation.NavigateTo<MainPageViewModel>());
            
            UpdateLevels();
        }

        // Обновляет список уровней на основе текущего состояния игры
        public void UpdateLevels()
        {
            Levels.Clear();
            
            for (int i = 0; i < _gameState.Levels.Count; i++)
            {
                var level = _gameState.Levels[i];
                var isLocked = ShouldLevelBeLocked(i);
                
                Levels.Add(new LevelDisplayInfo 
                {
                    Id = i + 1,
                    Name = level.Name,
                    IsLocked = isLocked
                });
            }
        }

        private bool ShouldLevelBeLocked(int levelIndex)
        {
            if (levelIndex == 0) return false; // Первый уровень всегда разблокирован
            return !_gameState.Levels[levelIndex - 1].WasEverCompleted;
        }

        private void SelectLevel(int levelId)
        {
            if (CanSelectLevel(levelId))
            {
                _gameState.CurrentLevelIndex = levelId - 1;
                _navigation.NavigateTo<GameViewModel>(_gameState);
            }
        }
        
        private bool CanSelectLevel(int levelId)
        {
            return levelId > 0 && 
                   levelId <= Levels.Count && 
                   !Levels[levelId - 1].IsLocked;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // Информация об уровне для отображения в UI
    public class LevelDisplayInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsLocked { get; set; }
    }
}