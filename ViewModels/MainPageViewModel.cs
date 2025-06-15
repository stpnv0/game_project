using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using ConnectDotsGame.Utils;
using ConnectDotsGame.Services;

namespace ConnectDotsGame.ViewModels
{
    public class MainPageViewModel
    {
        private readonly INavigation _navigation;

        public ICommand PlayCommand { get; }
        public ICommand AboutCommand { get; }
        public ICommand ExitCommand { get; }

        public MainPageViewModel(INavigation navigation)
        {
            _navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));

            PlayCommand = new RelayCommand(() => _navigation.NavigateTo<LevelSelectViewModel>());
            AboutCommand = new RelayCommand(() => _navigation.NavigateTo<AboutPageViewModel>());
            ExitCommand = new RelayCommand(Exit);
        }

        private void Exit()
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
            {
                lifetime.Shutdown();
            }
        }
    }
}