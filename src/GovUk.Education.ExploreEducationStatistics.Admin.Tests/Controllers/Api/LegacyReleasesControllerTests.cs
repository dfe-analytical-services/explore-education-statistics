#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api;

public class LegacyReleasesControllerTests
{
    private readonly Guid _releaseId = Guid.NewGuid();
    private readonly Guid _publicationId = Guid.NewGuid();
    private readonly Mock<ILegacyReleaseService> _legacyReleaseService;

    public LegacyReleasesControllerTests()
    {
        _legacyReleaseService = new Mock<ILegacyReleaseService>(Strict);
    }

    [Fact]
    public async Task GetReleaseSeriesView_ReturnsOk()
    {
        // Arrange
        var templateReleaseResult =
            new Either<ActionResult, List<ReleaseSeriesItemViewModel>>(new List<ReleaseSeriesItemViewModel>());

        _legacyReleaseService
            .Setup(s => s.GetReleaseSeriesView(It.Is<Guid>(id => id == _publicationId)))
            .ReturnsAsync(templateReleaseResult);

        var controller = BuildController(_legacyReleaseService.Object);

        // Act
        var result = await controller.GetReleaseSeriesView(_publicationId);

        // Assert
        VerifyAllMocks(_legacyReleaseService);
        result.AssertOkResult();
    }

    [Fact]
    public async Task GetLegacyRelease_ReturnsOk()
    {
        // Arrange
        var templateReleaseResult =
            new Either<ActionResult, LegacyReleaseViewModel>(new LegacyReleaseViewModel());

        _legacyReleaseService
            .Setup(s => s.GetLegacyRelease(It.Is<Guid>(id => id == _releaseId)))
            .ReturnsAsync(templateReleaseResult);

        var controller = BuildController(_legacyReleaseService.Object);

        // Act
        var result = await controller.GetLegacyRelease(_releaseId);

        // Assert
        VerifyAllMocks(_legacyReleaseService);
        result.AssertOkResult();
    }

    [Fact]
    public async Task CreateLegacyRelease_ReturnsOk()
    {
        // Arrange
        var returnedViewModel = new LegacyReleaseViewModel();

        _legacyReleaseService
            .Setup(s => s.CreateLegacyRelease(It.IsAny<LegacyReleaseCreateViewModel>()))
            .ReturnsAsync(returnedViewModel);

        var controller = BuildController(_legacyReleaseService.Object);

        // Act
        var result = await controller.CreateLegacyRelease(new LegacyReleaseCreateViewModel());

        // Assert
        VerifyAllMocks(_legacyReleaseService);
        result.AssertOkResult(returnedViewModel);
    }

    [Fact]
    public async Task UpdateLegacyRelease_ReturnsOk()
    {
        // Arrange
        _legacyReleaseService
            .Setup(s => s.UpdateLegacyRelease(
                It.Is<Guid>(id => id.Equals(_releaseId)),
                It.IsAny<LegacyReleaseUpdateViewModel>())
            )
            .ReturnsAsync(new LegacyReleaseViewModel { Id = _releaseId });

        var controller = BuildController(_legacyReleaseService.Object);

        // Act
        var result = await controller.UpdateLegacyRelease(_releaseId, new LegacyReleaseUpdateViewModel());

        // Assert
        VerifyAllMocks(_legacyReleaseService);
        var unboxed = result.AssertOkResult();
        Assert.Equal(_releaseId, unboxed.Id);
    }

    [Fact]
    public async Task DeleteLegacyRelease_ReturnsNoContent()
    {
        // Arrange
        _legacyReleaseService
            .Setup(service => service.DeleteLegacyRelease(_releaseId))
            .ReturnsAsync(true);

        var controller = BuildController(_legacyReleaseService.Object);

        // Act
        var result = await controller.DeleteLegacyRelease(_releaseId);

        // Assert
        VerifyAllMocks(_legacyReleaseService);
        Assert.IsAssignableFrom<NoContentResult>(result);
    }

    private static LegacyReleaseController BuildController(
        ILegacyReleaseService? legacyReleaseService = null)
    {
        return new LegacyReleaseController(
            legacyReleaseService ?? Mock.Of<ILegacyReleaseService>(Strict));
    }
}
