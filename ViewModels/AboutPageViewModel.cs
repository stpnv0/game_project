using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ConnectDotsGame.ViewModels
{
    public class AboutPageViewModel : INotifyPropertyChanged
    {
        private string _aboutText = "Цель игры — соединить точки одного цвета линиями, не пересекающимися с другими линиями.";

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

        public AboutPageViewModel() { }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
