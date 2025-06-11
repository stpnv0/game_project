using System; // Добавлено для использования Action
using ConnectDotsGame.Models;

namespace ConnectDotsGame.Services
{
    public interface INavigation
    {
        void RegisterView<TViewModel, TView>();
        void NavigateTo<TViewModel>(object? parameter = null);
        void GoBack();
        void ShowModal(string title, string message, string buttonText, Action onButtonClick);
    }
} 

