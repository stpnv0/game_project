using System;
using System.Collections.Generic;
using Avalonia.Media;
using ConnectDotsGame.Models;
using ConnectDotsGame.Services;
using NUnit.Framework;

namespace ConnectDotsGame.Tests;

[TestFixture]
public class GameServiceTests
{
    private class DummyModalService : IModalService
    {
        public bool WasShown { get; private set; }
        public void ShowModal(string title, string message, string buttonText, Action onButtonClick)
        {
            WasShown = true;
            onButtonClick?.Invoke();
        }
    }
    private class DummyPathService : IPathService
    {
        public string? CurrentPathId => null;
        public IBrush? CurrentPathColor => null;
        public List<Point> CurrentPath => new();
        public Point? LastSelectedPoint => null;
        public bool CompletionResult { get; set; } = false;
        public void StartPath(GameState gameState, Point point) { }
        public void ContinuePath(GameState gameState, Point point) { }
        public void EndPath(GameState gameState, Point? endPoint = null) { }
        public void CancelPath(GameState gameState) { }
        public bool CheckCompletion(Level level) => CompletionResult;
        public Point? GetPointByPosition(Level level, int row, int column) => null;
    }
    private class DummyGameStorageService : IGameStorageService
    {
        public bool LoadCalled { get; private set; }
        public bool SaveCalled { get; private set; }
        public void LoadProgress(List<Level> levels) => LoadCalled = true;
        public void SaveProgress(List<Level> levels) => SaveCalled = true;
    }
    private class DummyNavigation : INavigation
    {
        public bool Navigated { get; private set; }
        public void RegisterView<TViewModel, TView>() { }
        public void NavigateTo<TViewModel>(object? parameter = null) => Navigated = true;
    }
    private DummyModalService _modalService;
    private DummyPathService _pathService;
    private DummyGameStorageService _gameStorage;
    private DummyNavigation _navigation;
    private GameService _gameService;
    private GameState _gameState;

    [SetUp]
    public void Setup()
    {
        _modalService = new DummyModalService();
        _pathService = new DummyPathService();
        _gameStorage = new DummyGameStorageService();
        _navigation = new DummyNavigation();
        _gameService = new GameService(_modalService, _pathService, _gameStorage);
        _gameService.SetNavigation(_navigation);
        _gameState = new GameState(_pathService)
        {
            Levels = new List<Level> { new Level { Id = 1, Name = "Test", Rows = 2, Columns = 2 } },
            CurrentLevelIndex = 0
        };
    }

    [Test]
    public void ResetAllPaths_ResetsPointsLinesAndPaths()
    {
        var level = _gameState.Levels[0];
        level.Points.Add(new Point(1, 0, 0) { IsConnected = true });
        level.Lines.Add(new Line(new Point(1, 0, 0), new Point(2, 0, 1)));
        level.Paths["test"] = new List<Line> { new Line(new Point(1, 0, 0), new Point(2, 0, 1)) };
        _gameService.ResetAllPaths(_gameState);
        Assert.That(level.Points.TrueForAll(p => !p.IsConnected));
        Assert.That(level.Lines, Is.Empty);
        Assert.That(level.Paths, Is.Empty);
    }

    [Test]
    public void NextLevel_IncrementsCurrentLevelIndex_AndResetsPaths()
    {
        _gameState.Levels.Add(new Level { Id = 2, Name = "Next", Rows = 2, Columns = 2 });
        _gameState.CurrentLevelIndex = 0;
        _gameService.NextLevel(_gameState);
        Assert.That(_gameState.CurrentLevelIndex, Is.EqualTo(1));
    }

    [Test]
    public void CheckLevelCompletion_CallsHandleLevelCompletion_WhenCompleted()
    {
        _gameState.Levels[0].WasEverCompleted = false;
        _gameState.CurrentLevelIndex = 0;
        _pathService.CompletionResult = true;
        var result = _gameService.CheckLevelCompletion(_gameState);
        Assert.That(result, Is.True);
    }

    [Test]
    public void CheckLevelCompletion_ReturnsFalse_WhenNotCompleted()
    {
        _gameState.CurrentLevelIndex = 0;
        _pathService.CompletionResult = false;
        var result = _gameService.CheckLevelCompletion(_gameState);
        Assert.That(result, Is.False);
    }

    [Test]
    public void LoadProgress_DelegatesToGameStorage()
    {
        _gameService.LoadProgress(_gameState);
        Assert.That(_gameStorage.LoadCalled, Is.True);
    }

    [Test]
    public void SaveProgress_DelegatesToGameStorage()
    {
        _gameService.SaveProgress(_gameState);
        Assert.That(_gameStorage.SaveCalled, Is.True);
    }
}
