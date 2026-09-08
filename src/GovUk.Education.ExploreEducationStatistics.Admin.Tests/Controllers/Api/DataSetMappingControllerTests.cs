#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api;

public abstract class DataSetMappingControllerTests
{
    public class UpdateFilterMappingsTests : DataSetMappingControllerTests
    {
        [Fact]
        public async Task Success()
        {
            var releaseVersionId = Guid.NewGuid();
            var request = new FilterMappingUpdatesRequest
            {
                OriginalDataFileId = Guid.NewGuid(),
                ReplacementDataFileId = Guid.NewGuid(),
                FilterUpdates = [],
                FilterGroupUpdates = [],
                FilterItemUpdates = [],
            };

            var expectedDto = new FiltersMappingDto
            {
                Filters = [],
                FilterGroups = [],
                FilterItems = [],
            };

            var dataSetMappingService = new Mock<IDataSetMappingService>(MockBehavior.Strict);

            dataSetMappingService
                .Setup(s => s.UpdateFilterMappings(releaseVersionId, request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var controller = BuildController(dataSetMappingService: dataSetMappingService.Object);

            var result = await controller.UpdateFilterMappings(
                releaseVersionId: releaseVersionId,
                request: request,
                cancellationToken: default
            );

            MockUtils.VerifyAllMocks(dataSetMappingService);

            var returnedDto = result.AssertOkResult();
            Assert.Equal(expectedDto, returnedDto);
        }

        [Fact]
        public async Task ValidationProblem()
        {
            var releaseVersionId = Guid.NewGuid();
            var request = new FilterMappingUpdatesRequest();

            var dataSetMappingService = new Mock<IDataSetMappingService>(MockBehavior.Strict);

            dataSetMappingService
                .Setup(s => s.UpdateFilterMappings(releaseVersionId, request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new NotFoundResult());

            var controller = BuildController(dataSetMappingService: dataSetMappingService.Object);

            var result = await controller.UpdateFilterMappings(
                releaseVersionId: releaseVersionId,
                request: request,
                cancellationToken: default
            );

            MockUtils.VerifyAllMocks(dataSetMappingService);

            result.AssertNotFoundResult();
        }
    }

    private static DataSetMappingController BuildController(IDataSetMappingService? dataSetMappingService = null)
    {
        return new DataSetMappingController(
            dataSetMappingService ?? Mock.Of<IDataSetMappingService>(MockBehavior.Strict)
        );
    }
}
