using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ConnectDotsGame.Models;
using ConnectDotsGame.Services;
using NUnit.Framework;

namespace ConnectDotsGame.Tests;

[TestFixture]
public class GameStorageServiceTests
{
    private string _testDir;
    private string _savePath;
    private GameStorageService _service;

    [SetUp]
    public void Setup()
    {
        _testDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDir);
        _savePath = System.IO.Path.Combine(_testDir, "progress.json");
        Environment.SetEnvironmentVariable("LocalAppData", _testDir);
        _service = new GameStorageService();
    }

    [TearDown]
    public void Cleanup()
    {
        if (Directory.Exists(_testDir))
            Directory.Delete(_testDir, true);
    }

    [Test]
    public void SaveProgress_CreatesFileWithCorrectContent()
    {
        var levels = new List<Level> { new Level { Id = 1, WasEverCompleted = true }, new Level { Id = 2, WasEverCompleted = false } };
        _service.SaveProgress(levels);
        Assert.That(File.Exists(_savePath), Is.True);
        var json = File.ReadAllText(_savePath);
        Assert.That(json, Does.Contain("\"Id\":1"));
        Assert.That(json, Does.Contain("\"WasEverCompleted\":true"));
    }

    [Test]
    public void LoadProgress_UpdatesLevelsCorrectly()
    {
        var levels = new List<Level> { new Level { Id = 1 }, new Level { Id = 2 } };
        File.WriteAllText(_savePath, "[{\"Id\":1,\"WasEverCompleted\":true},{\"Id\":2,\"WasEverCompleted\":false}]");
        _service.LoadProgress(levels);
        Assert.That(levels[0].WasEverCompleted, Is.True);
        Assert.That(levels[1].WasEverCompleted, Is.False);
    }

    [Test]
    public void LoadProgress_DoesNothing_IfFileNotExists()
    {
        var levels = new List<Level> { new Level { Id = 1 }, new Level { Id = 2 } };
        if (File.Exists(_savePath)) File.Delete(_savePath);
        Assert.DoesNotThrow(() => _service.LoadProgress(levels));
        Assert.That(levels.All(l => l.WasEverCompleted == false), Is.True);
    }

    [Test]
    public void SaveProgress_WorksWithEmptyList()
    {
        var levels = new List<Level>();
        Assert.DoesNotThrow(() => _service.SaveProgress(levels));
        Assert.That(File.Exists(_savePath), Is.True);
    }
}
