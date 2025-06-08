using System;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using ConnectDotsGame.Utils;
using ConnectDotsGame.Services;
using ConnectDotsGame.Models;

namespace ConnectDotsGame.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private readonly INavigation _navigation;

        public ICommand PlayCommand { get; }
        public ICommand SettingsCommand { get; }
        public ICommand AboutCommand { get; }
        public ICommand ExitCommand { get; }

        public MainPageViewModel(INavigation navigation)
        {
            _navigation = navigation;

            PlayCommand = new RelayCommand(() => _navigation.NavigateTo<LevelSelectViewModel>());
            SettingsCommand = new RelayCommand(() => { /* Переход на экран настроек */ });
            AboutCommand = new RelayCommand(() => { /* Показать информацию об игре */ });
            ExitCommand = new RelayCommand(Exit);
        }

        private void Exit()
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
            {
                lifetime.Shutdown();
            }
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