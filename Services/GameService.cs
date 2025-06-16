using System;
using ConnectDotsGame.Models;
using ConnectDotsGame.ViewModels;

namespace ConnectDotsGame.Services
{
    public class GameService : IGameService
    {
        private INavigation? _navigation;
        private readonly IModalService _modalService;
        private readonly IPathService _pathService;
        private readonly IGameStorageService _gameStorage;

        public GameService(IModalService modalService, IPathService pathService, IGameStorageService gameStorage)
        {
            _modalService = modalService ?? throw new ArgumentNullException(nameof(modalService));
            _pathService = pathService ?? throw new ArgumentNullException(nameof(pathService));
            _gameStorage = gameStorage ?? throw new ArgumentNullException(nameof(gameStorage));
        }

        // Устанавливает сервис навигации
        public void SetNavigation(INavigation navigation)
        {
            _navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
        }

        // Сбрасывает все пути на всех уровнях
        public void ResetAllPaths(GameState gameState)
        {
            foreach (var level in gameState.Levels)
            {
                level.Points.ForEach(p => p.IsConnected = false);
                level.Lines.Clear();
                level.Paths.Clear();
            }

            if (gameState.CurrentLevel != null)
            {
                _pathService.CancelPath(gameState);
            }
        }

        // Переходит к следующему уровню
        public void NextLevel(GameState gameState)
        {
            if (gameState.HasNextLevel)
            {
                ResetAllPaths(gameState);
                gameState.CurrentLevelIndex++;
            }
        }

        // Проверяет завершение уровня
        public bool CheckLevelCompletion(GameState gameState)
        {
            if (gameState.CurrentLevel == null)
                return false;

            bool isCompleted = _pathService.CheckCompletion(gameState.CurrentLevel);
            if (isCompleted)
                HandleLevelCompletion(gameState);
                
            return isCompleted;
        }

        // Обрабатывает успешное завершение уровня
        private void HandleLevelCompletion(GameState gameState)
        {
            if (!gameState.CurrentLevel!.WasEverCompleted)
            {
                gameState.CurrentLevel!.WasEverCompleted = true;
                SaveProgress(gameState);
            }

            if (gameState.HasNextLevel)
                ShowLevelCompletionModal(gameState);
            else
                ShowGameCompletionModal();
        }

        // Модальное окно завершения игры
        private void ShowGameCompletionModal() =>
            ShowModal(
                "Поздравляем!",
                "Вы прошли все уровни!",
                "В меню",
                () => _navigation!.NavigateTo<LevelSelectViewModel>()
            );

        // Модальное окно завершения уровня
        private void ShowLevelCompletionModal(GameState gameState) =>
            ShowModal(
                "Уровень завершён!",
                $"Вы успешно завершили {gameState.CurrentLevel!.Name}",
                "Следующий уровень",
                () =>
                {
                    NextLevel(gameState);
                    _navigation!.NavigateTo<GameViewModel>(gameState);
                }
            );

        // Метод для показа модальных окон
        private void ShowModal(string title, string message, string buttonText, Action action)
        {
            _modalService.ShowModal(title, message, buttonText, action);
        }

        // Загружает прогресс
        public void LoadProgress(GameState gameState) =>
            _gameStorage.LoadProgress(gameState.Levels);

        // Сохраняет прогресс
        public void SaveProgress(GameState gameState) =>
            _gameStorage.SaveProgress(gameState.Levels);
    }
} 
