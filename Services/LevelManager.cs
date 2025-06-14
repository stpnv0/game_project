using ConnectDotsGame.Models;
using ConnectDotsGame.ViewModels;

namespace ConnectDotsGame.Services
{
    public class LevelManager : ILevelManager
    {
        private readonly INavigation _navigation;
        private readonly IModalService _modalService;

        public LevelManager(INavigation navigation, IModalService modalService)
        {
            _navigation = navigation;
            _modalService = modalService;
        }

        public void ResetLevel(GameState gameState)
        {
            gameState.ResetCurrentLevel();
        }

        public void NextLevel(GameState gameState)
        {
            gameState.GoToNextLevel();
        }

        public bool CheckLevelCompletion(GameState gameState)
        {
            if (gameState.CurrentLevel == null)
                return false;

            bool allCompleted = gameState.CurrentLevel.CheckCompletion();
            if (allCompleted)
            {
                HandleLevelCompletion(gameState);
            }
            return allCompleted;
        }

        private void HandleLevelCompletion(GameState gameState)
        {
            gameState.CurrentLevel!.WasEverCompleted = true;
            gameState.SaveProgress();

            if (!gameState.HasNextLevel)
            {
                ShowGameCompletionModal();
            }
            else
            {
                ShowLevelCompletionModal(gameState);
            }
        }

        private void ShowGameCompletionModal()
        {
            _modalService.ShowModal(
                "Поздравляем!",
                "Вы прошли все уровни!",
                "В меню",
                () => _navigation.NavigateTo<LevelSelectViewModel>()
            );
        }

        private void ShowLevelCompletionModal(GameState gameState)
        {
            _modalService.ShowModal(
                "Уровень завершён!",
                $"Вы успешно завершили {gameState.CurrentLevel!.Name}",
                "Следующий уровень",
                () =>
                {
                    if (gameState.GoToNextLevel())
                    {
                        _navigation.NavigateTo<GameViewModel>(gameState);
                    }
                }
            );
        }
    }
} 