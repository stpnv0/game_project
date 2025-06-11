using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ConnectDotsGame.Services;
using ConnectDotsGame.Utils;

namespace ConnectDotsGame.ViewModels
{
    public class AboutPageViewModel : INotifyPropertyChanged
    {
        private string _aboutText = "Цель игры — соединить точки одного цвета линиями, не пересекающимися с другими линиями.";

        private readonly INavigation _navigation;

        public ICommand NavigateToMenuCommand { get; }

        public AboutPageViewModel(INavigation navigation)
        {
            _navigation = navigation;
            NavigateToMenuCommand = new RelayCommand(() => _navigation.NavigateTo<MainPageViewModel>());
        }

        public string AboutText
        {
            get => _aboutText;
            set
            {
                if (_aboutText != value)
                {
                    _aboutText = value;
                    OnPropertyChanged();
                }
            }
        }

        // Удалён конструктор без параметров

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
