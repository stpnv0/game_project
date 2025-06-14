using System;
using ConnectDotsGame.Models;
using ConnectDotsGame.ViewModels;

namespace ConnectDotsGame.Services
{
    public class GameService : IGameService
    {
        private INavigation? _navigation;
        private readonly IModalService _modalService;

        public GameService(IModalService modalService)
        {
            _modalService = modalService ?? throw new ArgumentNullException(nameof(modalService));
        }

        public void SetNavigation(INavigation navigation)
        {
            _navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
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
            if (_navigation == null)
                throw new InvalidOperationException("Navigation service is not set");

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
            if (_navigation == null)
                throw new InvalidOperationException("Navigation service is not set");

                    _modalService.ShowModal(
                        "Поздравляем!",
                        "Вы прошли все уровни!",
                        "В меню",
                        () => _navigation.NavigateTo<LevelSelectViewModel>()
                    );
                }

        private void ShowLevelCompletionModal(GameState gameState)
                {
            if (_navigation == null)
                throw new InvalidOperationException("Navigation service is not set");

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
