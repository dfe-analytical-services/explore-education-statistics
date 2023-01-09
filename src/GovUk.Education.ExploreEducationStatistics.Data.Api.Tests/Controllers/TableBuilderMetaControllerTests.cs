#nullable enable
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Controllers
{
    [Collection(CacheServiceTests)]
    public class TableBuilderMetaControllerTests : CacheServiceTestFixture
    {
        private static readonly Guid ReleaseId = Guid.NewGuid();
        private static readonly Guid SubjectId = Guid.NewGuid();

        private static readonly ObservationQueryContext QueryContext = new()
        {
            SubjectId = SubjectId
        };

        [Fact]
        public async Task GetSubjectMeta_LatestRelease()
        {
            var contentRelease = new Content.Model.Release
            {
                Id = ReleaseId,
                Slug = "release-slug",
                Publication = new Publication
                {
                    Slug = "publication-slug"
                }
            };

            var releaseSubject = new ReleaseSubject
            {
                ReleaseId = ReleaseId,
                SubjectId = SubjectId
            };

            var subjectMetaViewModel = new SubjectMetaViewModel();

            var cacheKey = GetCacheKey(contentRelease, releaseSubject);

            var (controller, mocks) = BuildControllerAndMocks();

            SetupCall(mocks.contentPersistenceHelper, ReleaseId, contentRelease);

            mocks.cacheService
                .Setup(s => s.GetItem(cacheKey, typeof(SubjectMetaViewModel)))
                .ReturnsAsync(null);

            mocks.cacheService
                .Setup(s => s.SetItem<object>(cacheKey, subjectMetaViewModel))
                .Returns(Task.CompletedTask);

            mocks.releaseSubjectRepository
                .Setup(mock => mock.GetReleaseSubjectForLatestPublishedVersion(SubjectId))
                .ReturnsAsync(releaseSubject);

            mocks.subjectMetaService
                .Setup(s => s.GetSubjectMeta(releaseSubject))
                .ReturnsAsync(subjectMetaViewModel);

            var result = await controller.GetSubjectMeta(SubjectId);
            VerifyAllMocks(mocks);

            result.AssertOkResult(subjectMetaViewModel);
        }

        [Fact]
        public async Task GetSubjectMeta_SpecificRelease()
        {
            var contentRelease = new Content.Model.Release
            {
                Id = ReleaseId,
                Slug = "release-slug",
                Publication = new Publication
                {
                    Slug = "publication-slug"
                }
            };

            var releaseSubject = new ReleaseSubject
            {
                ReleaseId = ReleaseId,
                SubjectId = SubjectId
            };

            var subjectMetaViewModel = new SubjectMetaViewModel();

            var cacheKey = GetCacheKey(contentRelease, releaseSubject);

            var (controller, mocks) = BuildControllerAndMocks();

            SetupCall(mocks.contentPersistenceHelper, ReleaseId, contentRelease);
            SetupCall(mocks.statisticsPersistenceHelper, releaseSubject);

            mocks.cacheService
                .Setup(s => s.GetItem(cacheKey, typeof(SubjectMetaViewModel)))
                .ReturnsAsync(null);

            mocks.cacheService
                .Setup(s => s.SetItem<object>(cacheKey, subjectMetaViewModel))
                .Returns(Task.CompletedTask);

            mocks.subjectMetaService
                .Setup(s => s.GetSubjectMeta(releaseSubject))
                .ReturnsAsync(subjectMetaViewModel);

            var result = await controller.GetSubjectMeta(ReleaseId, SubjectId);
            VerifyAllMocks(mocks);

            result.AssertOkResult(subjectMetaViewModel);
        }

        [Fact]
        public async Task GetSubjectMeta_LatestRelease_ReleaseSubjectNotFound()
        {
            var (controller, mocks) = BuildControllerAndMocks();

            mocks.releaseSubjectRepository
                .Setup(mock => mock.GetReleaseSubjectForLatestPublishedVersion(SubjectId))
                .ReturnsAsync((ReleaseSubject?) null);

            var result = await controller.GetSubjectMeta(SubjectId);
            VerifyAllMocks(mocks);

            result.AssertNotFoundResult();
        }

        [Fact]
        public async Task GetSubjectMeta_SpecificRelease_ReleaseSubjectNotFound()
        {
            var (controller, mocks) = BuildControllerAndMocks();

            mocks.statisticsPersistenceHelper
                .Setup(mock => mock.CheckEntityExists(
                        It.IsAny<Func<IQueryable<ReleaseSubject>, IQueryable<ReleaseSubject>>>()
                    )
                )
                .ReturnsAsync(new NotFoundResult());

            var result = await controller.GetSubjectMeta(ReleaseId, SubjectId);
            VerifyAllMocks(mocks);

            result.AssertNotFoundResult();
        }

        [Fact]
        public async Task FilterSubjectMeta_LatestRelease()
        {
            var subjectMetaViewModel = new SubjectMetaViewModel();
            var cancellationToken = new CancellationTokenSource().Token;

            var (controller, mocks) = BuildControllerAndMocks();

            mocks.subjectMetaService
                .Setup(s => s.FilterSubjectMeta(null, QueryContext, cancellationToken))
                .ReturnsAsync(subjectMetaViewModel);

            var result = await controller.FilterSubjectMeta(QueryContext, cancellationToken);
            VerifyAllMocks(mocks);

            result.AssertOkResult(subjectMetaViewModel);
        }

        [Fact]
        public async Task FilterSubjectMeta_SpecificRelease()
        {
            var subjectMetaViewModel = new SubjectMetaViewModel();
            var cancellationToken = new CancellationTokenSource().Token;

            var (controller, mocks) = BuildControllerAndMocks();

            mocks.subjectMetaService
                .Setup(s => s.FilterSubjectMeta(ReleaseId, QueryContext, cancellationToken))
                .ReturnsAsync(subjectMetaViewModel);

            var result = await controller.FilterSubjectMeta(ReleaseId, QueryContext, cancellationToken);
            VerifyAllMocks(mocks);

            result.AssertOkResult(subjectMetaViewModel);
        }

        private static SubjectMetaCacheKey GetCacheKey(Content.Model.Release release, ReleaseSubject releaseSubject)
        {
            return new SubjectMetaCacheKey(release.Publication.Slug, release.Slug, releaseSubject.SubjectId);
        }

        private static (TableBuilderMetaController controller,
            (
            Mock<IPersistenceHelper<ContentDbContext>> contentPersistenceHelper,
            Mock<IPersistenceHelper<StatisticsDbContext>> statisticsPersistenceHelper,
            Mock<IReleaseSubjectRepository> releaseSubjectRepository,
            Mock<ISubjectMetaService> subjectMetaService,
            Mock<IBlobCacheService> cacheService) mocks)
            BuildControllerAndMocks()
        {
            var contentPersistenceHelper = MockPersistenceHelper<ContentDbContext>();
            var statisticsPersistenceHelper = MockPersistenceHelper<StatisticsDbContext>();
            var releaseSubjectRepository = new Mock<IReleaseSubjectRepository>(Strict);
            var subjectMetaService = new Mock<ISubjectMetaService>(Strict);

            var controller = new TableBuilderMetaController(contentPersistenceHelper.Object,
                statisticsPersistenceHelper.Object,
                releaseSubjectRepository.Object,
                subjectMetaService.Object);

            return (controller, (
                contentPersistenceHelper,
                statisticsPersistenceHelper,
                releaseSubjectRepository,
                subjectMetaService,
                BlobCacheService));
        }
    }
}
