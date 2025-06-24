using System.Collections.Generic;
using System.Linq;
using ConnectDotsGame.Models;
using ConnectDotsGame.Services;
using ConnectDotsGame.ViewModels;
using NUnit.Framework;

namespace ConnectDotsGame.Tests;

[TestFixture]
public class LevelSelectViewModelTests
{
    private DummyNavigation _navigation;
    private GameState _gameState;
    private LevelSelectViewModel _vm;

    [SetUp]
    public void Setup()
    {
        _navigation = new DummyNavigation();
        _gameState = new GameState(new DummyPathService())
        {
            Levels = new List<Level>
            {
                new Level { Id = 1, Name = "Level 1", WasEverCompleted = true },
                new Level { Id = 2, Name = "Level 2", WasEverCompleted = false },
                new Level { Id = 3, Name = "Level 3", WasEverCompleted = false }
            },
            CurrentLevelIndex = 0
        };
        _vm = new LevelSelectViewModel(_navigation, _gameState);
        _vm.UpdateLevels();
    }

    [Test]
    public void Levels_AreCorrectlyUnlocked()
    {
        // Первый открыт, второй открыт (следующий после завершённого), третий закрыт
        Assert.That(_vm.Levels[0].IsLocked, Is.False);
        Assert.That(_vm.Levels[1].IsLocked, Is.False);
        Assert.That(_vm.Levels[2].IsLocked, Is.True);
    }

    [Test]
    public void SelectLevelCommand_ChangesCurrentLevelIndex_IfUnlocked()
    {
        _vm.SelectLevelCommand.Execute(2); // Второй уровень (Id=2) открыт
        Assert.That(_gameState.CurrentLevelIndex, Is.EqualTo(1));
    }

    [Test]
    public void SelectLevelCommand_DoesNotChangeIndex_IfLocked()
    {
        _vm.SelectLevelCommand.Execute(3); // Третий уровень (Id=3) закрыт
        Assert.That(_gameState.CurrentLevelIndex, Is.Not.EqualTo(2));
    }

    [Test]
    public void BackToMainCommand_NavigatesToMainPage()
    {
        _vm.BackToMainCommand.Execute(null);
        Assert.That(_navigation.NavigatedToMain, Is.True);
    }

    // Dummy classes for test
    private class DummyNavigation : INavigation
    {
        public bool NavigatedToMain;
        public void RegisterView<TViewModel, TView>() { }
        public void NavigateTo<TViewModel>(object? parameter = null)
        {
            if (typeof(TViewModel).Name == "MainPageViewModel")
                NavigatedToMain = true;
        }
    }
    private class DummyPathService : IPathService
    {
        public string? CurrentPathId => null;
        public Avalonia.Media.IBrush? CurrentPathColor => null;
        public List<Point> CurrentPath => new();
        public Point? LastSelectedPoint => null;
        public void StartPath(GameState gameState, Point point) { }
        public void ContinuePath(GameState gameState, Point point) { }
        public void EndPath(GameState gameState, Point? endPoint = null) { }
        public void CancelPath(GameState gameState) { }
        public bool CheckCompletion(Level level) => false;
        public Point? GetPointByPosition(Level level, int row, int column) => null;
    }
}

