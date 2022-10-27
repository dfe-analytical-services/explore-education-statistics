#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
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

            var publicationId = Guid.NewGuid();

            var release = new Release
            {
                Id = Guid.NewGuid()
            };

            var (controller, mocks) = BuildControllerAndMocks();

            var cacheKey = new ReleaseSubjectsCacheKey("publication", "release", release.Id);

            mocks
                .cacheKeyService
                .Setup(s => s.CreateCacheKeyForReleaseSubjects(release.Id))
                .ReturnsAsync(cacheKey);

            mocks.cacheService
                .Setup(s => s.GetItem(cacheKey, typeof(List<SubjectViewModel>)))
                .ReturnsAsync(null);

            mocks
                .releaseRepository
                .Setup(s => s.GetLatestPublishedRelease(publicationId))
                .ReturnsAsync(release);

            mocks
                .releaseService
                .Setup(s => s.ListSubjects(release.Id))
                .ReturnsAsync(subjects);

            mocks.cacheService
                .Setup(s => s.SetItem<object>(cacheKey, subjects))
                .Returns(Task.CompletedTask);

            var result = await controller.ListLatestReleaseSubjects(publicationId);
            VerifyAllMocks(mocks);

            result.AssertOkResult(subjects);
        }

        [Fact]
        public async Task ListLatestReleaseFeaturedTables()
        {
            var publicationId = Guid.NewGuid();

            var release = new Release
            {
                Id = Guid.NewGuid()
            };

            var featuredTables = new List<FeaturedTableViewModel>
            {
                new(Guid.NewGuid(), "name", "description")
            };

            var (controller, mocks) = BuildControllerAndMocks();

            mocks.releaseRepository
                .Setup(s => s.GetLatestPublishedRelease(publicationId))
                .ReturnsAsync(release);

            mocks.releaseService
                .Setup(s => s.ListFeaturedTables(release.Id))
                .ReturnsAsync(featuredTables);

            var result = await controller.ListLatestReleaseFeaturedTables(publicationId);
            VerifyAllMocks(mocks);

            result.AssertOkResult(featuredTables);
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
            Mock<IReleaseRepository> releaseRepository,
            Mock<IReleaseService> releaseService,
            Mock<ICacheKeyService> cacheKeyService,
            Mock<IBlobCacheService> cacheService
            ) mocks
            ) BuildControllerAndMocks()
        {
            var releaseRepository = new Mock<IReleaseRepository>(Strict);
            var releaseService = new Mock<IReleaseService>(Strict);
            var cacheKeyService = new Mock<ICacheKeyService>(Strict);
            var controller = new PublicationController(
                releaseRepository.Object, releaseService.Object, cacheKeyService.Object);

            return (controller, (releaseRepository, releaseService, cacheKeyService, BlobCacheService));
        }
    }
}
