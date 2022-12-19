#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;
using static Newtonsoft.Json.JsonConvert;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Controllers
{
    [Collection(CacheServiceTests)]
    public class PublicationControllerTests : CacheServiceTestFixture
    {
        [Fact]
        public async Task ListLatestReleaseSubjects()
        {
            var subjects = new List<SubjectViewModel>();

            var latestReleaseId = Guid.NewGuid();

            var publication = new Publication
            {
                Id = Guid.NewGuid(),
                LatestPublishedReleaseId = latestReleaseId
            };

            var (controller, mocks) = BuildControllerAndMocks();

            var cacheKey = new ReleaseSubjectsCacheKey("publication", "release", latestReleaseId);

            SetupCall(mocks.contentPersistenceHelper, publication.Id, publication);

            mocks
                .cacheKeyService
                .Setup(s => s.CreateCacheKeyForReleaseSubjects(latestReleaseId))
                .ReturnsAsync(cacheKey);

            mocks.cacheService
                .Setup(s => s.GetItem(cacheKey, typeof(List<SubjectViewModel>)))
                .ReturnsAsync(null);

            mocks
                .releaseService
                .Setup(s => s.ListSubjects(latestReleaseId))
                .ReturnsAsync(subjects);

            mocks.cacheService
                .Setup(s => s.SetItem<object>(cacheKey, subjects))
                .Returns(Task.CompletedTask);

            var result = await controller.ListLatestReleaseSubjects(publication.Id);
            VerifyAllMocks(mocks);

            result.AssertOkResult(subjects);
        }

        [Fact]
        public async Task ListLatestReleaseSubjects_PublicationHasNoPublishedRelease()
        {
            var publication = new Publication
            {
                Id = Guid.NewGuid(),
                LatestPublishedReleaseId = null
            };

            var (controller, mocks) = BuildControllerAndMocks();

            SetupCall(mocks.contentPersistenceHelper, publication.Id, publication);

            var result = await controller.ListLatestReleaseSubjects(publication.Id);
            VerifyAllMocks(mocks);

            result.AssertNotFoundResult();
        }

        [Fact]
        public async Task ListLatestReleaseFeaturedTables()
        {
            var latestReleaseId = Guid.NewGuid();

            var publication = new Publication
            {
                Id = Guid.NewGuid(),
                LatestPublishedReleaseId = latestReleaseId
            };

            var featuredTables = new List<FeaturedTableViewModel>
            {
                new(Guid.NewGuid(), "name", "description")
            };

            var (controller, mocks) = BuildControllerAndMocks();

            SetupCall(mocks.contentPersistenceHelper, publication.Id, publication);

            mocks.releaseService
                .Setup(s => s.ListFeaturedTables(latestReleaseId))
                .ReturnsAsync(featuredTables);

            var result = await controller.ListLatestReleaseFeaturedTables(publication.Id);
            VerifyAllMocks(mocks);

            result.AssertOkResult(featuredTables);
        }

        [Fact]
        public async Task ListLatestReleaseFeaturedTables_PublicationHasNoPublishedRelease()
        {
            var publication = new Publication
            {
                Id = Guid.NewGuid(),
                LatestPublishedReleaseId = null
            };

            var (controller, mocks) = BuildControllerAndMocks();

            SetupCall(mocks.contentPersistenceHelper, publication.Id, publication);

            var result = await controller.ListLatestReleaseFeaturedTables(publication.Id);
            VerifyAllMocks(mocks);

            result.AssertNotFoundResult();
        }

        [Fact]
        public void SubjectViewModel_SerializeAndDeserialize()
        {
            var original = new SubjectViewModel(
                Guid.NewGuid(),
                "Name",
                0,
                "Content",
                new TimePeriodLabels
                {
                    From = "2020",
                    To = "2022"
                },
                new List<string>
                {
                    "level1"
                },
                new FileInfo
                {
                    Created = DateTime.Now,
                    Id = Guid.NewGuid(),
                    Name = "Name",
                    Size = "1234",
                    Summary = "Summary",
                    Type = FileType.Ancillary,
                    FileName = "Filename",
                    UserName = "UserName"
                });

            var converted = DeserializeObject<SubjectViewModel>(SerializeObject(original));
            converted.AssertDeepEqualTo(original);
        }

        private (PublicationController controller,
            (
            Mock<IPersistenceHelper<ContentDbContext>> contentPersistenceHelper,
            Mock<IReleaseService> releaseService,
            Mock<ICacheKeyService> cacheKeyService,
            Mock<IBlobCacheService> cacheService
            ) mocks
            ) BuildControllerAndMocks()
        {
            var contentPersistenceHelper = MockPersistenceHelper<ContentDbContext>();
            var releaseService = new Mock<IReleaseService>(Strict);
            var cacheKeyService = new Mock<ICacheKeyService>(Strict);
            var controller = new PublicationController(
                contentPersistenceHelper.Object, releaseService.Object, cacheKeyService.Object);

            return (controller,
                (contentPersistenceHelper, releaseService, cacheKeyService, BlobCacheService));
        }
    }
}
