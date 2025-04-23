namespace ConnectDotsGame.Navigation
{
    public class DummyNavigation : INavigation
    {
        public void GoBack()
        {
            // Ничего не делаем в режиме дизайна
        }

        public void NavigateTo<TViewModel>(object? parameter = null)
        {
            // Ничего не делаем в режиме дизайна
        }

        public void RegisterView<TViewModel, TView>()
        {
            // Ничего не делаем в режиме дизайна
        }
    }
} 