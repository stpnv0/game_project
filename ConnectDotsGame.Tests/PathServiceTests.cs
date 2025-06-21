using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using ConnectDotsGame.Models;
using ConnectDotsGame.Services;
using NUnit.Framework;

namespace ConnectDotsGame.Tests;

[TestFixture]
public class PathServiceTests
{
    private PathService _pathService;
    private GameState _gameState;
    private Level _level;
    private Point _p1, _p2, _p3;

    [SetUp]
    public void Setup()
    {
        _pathService = new PathService();
        _p1 = new Point(1, 0, 0, Brushes.Red);
        _p2 = new Point(2, 0, 1);
        _p3 = new Point(3, 0, 2, Brushes.Red);
        _level = new Level { Id = 1, Name = "Test", Rows = 1, Columns = 3, Points = new List<Point> { _p1, _p2, _p3 }, Lines = new List<Line>() };
        _gameState = new GameState(_pathService) { Levels = new List<Level> { _level }, CurrentLevelIndex = 0 };
    }

    [Test]
    public void StartPath_SetsCurrentPath()
    {
        _pathService.StartPath(_gameState, _p1);
        Assert.That(_pathService.CurrentPathColor, Is.EqualTo(Brushes.Red));
        Assert.That(_pathService.CurrentPath.Count, Is.EqualTo(1));
        Assert.That(_pathService.LastSelectedPoint, Is.EqualTo(_p1));
        Assert.That(_p1.IsConnected, Is.True);
    }

    [Test]
    public void ContinuePath_AddsNeighborPoint()
    {
        _pathService.StartPath(_gameState, _p1);
        _pathService.ContinuePath(_gameState, _p2);
        Assert.That(_pathService.CurrentPath, Has.Member(_p2));
    }

    [Test]
    public void ContinuePath_DoesNotAddNonNeighbor()
    {
        _pathService.StartPath(_gameState, _p1);
        var farPoint = new Point(4, 0, 5);
        _pathService.ContinuePath(_gameState, farPoint);
        Assert.That(_pathService.CurrentPath, Has.No.Member(farPoint));
    }

    [Test]
    public void EndPath_CompletePath_ConnectsBothEnds()
    {
        _pathService.StartPath(_gameState, _p1);
        _pathService.ContinuePath(_gameState, _p2);
        _pathService.EndPath(_gameState, _p3);
        Assert.That(_p1.IsConnected, Is.True);
        Assert.That(_p3.IsConnected, Is.True);
        Assert.That(_level.Lines.Count, Is.GreaterThan(0));
    }

    [Test]
    public void CancelPath_ResetsCurrentPath()
    {
        _pathService.StartPath(_gameState, _p1);
        _pathService.CancelPath(_gameState);
        Assert.That(_pathService.CurrentPath, Is.Empty);
        Assert.That(_pathService.CurrentPathColor, Is.Null);
    }

    [Test]
    public void CheckCompletion_ReturnsTrue_WhenAllPairsConnected()
    {
        _p1.IsConnected = true;
        _p3.IsConnected = true;
        Assert.That(_pathService.CheckCompletion(_level), Is.True);
    }

    [Test]
    public void CheckCompletion_ReturnsFalse_WhenNotAllPairsConnected()
    {
        _p1.IsConnected = true;
        _p3.IsConnected = false;
        Assert.That(_pathService.CheckCompletion(_level), Is.False);
    }

    [Test]
    public void GetPointByPosition_ReturnsCorrectPoint()
    {
        var found = _pathService.GetPointByPosition(_level, 0, 1);
        Assert.That(found, Is.EqualTo(_p2));
    }

    [Test]
    public void GetPointByPosition_ReturnsNull_IfNotFound()
    {
        var found = _pathService.GetPointByPosition(_level, 5, 5);
        Assert.That(found, Is.Null);
    }
}
