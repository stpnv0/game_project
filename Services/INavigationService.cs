namespace ConnectDotsGame.Services
{
    public interface INavigation
    {
        void RegisterView<TViewModel, TView>();
        void NavigateTo<TViewModel>(object? parameter = null);
    }
} 

