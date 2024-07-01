#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Repository;
public sealed class BoundaryLevelRepositoryTests : IDisposable
{
    private readonly DbContextOptions<StatisticsDbContext> _contextOptions;
    private readonly SqliteConnection _connection;
    private readonly StatisticsDbContext _context;
    private readonly BoundaryLevelRepository _sut;

    private static readonly BoundaryLevel BoundaryLevel1 = new()
    {
        Level = GeographicLevel.School,
        Label = "Boundary Level 1",
        Published = DateTime.Parse("2022-01-01", CultureInfo.InvariantCulture)
    };

    private static readonly BoundaryLevel BoundaryLevel2 = new()
    {
        Level = GeographicLevel.School,
        Label = "Boundary Level 2",
        Published = DateTime.Parse("2023-01-01", CultureInfo.InvariantCulture)
    };

    private static readonly BoundaryLevel BoundaryLevel3 = new()
    {
        Level = GeographicLevel.LocalAuthority,
        Label = "Boundary Level 3",
        Published = DateTime.Parse("2024-01-01", CultureInfo.InvariantCulture)
    };

    public BoundaryLevelRepositoryTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _contextOptions = new DbContextOptionsBuilder<StatisticsDbContext>()
        .UseSqlite(_connection)
            .Options;

        _connection.Open();
        _context = new StatisticsDbContext(_contextOptions);
        _context.Database.EnsureCreatedAsync();

        _sut = new BoundaryLevelRepository(_context);
    }

    [Fact]
    public async Task ListBoundaryLevels_NoItems_ReturnsEmptyList()
    {
        // Act
        var result = await _sut.ListBoundaryLevels();

        // Assert
        Assert.Empty(result);
    }

    //[Fact]
    //public async Task ListBoundaryLevels_ReturnsList()
    //{
    //    // Arrange
    //    await _context.BoundaryLevel.AddRangeAsync(
    //        BoundaryLevel1,
    //        BoundaryLevel2
    //    );

    //    await _context.SaveChangesAsync();

    //    // Act
    //    var result = await _sut.ListBoundaryLevels();

    //    // Assert
    //    var items = result.ToList();
    //    Assert.Equal(2, result.Count());
    //    Assert.Equal("Boundary Level 1", items[0].Label);
    //    Assert.Equal("Boundary Level 2", items[1].Label);
    //}

    [Fact]
    public async Task GetBoundaryLevel_NotFound_ReturnsNull()
    {
        // Act
        var result = await _sut.GetBoundaryLevel(1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetBoundaryLevel_ReturnsCorrectItem()
    {
        // Arrange
        var level = await _context.BoundaryLevel.AddAsync(BoundaryLevel1);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetBoundaryLevel(level.Entity.Id);

        // Assert
        Assert.Equal("Boundary Level 1", result.Label);
    }

    [Fact]
    public async Task UpdateBoundaryLevel_NoMatchFound_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(async () => await _sut.UpdateBoundaryLevel(1, ""));
    }

    //[Fact]
    //public async Task UpdateBoundaryLevel_ReturnsUpdatedItem()
    //{
    //    // Arrange
    //    var level = await _context.BoundaryLevel.AddAsync(BoundaryLevel1);
    //    await _context.SaveChangesAsync();

    //    // Act & Assert
    //    await Assert.IsAssignableFrom<Task>(async () => await _sut.UpdateBoundaryLevel(level.Entity.Id, "Boundary Level 1 (UPDATED)"));
    //}

    //[Fact]
    //public async Task FindByGeographicLevels_ReturnsOrderedList()
    //{
    //    // Arrange
    //    await _context.BoundaryLevel.AddRangeAsync(BoundaryLevel1, BoundaryLevel2, BoundaryLevel3);
    //    await _context.SaveChangesAsync();

    //    // Act
    //    var result = _sut.FindByGeographicLevels([GeographicLevel.School]);

    //    // Assert
    //    var resultList = result.ToList();
    //    Assert.Equal(2, result.Count());
    //    Assert.Equal("Boundary Level 2", resultList[0].Label);
    //    Assert.Equal("Boundary Level 1", resultList[1].Label);
    //}

    [Fact]
    public async Task CreateBoundaryLevel_ReturnsNewItem()
    {
        // Act
        var result = await _sut.CreateBoundaryLevel(GeographicLevel.LocalAuthority, "New Boundary Level", DateTime.UtcNow);

        // Assert
        Assert.Equal(GeographicLevel.LocalAuthority, result.Level);
        Assert.Equal("New Boundary Level", result.Label);
        Assert.Equal(DateTime.UtcNow, result.Created, TimeSpan.FromSeconds(1));
        Assert.Equal(DateTime.UtcNow, result.Published, TimeSpan.FromSeconds(1));
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}
