using System;

namespace ConnectDotsGame.Services
{
    public interface IModalService
    {
        void ShowModal(string title, string message, string buttonText, Action onButtonClick);
    }
} 