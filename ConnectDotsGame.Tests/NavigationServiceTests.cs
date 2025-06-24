using System;
using Avalonia.Controls;
using ConnectDotsGame.Models;
using ConnectDotsGame.Services;
using ConnectDotsGame.ViewModels;
using NUnit.Framework;

namespace ConnectDotsGame.Tests;

[TestFixture]
public class NavigationServiceTests
{
    private ContentControl _contentControl;
    private DummyModalService _modalService;
    private GameState _gameState;
    private DummyGameService _gameService;
    private DummyPathService _pathService;
    private NavigationService _navigationService;

    [SetUp]
    public void Setup()
    {
        _contentControl = new ContentControl();
        _modalService = new DummyModalService();
        _gameState = new GameState(new DummyPathService());
        _gameService = new DummyGameService();
        _pathService = new DummyPathService();
        _navigationService = new NavigationService(_contentControl, _modalService, _gameState, _gameService, _pathService);
    }

    [Test]
    public void RegisterView_And_NavigateTo_SetsContentControlContent()
    {
        _navigationService.RegisterView<TestViewModel, TestView>();
        _navigationService.NavigateTo<TestViewModel>();
        Assert.That(_contentControl.Content, Is.InstanceOf<TestView>());
        Assert.That(((TestView)_contentControl.Content!).DataContext, Is.InstanceOf<TestViewModel>());
    }

    [Test]
    public void NavigateTo_GameViewModel_SetsGameViewModelWithDependencies()
    {
        _navigationService.RegisterView<GameViewModel, TestView>();
        _navigationService.NavigateTo<GameViewModel>();
        Assert.That(_contentControl.Content, Is.InstanceOf<TestView>());
        Assert.That(((TestView)_contentControl.Content!).DataContext, Is.InstanceOf<GameViewModel>());
    }

    [Test]
    public void NavigateTo_LevelSelectViewModel_CallsUpdateLevels()
    {
        _navigationService.RegisterView<LevelSelectViewModel, TestView>();
        _navigationService.NavigateTo<LevelSelectViewModel>();
        Assert.That(_contentControl.Content, Is.InstanceOf<TestView>());
        Assert.That(((TestView)_contentControl.Content!).DataContext, Is.InstanceOf<LevelSelectViewModel>());
    }

    // Dummy classes for test
    private class DummyModalService : IModalService
    {
        public void ShowModal(string title, string message, string buttonText, Action onButtonClick) { }
    }
    private class DummyGameService : IGameService
    {
        public void SetNavigation(INavigation navigation) { }
        public void ResetAllPaths(GameState gameState) { }
        public void NextLevel(GameState gameState) { }
        public bool CheckLevelCompletion(GameState gameState) => false;
        public void LoadProgress(GameState gameState) { }
        public void SaveProgress(GameState gameState) { }
    }
    private class DummyPathService : IPathService
    {
        public string? CurrentPathId => null;
        public Avalonia.Media.IBrush? CurrentPathColor => null;
        public System.Collections.Generic.List<Point> CurrentPath => new();
        public Point? LastSelectedPoint => null;
        public void StartPath(GameState gameState, Point point) { }
        public void ContinuePath(GameState gameState, Point point) { }
        public void EndPath(GameState gameState, Point? endPoint = null) { }
        public void CancelPath(GameState gameState) { }
        public bool CheckCompletion(Level level) => false;
        public Point? GetPointByPosition(Level level, int row, int column) => null;
    }
    public class TestViewModel { }
    public class TestView : Control { }
}

