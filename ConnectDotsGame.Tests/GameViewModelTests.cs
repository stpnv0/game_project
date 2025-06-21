using System.Collections.Generic;
using Avalonia.Media;
using ConnectDotsGame.Models;
using ConnectDotsGame.Services;
using ConnectDotsGame.ViewModels;
using NUnit.Framework;

namespace ConnectDotsGame.Tests;

[TestFixture]
public class GameViewModelTests
{
    private DummyNavigation _navigation;
    private DummyModalService _modalService;
    private DummyGameService _gameService;
    private DummyPathService _pathService;
    private GameState _gameState;
    private GameViewModel _vm;

    [SetUp]
    public void Setup()
    {
        _navigation = new DummyNavigation();
        _modalService = new DummyModalService();
        _gameService = new DummyGameService();
        _pathService = new DummyPathService();
        _gameState = new GameState(_pathService)
        {
            Levels = new List<Level> { new Level { Id = 1, Name = "Test", Rows = 2, Columns = 2 } },
            CurrentLevelIndex = 0
        };
        _vm = new GameViewModel(_navigation, _gameState, _modalService, _gameService, _pathService);
    }

    [Test]
    public void LevelName_ReturnsCurrentLevelName()
    {
        Assert.That(_vm.LevelName, Is.EqualTo("Test"));
    }

    [Test]
    public void StartPath_CallsPathService()
    {
        var point = new Point(1, 0, 0, Brushes.Red);
        _vm.StartPath(point);
        Assert.That(_pathService.StartPathCalled, Is.True);
    }

    [Test]
    public void ContinuePath_CallsPathService()
    {
        var point = new Point(1, 0, 0, Brushes.Red);
        _vm.ContinuePath(point);
        Assert.That(_pathService.ContinuePathCalled, Is.True);
    }

    [Test]
    public void EndPath_CallsPathService()
    {
        var point = new Point(1, 0, 0, Brushes.Red);
        _vm.EndPath(point);
        Assert.That(_pathService.EndPathCalled, Is.True);
    }

    [Test]
    public void ResetLevelCommand_ResetsPaths()
    {
        _vm.ResetLevelCommand.Execute(null);
        Assert.That(_gameService.ResetAllPathsCalled, Is.True);
    }

    [Test]
    public void NextLevelCommand_CallsGameService()
    {
        _vm.NextLevelCommand.Execute(null);
        Assert.That(_gameService.NextLevelCalled, Is.True);
    }

    [Test]
    public void PrevLevelCommand_ChangesCurrentLevelIndex()
    {
        _gameState.Levels.Add(new Level { Id = 2, Name = "Next", Rows = 2, Columns = 2 });
        _gameState.CurrentLevelIndex = 1;
        _vm.PrevLevelCommand.Execute(null);
        Assert.That(_gameState.CurrentLevelIndex, Is.EqualTo(0));
    }

    // Dummy classes for test
    private class DummyNavigation : INavigation
    {
        public void RegisterView<TViewModel, TView>() { }
        public void NavigateTo<TViewModel>(object? parameter = null) { }
    }
    private class DummyModalService : IModalService
    {
        public void ShowModal(string title, string message, string buttonText, System.Action onButtonClick) { }
    }
    private class DummyGameService : IGameService
    {
        public bool ResetAllPathsCalled, NextLevelCalled;
        public void SetNavigation(INavigation navigation) { }
        public void ResetAllPaths(GameState gameState) => ResetAllPathsCalled = true;
        public void NextLevel(GameState gameState) => NextLevelCalled = true;
        public bool CheckLevelCompletion(GameState gameState) => false;
        public void LoadProgress(GameState gameState) { }
        public void SaveProgress(GameState gameState) { }
    }
    private class DummyPathService : IPathService
    {
        public bool StartPathCalled, ContinuePathCalled, EndPathCalled;
        public string? CurrentPathId => null;
        public IBrush? CurrentPathColor => null;
        public List<Point> CurrentPath => new();
        public Point? LastSelectedPoint => null;
        public void StartPath(GameState gameState, Point point) => StartPathCalled = true;
        public void ContinuePath(GameState gameState, Point point) => ContinuePathCalled = true;
        public void EndPath(GameState gameState, Point? endPoint = null) => EndPathCalled = true;
        public void CancelPath(GameState gameState) { }
        public bool CheckCompletion(Level level) => false;
        public Point? GetPointByPosition(Level level, int row, int column) => null;
    }
}

