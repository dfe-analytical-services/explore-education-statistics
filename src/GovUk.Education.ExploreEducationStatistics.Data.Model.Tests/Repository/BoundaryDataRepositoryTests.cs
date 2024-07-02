#nullable enable
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Repository;
public sealed class BoundaryDataRepositoryTests : IDisposable
{
    private readonly DbContextOptions<StatisticsDbContext> _contextOptions;
    private readonly SqliteConnection _connection;
    private readonly StatisticsDbContext _context;
    private readonly BoundaryDataRepository _sut;

    public BoundaryDataRepositoryTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _contextOptions = new DbContextOptionsBuilder<StatisticsDbContext>()
            .UseSqlite(_connection)
            .Options;

        _connection.Open();
        _context = new StatisticsDbContext(_contextOptions);
        _context.Database.EnsureCreated();

        _sut = new BoundaryDataRepository(_context);
    }

    //[Fact]
    //public async Task AddRange()
    //{
    //    // Arrange
    //    var boundaryLevel1 = new BoundaryLevel { Level = GeographicLevel.School, Label = "Boundary Level 1", Published = DateTime.UtcNow };

    //    // Act
    //    await _sut.AddRange([
    //        new BoundaryData { BoundaryLevel = boundaryLevel1, Name = "Boundary Data 1", Code = "E06000001", GeoJson = "{}" },
    //        new BoundaryData { BoundaryLevel = boundaryLevel1, Name = "Boundary Data 2", Code = "E06000001", GeoJson = "{}" }
    //    ]);

    //    // Assert
    //    var data = await _context.BoundaryData.ToListAsync();
    //    Assert.Equal(2, data.Count);
    //    Assert.Equal("Boundary Data 1", data[0].Name);
    //    Assert.Equal("Boundary Data 2", data[1].Name);
    //}

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}
