using ConnectDotsGame.Models;

namespace ConnectDotsGame.Navigation
{
    public interface INavigation
    {
        void RegisterView<TViewModel, TView>();
        void NavigateTo<TViewModel>(object? parameter = null);
        void GoBack();
    }
} 