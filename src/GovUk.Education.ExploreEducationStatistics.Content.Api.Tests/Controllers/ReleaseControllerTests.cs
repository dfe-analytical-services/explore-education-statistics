#nullable enable
using System;
using System.Collections.Generic;
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
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers
{
    [Collection(CacheServiceTests)]
    public class ReleaseControllerTests : CacheServiceTestFixture
    {
        private const string PublicationSlug = "publication-a";
        private const string ReleaseSlug = "200";

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
            var methodologySummaries = new List<MethodologyVersionSummaryViewModel>();
            var publicationCacheViewModel = new PublicationCacheViewModel
            {
                Id = Guid.NewGuid()
            };
            var releaseCacheViewModel = BuildReleaseCacheViewModel();

            var methodologyCacheService = new Mock<IMethodologyCacheService>(MockBehavior.Strict);
            var publicationCacheService = new Mock<IPublicationCacheService>(MockBehavior.Strict);
            var releaseCacheService = new Mock<IReleaseCacheService>(MockBehavior.Strict);

            methodologyCacheService.Setup(mock => mock.GetSummariesByPublication(publicationCacheViewModel.Id))
                .ReturnsAsync(methodologySummaries);

            publicationCacheService.Setup(mock => mock.GetPublication(PublicationSlug))
                .ReturnsAsync(publicationCacheViewModel);

            releaseCacheService.Setup(mock => mock.GetRelease(PublicationSlug, null))
                .ReturnsAsync(releaseCacheViewModel);

            var controller = BuildReleaseController(
                methodologyCacheService: methodologyCacheService.Object,
                publicationCacheService: publicationCacheService.Object,
                releaseCacheService: releaseCacheService.Object);

            var result = await controller.GetLatestRelease(PublicationSlug);

            MockUtils.VerifyAllMocks(methodologyCacheService,
                publicationCacheService,
                releaseCacheService);

            result.AssertOkResult(new ReleaseViewModel(releaseCacheViewModel,
                new PublicationViewModel(publicationCacheViewModel, methodologySummaries)));
        }

        [Fact]
        public async Task GetLatestRelease_PublicationNotFound()
        {
            var publicationCacheService = new Mock<IPublicationCacheService>(MockBehavior.Strict);

            publicationCacheService.Setup(mock => mock.GetPublication(PublicationSlug))
                .ReturnsAsync(new NotFoundResult());

            var controller = BuildReleaseController(publicationCacheService: publicationCacheService.Object);

            var result = await controller.GetLatestRelease(PublicationSlug);

            MockUtils.VerifyAllMocks(publicationCacheService);

            result.AssertNotFoundResult();
        }

        [Fact]
        public async Task GetLatestRelease_ReleaseNotFound()
        {
            var methodologySummaries = new List<MethodologyVersionSummaryViewModel>();
            var publicationCacheViewModel = new PublicationCacheViewModel
            {
                Id = Guid.NewGuid()
            };

            var methodologyCacheService = new Mock<IMethodologyCacheService>(MockBehavior.Strict);
            var publicationCacheService = new Mock<IPublicationCacheService>(MockBehavior.Strict);
            var releaseCacheService = new Mock<IReleaseCacheService>(MockBehavior.Strict);

            methodologyCacheService.Setup(mock => mock.GetSummariesByPublication(publicationCacheViewModel.Id))
                .ReturnsAsync(methodologySummaries);

            publicationCacheService.Setup(mock => mock.GetPublication(PublicationSlug))
                .ReturnsAsync(publicationCacheViewModel);

            releaseCacheService.Setup(mock => mock.GetRelease(PublicationSlug, null))
                .ReturnsAsync(new NotFoundResult());

            var controller = BuildReleaseController(
                methodologyCacheService: methodologyCacheService.Object,
                publicationCacheService: publicationCacheService.Object,
                releaseCacheService: releaseCacheService.Object);

            var result = await controller.GetLatestRelease(PublicationSlug);

            MockUtils.VerifyAllMocks(methodologyCacheService,
                publicationCacheService,
                releaseCacheService);

            result.AssertNotFoundResult();
        }

        [Fact]
        public async Task GetRelease()
        {
            var methodologySummaries = new List<MethodologyVersionSummaryViewModel>();
            var publicationCacheViewModel = new PublicationCacheViewModel
            {
                Id = Guid.NewGuid()
            };
            var releaseCacheViewModel = BuildReleaseCacheViewModel();

            var methodologyCacheService = new Mock<IMethodologyCacheService>(MockBehavior.Strict);
            var publicationCacheService = new Mock<IPublicationCacheService>(MockBehavior.Strict);
            var releaseCacheService = new Mock<IReleaseCacheService>(MockBehavior.Strict);

            methodologyCacheService.Setup(mock => mock.GetSummariesByPublication(publicationCacheViewModel.Id))
                .ReturnsAsync(methodologySummaries);

            publicationCacheService.Setup(mock => mock.GetPublication(PublicationSlug))
                .ReturnsAsync(publicationCacheViewModel);

            releaseCacheService.Setup(mock => mock.GetRelease(PublicationSlug, ReleaseSlug))
                .ReturnsAsync(releaseCacheViewModel);

            var controller = BuildReleaseController(
                methodologyCacheService: methodologyCacheService.Object,
                publicationCacheService: publicationCacheService.Object,
                releaseCacheService: releaseCacheService.Object);

            var result = await controller.GetRelease(PublicationSlug, ReleaseSlug);

            MockUtils.VerifyAllMocks(methodologyCacheService,
                publicationCacheService,
                releaseCacheService);

            result.AssertOkResult(new ReleaseViewModel(releaseCacheViewModel,
                new PublicationViewModel(publicationCacheViewModel, methodologySummaries)));
        }

        [Fact]
        public async Task GetRelease_PublicationNotFound()
        {
            var publicationCacheService = new Mock<IPublicationCacheService>(MockBehavior.Strict);

            publicationCacheService.Setup(mock => mock.GetPublication(PublicationSlug))
                .ReturnsAsync(new NotFoundResult());

            var controller = BuildReleaseController(publicationCacheService: publicationCacheService.Object);

            var result = await controller.GetRelease(PublicationSlug, ReleaseSlug);

            MockUtils.VerifyAllMocks(publicationCacheService);

            result.AssertNotFoundResult();
        }

        [Fact]
        public async Task GetRelease_ReleaseNotFound()
        {
            var methodologySummaries = new List<MethodologyVersionSummaryViewModel>();
            var publicationCacheViewModel = new PublicationCacheViewModel
            {
                Id = Guid.NewGuid()
            };

            var methodologyCacheService = new Mock<IMethodologyCacheService>(MockBehavior.Strict);
            var publicationCacheService = new Mock<IPublicationCacheService>(MockBehavior.Strict);
            var releaseCacheService = new Mock<IReleaseCacheService>(MockBehavior.Strict);

            methodologyCacheService.Setup(mock => mock.GetSummariesByPublication(publicationCacheViewModel.Id))
                .ReturnsAsync(methodologySummaries);

            publicationCacheService.Setup(mock => mock.GetPublication(PublicationSlug))
                .ReturnsAsync(publicationCacheViewModel);

            releaseCacheService.Setup(mock => mock.GetRelease(PublicationSlug, ReleaseSlug))
                .ReturnsAsync(new NotFoundResult());

            var controller = BuildReleaseController(
                methodologyCacheService: methodologyCacheService.Object,
                publicationCacheService: publicationCacheService.Object,
                releaseCacheService: releaseCacheService.Object);

            var result = await controller.GetRelease(PublicationSlug, ReleaseSlug);

            MockUtils.VerifyAllMocks(methodologyCacheService,
                publicationCacheService,
                releaseCacheService);

            result.AssertNotFoundResult();
        }

        [Fact]
        public async Task GetLatestReleaseSummary()
        {
            var publicationCacheViewModel = new PublicationCacheViewModel();
            var releaseCacheViewModel = BuildReleaseCacheViewModel();

            var publicationCacheService = new Mock<IPublicationCacheService>(MockBehavior.Strict);
            var releaseCacheService = new Mock<IReleaseCacheService>(MockBehavior.Strict);

            publicationCacheService.Setup(mock => mock.GetPublication(
                    PublicationSlug))
                .ReturnsAsync(publicationCacheViewModel);

            releaseCacheService.Setup(mock => mock.GetRelease(
                    PublicationSlug, null))
                .ReturnsAsync(releaseCacheViewModel);

            var controller = BuildReleaseController(
                publicationCacheService: publicationCacheService.Object,
                releaseCacheService: releaseCacheService.Object);
            var result = await controller.GetLatestReleaseSummary(PublicationSlug);

            MockUtils.VerifyAllMocks(publicationCacheService, releaseCacheService);

            result.AssertOkResult(new ReleaseSummaryViewModel(
                releaseCacheViewModel, publicationCacheViewModel));
        }

        [Fact]
        public async Task GetLatestReleaseSummary_PublicationNotFound()
        {
            var publicationCacheService = new Mock<IPublicationCacheService>(MockBehavior.Strict);

            publicationCacheService.Setup(mock => mock.GetPublication(
                    PublicationSlug))
                .ReturnsAsync(new NotFoundResult());

            var controller = BuildReleaseController(publicationCacheService: publicationCacheService.Object);
            var result = await controller.GetLatestReleaseSummary(PublicationSlug);

            MockUtils.VerifyAllMocks(publicationCacheService);

            result.AssertNotFoundResult();
        }

        [Fact]
        public async Task GetLatestReleaseSummary_ReleaseNotFound()
        {
            var publicationCacheViewModel = new PublicationCacheViewModel();

            var publicationCacheService = new Mock<IPublicationCacheService>(MockBehavior.Strict);
            var releaseCacheService = new Mock<IReleaseCacheService>(MockBehavior.Strict);

            publicationCacheService.Setup(mock => mock.GetPublication(
                    PublicationSlug))
                .ReturnsAsync(publicationCacheViewModel);

            releaseCacheService.Setup(mock => mock.GetRelease(
                    PublicationSlug, null))
                .ReturnsAsync(new NotFoundResult());

            var controller = BuildReleaseController(
                publicationCacheService: publicationCacheService.Object,
                releaseCacheService: releaseCacheService.Object);
            var result = await controller.GetLatestReleaseSummary(PublicationSlug);

            MockUtils.VerifyAllMocks(publicationCacheService, releaseCacheService);

            result.AssertNotFoundResult();
        }

        [Fact]
        public async Task GetReleaseSummary()
        {
            var publicationCacheViewModel = new PublicationCacheViewModel();
            var releaseCacheViewModel = BuildReleaseCacheViewModel();

            var publicationCacheService = new Mock<IPublicationCacheService>(MockBehavior.Strict);
            var releaseCacheService = new Mock<IReleaseCacheService>(MockBehavior.Strict);

            publicationCacheService.Setup(mock => mock.GetPublication(
                    PublicationSlug))
                .ReturnsAsync(publicationCacheViewModel);

            releaseCacheService.Setup(mock => mock.GetRelease(
                    PublicationSlug, ReleaseSlug))
                .ReturnsAsync(releaseCacheViewModel);

            var controller = BuildReleaseController(
                publicationCacheService: publicationCacheService.Object,
                releaseCacheService: releaseCacheService.Object);
            var result = await controller.GetReleaseSummary(PublicationSlug, ReleaseSlug);

            MockUtils.VerifyAllMocks(publicationCacheService, releaseCacheService);

            result.AssertOkResult(new ReleaseSummaryViewModel(
                releaseCacheViewModel, publicationCacheViewModel));
        }

        [Fact]
        public async Task GetReleaseSummary_PublicationNotFound()
        {
            var publicationCacheService = new Mock<IPublicationCacheService>(MockBehavior.Strict);

            publicationCacheService.Setup(mock => mock.GetPublication(
                    PublicationSlug))
                .ReturnsAsync(new NotFoundResult());

            var controller = BuildReleaseController(publicationCacheService: publicationCacheService.Object);
            var result = await controller.GetReleaseSummary(PublicationSlug, ReleaseSlug);

            MockUtils.VerifyAllMocks(publicationCacheService);

            result.AssertNotFoundResult();
        }

        [Fact]
        public async Task GetReleaseSummary_ReleaseNotFound()
        {
            var publicationCacheViewModel = new PublicationCacheViewModel();

            var publicationCacheService = new Mock<IPublicationCacheService>(MockBehavior.Strict);
            var releaseCacheService = new Mock<IReleaseCacheService>(MockBehavior.Strict);

            publicationCacheService.Setup(mock => mock.GetPublication(
                    PublicationSlug))
                .ReturnsAsync(publicationCacheViewModel);

            releaseCacheService.Setup(mock => mock.GetRelease(
                    PublicationSlug, ReleaseSlug))
                .ReturnsAsync(new NotFoundResult());

            var controller = BuildReleaseController(
                publicationCacheService: publicationCacheService.Object,
                releaseCacheService: releaseCacheService.Object);
            var result = await controller.GetReleaseSummary(PublicationSlug, ReleaseSlug);

            MockUtils.VerifyAllMocks(publicationCacheService, releaseCacheService);

            result.AssertNotFoundResult();
        }

        private static ReleaseCacheViewModel BuildReleaseCacheViewModel()
        {
            return new ReleaseCacheViewModel(Guid.NewGuid());
        }

        private static ReleaseController BuildReleaseController(
            IMethodologyCacheService? methodologyCacheService = null,
            IPublicationCacheService? publicationCacheService = null,
            IReleaseCacheService? releaseCacheService = null,
            IReleaseService? releaseService = null
        )
        {
            return new(
                methodologyCacheService ?? Mock.Of<IMethodologyCacheService>(MockBehavior.Strict),
                publicationCacheService ?? Mock.Of<IPublicationCacheService>(MockBehavior.Strict),
                releaseCacheService ?? Mock.Of<IReleaseCacheService>(MockBehavior.Strict),
                releaseService ?? Mock.Of<IReleaseService>(MockBehavior.Strict)
            );
        }
    }
}
