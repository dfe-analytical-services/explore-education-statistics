using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Statistics;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;
public class BoundaryLevelServiceTests
{
    private readonly Mock<IBoundaryLevelRepository> _boundaryLevelRepository;
    private readonly BoundaryLevelService _sut;

    private static readonly List<BoundaryLevel> BoundaryLevels = [
        new BoundaryLevel{ Id = 1, Level = GeographicLevel.School, Label = "Boundary Level 1", Created = DateTime.Parse("2022-01-01", CultureInfo.InvariantCulture), Published = DateTime.Parse("2022-01-01", CultureInfo.InvariantCulture) },
        new BoundaryLevel{ Id = 2, Level = GeographicLevel.School, Label = "Boundary Level 2", Created = DateTime.Parse("2023-01-01", CultureInfo.InvariantCulture), Published = DateTime.Parse("2023-01-01", CultureInfo.InvariantCulture) },
        new BoundaryLevel{ Id = 3, Level = GeographicLevel.LocalAuthority, Label = "Boundary Level 3", Created = DateTime.Parse("2024-01-01", CultureInfo.InvariantCulture), Published = DateTime.Parse("2024-01-01", CultureInfo.InvariantCulture) },
    ];

    public BoundaryLevelServiceTests()
    {
        _boundaryLevelRepository = new Mock<IBoundaryLevelRepository>(MockBehavior.Strict);
        _sut = new(_boundaryLevelRepository.Object);
    }

    [Fact]
    public async Task Get_ReturnsListOfViewModels()
    {
        // Arrange
        _boundaryLevelRepository
            .Setup(blr => blr.Get())
            .ReturnsAsync(BoundaryLevels);

        // Act
        var results = await _sut.Get();

        // Assert
        Assert.IsType<List<BoundaryLevelViewModel>>(results);
        Assert.Equal(3, results.Count);

        Assert.Equal(1, results[0].Id);
        Assert.Equal("Boundary Level 1", results[0].Label);
        Assert.Equal("SCH", results[0].Level);
        Assert.Equal(DateTime.Parse("2022-01-01", CultureInfo.InvariantCulture), results[0].Published);
    }

    [Fact]
    public async Task Get_Single_ReturnsViewModel()
    {
        // Arrange
        _boundaryLevelRepository
            .Setup(blr => blr.Get(1))
            .ReturnsAsync(BoundaryLevels[0]);

        // Act
        var result = await _sut.Get(1);

        // Assert
        Assert.Equal(1, result.Id);
        Assert.Equal("Boundary Level 1", result.Label);
        Assert.Equal("SCH", result.Level);
        Assert.Equal(DateTime.Parse("2022-01-01", CultureInfo.InvariantCulture), result.Published);
    }

    [Fact]
    public async Task UpdateLabel_NoId_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await _sut.UpdateLabel(default, ""));
    }

    [Fact]
    public async Task UpdateLabel_NoLabel_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await _sut.UpdateLabel(1, ""));
    }

    //[Fact]
    //public async Task UpdateLabel_ReturnsUpdatedItem()
    //{
    //    // Arrange
    //    _boundaryLevelRepository
    //        .Setup(blr => blr.Update(
    //            It.IsAny<long>(),
    //            It.IsAny<string>()))
    //        .Returns(Task.CompletedTask);

    //    // Act & Assert
    //    await Assert.IsAssignableFrom<Task>(async () => await _sut.UpdateLabel(1, "Boundary Level 1 (UPDATED)")).;
    //}
}
