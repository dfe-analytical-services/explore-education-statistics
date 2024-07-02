#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Statistics;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Statistics;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Statistics;
public class BoundaryDataControllerTests
{
    private readonly Mock<IBoundaryDataService> _boundaryDataService;
    private readonly Mock<IBoundaryLevelService> _boundaryLevelService;
    private readonly BoundaryDataController _sut;

    public BoundaryDataControllerTests()
    {
        _boundaryDataService = new Mock<IBoundaryDataService>(MockBehavior.Strict);
        _boundaryLevelService = new Mock<IBoundaryLevelService>(MockBehavior.Strict);

        _sut = new(_boundaryDataService.Object, _boundaryLevelService.Object);
    }

    [Fact]
    public async Task GetBoundaryLevels_ReturnsOk()
    {
        // Arrange
        var boundaryLevels = new List<BoundaryLevelViewModel>
        {
            new() {Id = 1, Level = "School", Label = "Test", Published = DateTime.UtcNow},
        };

        _boundaryLevelService
            .Setup(bls => bls.ListBoundaryLevels())
            .ReturnsAsync(boundaryLevels);

        // Act
        var result = await _sut.GetBoundaryLevels();

        // Assert
        Assert.IsAssignableFrom<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetBoundaryLevel_MissingId_ReturnsBadRequest()
    {
        // Act
        var result = await _sut.GetBoundaryLevel(0);

        // Assert
        Assert.IsAssignableFrom<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetBoundaryLevel_ReturnsOk()
    {
        // Arrange
        var boundaryLevel = new BoundaryLevelViewModel { Id = 1, Level = "School", Label = "Test", Published = DateTime.UtcNow };

        _boundaryLevelService
            .Setup(bls => bls.GetBoundaryLevel(1))
            .ReturnsAsync(boundaryLevel);

        // Act
        var result = await _sut.GetBoundaryLevel(1);

        // Assert
        Assert.IsAssignableFrom<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateBoundaryLevel_ReturnsOk()
    {
        // Arrange
        _boundaryLevelService
            .Setup(bls => bls.UpdateBoundaryLevel(1, "Test (updated)"))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateBoundaryLevel(new() { Id = 1, Label = "Test (updated)" });

        // Assert
        Assert.IsAssignableFrom<NoContentResult>(result);
    }

    [Fact]
    public async Task CreateBoundaryLevel_InvalidFileExtension_ReturnsBadRequest()
    {
        // Arrange
        var geoJsonFile = MockFormTestUtils.CreateFormFileMock("boundaries.json", "application/octet-stream").Object;

        // Act
        var response = await _sut.CreateBoundaryLevel(
            GeographicLevel.School,
            "Boundary Data 1",
            geoJsonFile,
            DateTime.UtcNow);

        // Assert
        Assert.IsAssignableFrom<BadRequestObjectResult>(response);
        Assert.Equal("Invalid file format. This function only accepts GeoJSON files", (response as BadRequestObjectResult).Value);
    }
}
