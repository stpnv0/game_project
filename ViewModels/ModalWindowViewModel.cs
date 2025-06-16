using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ConnectDotsGame.ViewModels
{
    public class ModalWindowViewModel : INotifyPropertyChanged
    {
        private string _title = string.Empty;
        private string _message = string.Empty;
        private string _buttonText = string.Empty;
        private readonly Action _onButtonClick;

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged();
            }
        }

        public string ButtonText
        {
            get => _buttonText;
            set
            {
                _buttonText = value;
                OnPropertyChanged();
            }
        }

        public ICommand CloseCommand { get; }

        public ModalWindowViewModel(string title, string message, string buttonText, Action onButtonClick)
        {
            _onButtonClick = onButtonClick ?? throw new ArgumentNullException(nameof(onButtonClick));
            
            Title = title;
            Message = message;
            ButtonText = buttonText;

            CloseCommand = new RelayCommand(_onButtonClick);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 