using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ConnectDotsGame.Services;

namespace ConnectDotsGame.ViewModels
{
    public class AboutPageViewModel : INotifyPropertyChanged
    {
        public string AboutText { get; } = "Цель игры — соединить точки одного цвета линиями, не пересекающимися с другими линиями.";
        public ICommand NavigateToMenuCommand { get; }

        public AboutPageViewModel(INavigation navigation)
        {
            NavigateToMenuCommand = new RelayCommand(() => navigation.NavigateTo<MainPageViewModel>());
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
