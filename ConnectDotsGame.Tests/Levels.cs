using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Avalonia.Media;
using ConnectDotsGame.Levels;
using ConnectDotsGame.Models;
using NUnit.Framework;

namespace ConnectDotsGame.Tests;

[TestFixture]
public class Levels
{
    private Dictionary<string, IBrush> _colorMap;

    [SetUp]
    public void Setup()
    {
        _colorMap = new Dictionary<string, IBrush>
        {
            { "Red", Brushes.Red },
            { "Blue", Brushes.Blue },
            { "Green", Brushes.Green },
            { "Yellow", Brushes.Yellow },
            { "Purple", Brushes.Purple },
            { "Orange", Brushes.Orange },
            { "Pink", Brushes.Pink },
            { "Cyan", Brushes.Cyan },
            { "Gray", Brushes.Gray }
        };
    }

    [Test]
    public void LevelDto_ToLevel_CreatesCorrectLevel()
    {
        var dto = new LevelDto
        {
            Id = 1,
            Name = "Test",
            Rows = 2,
            Columns = 2,
            ColorPoints = new List<ColorPointDto>
            {
                new ColorPointDto { Row = 0, Column = 0, Color = "Red" },
                new ColorPointDto { Row = 1, Column = 1, Color = "Blue" }
            }
        };
        var level = dto.ToLevel(_colorMap);
        Assert.That(level.Id, Is.EqualTo(1));
        Assert.That(level.Name, Is.EqualTo("Test"));
        Assert.That(level.Rows, Is.EqualTo(2));
        Assert.That(level.Columns, Is.EqualTo(2));
        Assert.That(level.Points.Count, Is.EqualTo(4));
        Assert.That(level.Points.Find(p => p.Row == 0 && p.Column == 0)?.Color, Is.EqualTo(Brushes.Red));
        Assert.That(level.Points.Find(p => p.Row == 1 && p.Column == 1)?.Color, Is.EqualTo(Brushes.Blue));
    }

    [Test]
    public void LevelDto_ToLevel_ThrowsOnUnknownColor()
    {
        var dto = new LevelDto
        {
            Id = 2,
            Name = "Test",
            Rows = 1,
            Columns = 1,
            ColorPoints = new List<ColorPointDto>
            {
                new ColorPointDto { Row = 0, Column = 0, Color = "UnknownColor" }
            }
        };
        Assert.Throws<InvalidOperationException>(() => dto.ToLevel(_colorMap));
    }

    [Test]
    public void LevelDto_ToLevel_ThrowsOnMissingPoint()
    {
        var dto = new LevelDto
        {
            Id = 3,
            Name = "Test",
            Rows = 1,
            Columns = 1,
            ColorPoints = new List<ColorPointDto>
            {
                new ColorPointDto { Row = 2, Column = 2, Color = "Red" }
            }
        };
        Assert.Throws<InvalidOperationException>(() => dto.ToLevel(_colorMap));
    }

    [Test]
    public void LevelLoader_LoadLevels_ThrowsIfFileNotFound()
    {
        var loader = new LevelLoader("not_existing_file.json");
        Assert.That(() => loader.LoadLevels(), Throws.TypeOf<FileNotFoundException>());
    }

    [Test]
    public void LevelLoader_LoadLevels_ThrowsIfJsonInvalid()
    {
        var path = System.IO.Path.GetTempFileName();
        File.WriteAllText(path, "{ invalid json }");
        var loader = new LevelLoader(path);
        Assert.That(() => loader.LoadLevels(), Throws.Exception);
        File.Delete(path);
    }

    [Test]
    public void LevelLoader_LoadLevels_LoadsValidLevels()
    {
        var json = "{\"levels\":[{\"id\":1,\"name\":\"Test\",\"rows\":2,\"columns\":2,\"colorPoints\":[{\"row\":0,\"column\":0,\"color\":\"Red\"}]}]}";
        var path = System.IO.Path.GetTempFileName();
        File.WriteAllText(path, json);
        var loader = new LevelLoader(path);
        var levels = loader.LoadLevels();
        Assert.That(levels.Count, Is.EqualTo(1));
        Assert.That(levels[0].Name, Is.EqualTo("Test"));
        File.Delete(path);
    }

    [Test]
    public void Level_Structure_IsCorrect()
    {
        var dto = new LevelDto
        {
            Id = 4,
            Name = "StructTest",
            Rows = 3,
            Columns = 3,
            ColorPoints = new List<ColorPointDto>
            {
                new ColorPointDto { Row = 0, Column = 0, Color = "Red" },
                new ColorPointDto { Row = 2, Column = 2, Color = "Blue" }
            }
        };
        var level = dto.ToLevel(_colorMap);
        Assert.That(level.Points.Count, Is.EqualTo(9));
        Assert.That(level.Lines, Is.Empty);
        Assert.That(level.Paths, Is.Empty);
        Assert.That(level.WasEverCompleted, Is.False);
    }

    [Test]
    public void LevelDto_ToLevel_AllPointsHaveUniqueIds()
    {
        var dto = new LevelDto
        {
            Id = 5,
            Name = "UniqueIdTest",
            Rows = 4,
            Columns = 4,
            ColorPoints = new List<ColorPointDto>
            {
                new ColorPointDto { Row = 0, Column = 0, Color = "Red" },
                new ColorPointDto { Row = 3, Column = 3, Color = "Blue" }
            }
        };
        var level = dto.ToLevel(_colorMap);
        var ids = new HashSet<int>();
        foreach (var p in level.Points)
            Assert.That(ids.Add(p.Id), Is.True, $"Duplicate id: {p.Id}");
    }

    [Test]
    public void LevelDto_ToLevel_NoColorPoints_AllTransparent()
    {
        var dto = new LevelDto
        {
            Id = 6,
            Name = "NoColorTest",
            Rows = 2,
            Columns = 2,
            ColorPoints = new List<ColorPointDto>()
        };
        var level = dto.ToLevel(_colorMap);
        Assert.That(level.Points.All(p => p.Color == Brushes.Transparent), Is.True);
    }

    [Test]
    public void LevelLoader_LoadLevels_IgnoresInvalidLevels()
    {
        // Некорректный уровень с неизвестным цветом, должен быть пропущен
        var json = "{\"levels\":[{\"id\":1,\"name\":\"Test\",\"rows\":2,\"columns\":2,\"colorPoints\":[{\"row\":0,\"column\":0,\"color\":\"Red\"}]},{\"id\":2,\"name\":\"Bad\",\"rows\":1,\"columns\":1,\"colorPoints\":[{\"row\":0,\"column\":0,\"color\":\"UnknownColor\"}]}]}";
        var path = System.IO.Path.GetTempFileName();
        File.WriteAllText(path, json);
        var loader = new LevelLoader(path);
        var levels = loader.LoadLevels();
        Assert.That(levels.Count, Is.EqualTo(1));
        Assert.That(levels[0].Name, Is.EqualTo("Test"));
        File.Delete(path);
    }

    [Test]
    public void LevelDto_ToLevel_ThrowsIfColorPointsNull()
    {
        var dto = new LevelDto
        {
            Id = 7,
            Name = "NullColorPoints",
            Rows = 2,
            Columns = 2,
            ColorPoints = null!
        };
        Assert.Throws<NullReferenceException>(() => dto.ToLevel(_colorMap));
    }

    [Test]
    public void LevelDto_ToLevel_ThrowsIfColorMapNull()
    {
        var dto = new LevelDto
        {
            Id = 8,
            Name = "NullColorMap",
            Rows = 2,
            Columns = 2,
            ColorPoints = new List<ColorPointDto> { new ColorPointDto { Row = 0, Column = 0, Color = "Red" } }
        };
        Assert.Throws<ArgumentNullException>(() => dto.ToLevel(null!));
    }
}