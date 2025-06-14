using System;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ConnectDotsGame.Utils;

namespace ConnectDotsGame.ViewModels
{
    public class ModalWindowViewModel : INotifyPropertyChanged
    {
        private string _title = string.Empty;
        private string _message = string.Empty;
        private string _buttonText = string.Empty;
        private Action? _onButtonClick;

        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Message
        {
            get => _message;
            set
            {
                if (_message != value)
                {
                    _message = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ButtonText
        {
            get => _buttonText;
            set
            {
                if (_buttonText != value)
                {
                    _buttonText = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand CloseCommand { get; }

        public ModalWindowViewModel(string title, string message, string buttonText, Action onButtonClick)
        {
            Title = title;
            Message = message;
            ButtonText = buttonText;
            _onButtonClick = onButtonClick;

            CloseCommand = new RelayCommand(ExecuteClose);
        }

        private void ExecuteClose()
        {
            _onButtonClick?.Invoke();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 