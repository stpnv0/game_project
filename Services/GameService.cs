using System;
using ConnectDotsGame.Models;
using ConnectDotsGame.ViewModels;

namespace ConnectDotsGame.Services
{
    public class GameService : IGameService
    {
        private INavigation? _navigation;
        private readonly IModalService _modalService;
        private readonly IPathManager _pathManager;
        private readonly IGameStorageService _gameStorage;

        public GameService(IModalService modalService, IPathManager pathManager, IGameStorageService gameStorage)
        {
            _modalService = modalService ?? throw new ArgumentNullException(nameof(modalService));
            _pathManager = pathManager ?? throw new ArgumentNullException(nameof(pathManager));
            _gameStorage = gameStorage ?? throw new ArgumentNullException(nameof(gameStorage));
        }

        public void SetNavigation(INavigation navigation)
            {
            _navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
        }

        public void ResetAllPaths(GameState gameState)
            {
            foreach (var level in gameState.Levels)
            {
                foreach (var point in level.Points)
                    {
                    point.IsConnected = false;
                    }
                
                level.Lines.Clear();
                level.Paths.Clear();
        }

            if (gameState.CurrentLevel != null)
            {
                _pathManager.CancelPath(gameState);
            }
        }

        public void NextLevel(GameState gameState)
        {
            if (gameState.HasNextLevel)
            {
                ResetAllPaths(gameState);
                gameState.CurrentLevelIndex++;
        }
        }

        public bool CheckLevelCompletion(GameState gameState)
        {
            if (gameState.CurrentLevel == null)
                return false;

            bool allCompleted = _pathManager.CheckCompletion(gameState.CurrentLevel);
            
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
            SaveProgress(gameState);

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
                    NextLevel(gameState);
                                _navigation.NavigateTo<GameViewModel>(gameState);
                            }
                    );
                }

        public void LoadProgress(GameState gameState)
        {
            _gameStorage.LoadProgress(gameState.Levels);
            }

        public void SaveProgress(GameState gameState)
        {
            _gameStorage.SaveProgress(gameState.Levels);
        }
    }
} 
