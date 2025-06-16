using System;
using Avalonia.Controls;
using ConnectDotsGame.ViewModels;
using ConnectDotsGame.Views;

namespace ConnectDotsGame.Services
{
    public class ModalService : IModalService
    {
        private readonly Window _parentWindow;
        
        public ModalService(Window parentWindow)
        {
            _parentWindow = parentWindow ?? throw new ArgumentNullException(nameof(parentWindow));
        }

        // Отображает модальное окно с параметрами
        public void ShowModal(string title, string message, string buttonText, Action onButtonClick)
        {
            var modalWindow = new ModalWindow
            {
                DataContext = new ModalWindowViewModel(title, message, buttonText, onButtonClick)
            };
            
            modalWindow.ShowDialog(_parentWindow);
        }
    }
} 