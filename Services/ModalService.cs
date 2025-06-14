using System;
using Avalonia.Controls;
using ConnectDotsGame.ViewModels;
using ConnectDotsGame.Views;

namespace ConnectDotsGame.Services
{
    public class ModalService : IModalService
    {
        private readonly ContentControl _contentControl;
        public ModalService(ContentControl contentControl)
        {
            _contentControl = contentControl ?? throw new ArgumentNullException(nameof(contentControl));
        }

        public void ShowModal(string title, string message, string buttonText, Action onButtonClick)
        {
            var viewModel = new ModalWindowViewModel(title, message, buttonText, onButtonClick);
            var modalWindow = new ModalWindow
            {
                DataContext = viewModel
            };

            if (_contentControl.Parent is Window parentWindow)
            {
                modalWindow.ShowDialog(parentWindow);
            }
            else
            {
                modalWindow.Show();
            }
        }
    }
} 