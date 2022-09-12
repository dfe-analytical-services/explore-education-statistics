#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers
{
    [Collection(CacheServiceTests)]
    public class ReleaseControllerTests : CacheServiceTestFixture
    {
        public ReleaseControllerTests()
        {
            MemoryCacheService
                .Setup(s => s.GetItem(
                    It.IsAny<IMemoryCacheKey>(), typeof(ReleaseViewModel)))
                .ReturnsAsync(null);

            MemoryCacheService
                .Setup(s => s.SetItem<object>(
                    It.IsAny<IMemoryCacheKey>(),
                    It.IsAny<ReleaseViewModel>(),
                    It.IsAny<MemoryCacheConfiguration>(),
                    null))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task GetLatestRelease()
        {
            var expectedResult = BuildReleaseViewModel();

            var releaseCacheService = new Mock<IReleaseCacheService>(MockBehavior.Strict);

            releaseCacheService.Setup(mock => mock.GetReleaseAndPublication("publication-a", null))
                .ReturnsAsync(expectedResult);

            var controller = BuildReleaseController(releaseCacheService: releaseCacheService.Object);
            var result = await controller.GetLatestRelease("publication-a");

            MockUtils.VerifyAllMocks(releaseCacheService);

            result.AssertOkResult(expectedResult);
        }

        [Fact]
        public async Task GetLatestRelease_NotFound()
        {
            var releaseCacheService = new Mock<IReleaseCacheService>(MockBehavior.Strict);

            releaseCacheService.Setup(mock => mock.GetReleaseAndPublication("publication-a", null))
                .ReturnsAsync(new NotFoundResult());

            var controller = BuildReleaseController(releaseCacheService: releaseCacheService.Object);
            var result = await controller.GetLatestRelease("publication-a");

            MockUtils.VerifyAllMocks(releaseCacheService);

            result.AssertNotFoundResult();
        }

        [Fact]
        public async Task GetRelease()
        {
            var expectedResult = BuildReleaseViewModel();

            var releaseCacheService = new Mock<IReleaseCacheService>(MockBehavior.Strict);

            releaseCacheService.Setup(mock => mock.GetReleaseAndPublication("publication-a", "2000"))
                .ReturnsAsync(expectedResult);

            var controller = BuildReleaseController(releaseCacheService: releaseCacheService.Object);
            var result = await controller.GetRelease("publication-a", "2000");

            MockUtils.VerifyAllMocks(releaseCacheService);

            result.AssertOkResult(expectedResult);
        }


        [Fact]
        public async Task GetRelease_NotFound()
        {
            var releaseCacheService = new Mock<IReleaseCacheService>(MockBehavior.Strict);

            releaseCacheService.Setup(mock => mock.GetReleaseAndPublication("publication-a", "2000"))
                .ReturnsAsync(new NotFoundResult());

            var controller = BuildReleaseController(releaseCacheService: releaseCacheService.Object);
            var result = await controller.GetRelease("publication-a", "2000");

            MockUtils.VerifyAllMocks(releaseCacheService);

            result.AssertNotFoundResult();
        }

        [Fact]
        public async Task GetLatestReleaseSummary()
        {
            var expectedResult = BuildReleaseSummaryViewModel();

            var releaseCacheService = new Mock<IReleaseCacheService>(MockBehavior.Strict);

            releaseCacheService.Setup(mock => mock.GetReleaseSummary(
                    "publication-a", null))
                .ReturnsAsync(expectedResult);

            var controller = BuildReleaseController(releaseCacheService: releaseCacheService.Object);
            var result = await controller.GetLatestReleaseSummary("publication-a");

            MockUtils.VerifyAllMocks(releaseCacheService);

            result.AssertOkResult(expectedResult);
        }

        [Fact]
        public async Task GetLatestReleaseSummary_NotFound()
        {
            var releaseCacheService = new Mock<IReleaseCacheService>(MockBehavior.Strict);

            releaseCacheService.Setup(mock => mock.GetReleaseSummary("publication-a", null))
                .ReturnsAsync(new NotFoundResult());

            var controller = BuildReleaseController(releaseCacheService: releaseCacheService.Object);
            var result = await controller.GetLatestReleaseSummary("publication-a");

            MockUtils.VerifyAllMocks(releaseCacheService);

            result.AssertNotFoundResult();
        }

        [Fact]
        public async Task GetReleaseSummary()
        {
            var expectedResult = BuildReleaseSummaryViewModel();

            var releaseCacheService = new Mock<IReleaseCacheService>(MockBehavior.Strict);

            releaseCacheService.Setup(mock => mock.GetReleaseSummary(
                    "publication-a", "2000"))
                .ReturnsAsync(expectedResult);

            var controller = BuildReleaseController(releaseCacheService: releaseCacheService.Object);
            var result = await controller.GetReleaseSummary("publication-a", "2000");

            MockUtils.VerifyAllMocks(releaseCacheService);

            result.AssertOkResult(expectedResult);
        }

        [Fact]
        public async Task GetReleaseSummary_NotFound()
        {
            var releaseCacheService = new Mock<IReleaseCacheService>(MockBehavior.Strict);

            releaseCacheService.Setup(mock => mock.GetReleaseSummary("publication-a", "2000"))
                .ReturnsAsync(new NotFoundResult());

            var controller = BuildReleaseController(releaseCacheService: releaseCacheService.Object);
            var result = await controller.GetReleaseSummary("publication-a", "2000");

            MockUtils.VerifyAllMocks(releaseCacheService);

            result.AssertNotFoundResult();
        }

        private static ReleaseViewModel BuildReleaseViewModel()
        {
            return new ReleaseViewModel(
                new CachedReleaseViewModel(Guid.NewGuid())
                {
                    Type = new ReleaseTypeViewModel
                    {
                        Title = "National Statistics"
                    }
                },
                new PublicationViewModel
                {
                    Releases = ListOf(new ReleaseTitleViewModel
                    {
                        Id = Guid.NewGuid()
                    })
                });
        }

        private static ReleaseSummaryViewModel BuildReleaseSummaryViewModel()
        {
            var releaseId = Guid.NewGuid();

            return new ReleaseSummaryViewModel(
                new CachedReleaseViewModel(releaseId)
                {
                    Type = new ReleaseTypeViewModel
                    {
                        Title = "National Statistics"
                    }
                },
                new PublicationViewModel());
        }

        private static ReleaseController BuildReleaseController(
            IReleaseCacheService? releaseCacheService = null,
            IReleaseService? releaseService = null
        )
        {
            return new(
                releaseCacheService ?? Mock.Of<IReleaseCacheService>(MockBehavior.Strict),
                releaseService ?? Mock.Of<IReleaseService>(MockBehavior.Strict)
            );
        }
    }
}
