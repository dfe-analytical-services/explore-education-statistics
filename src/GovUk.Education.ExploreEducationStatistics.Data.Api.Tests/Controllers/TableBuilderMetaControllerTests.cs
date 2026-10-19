#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Requests;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Controllers;

public class TableBuilderMetaControllerTests
{
    private readonly DataFixture _dataFixture = new();

    private static readonly Guid ReleaseVersionId = Guid.NewGuid();
    private static readonly Guid SubjectId = Guid.NewGuid();

    private static readonly LocationsOrTimePeriodsQueryRequest QueryRequest = new() { SubjectId = SubjectId };

    [Fact]
    public async Task GetSubjectMeta_LatestRelease()
    {
        ReleaseVersion releaseVersion = _dataFixture
            .DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

        ReleaseFile releaseFile = _dataFixture
            .DefaultReleaseFile()
            .WithReleaseVersionId(ReleaseVersionId)
            .WithFile(_dataFixture.DefaultFile(FileType.Data).WithSubjectId(SubjectId));

        var subjectMetaViewModel = new SubjectMetaViewModel();

        var cacheKey = GetCacheKey(releaseVersion, releaseFile);

        var (controller, mocks) = BuildControllerAndMocks();

        SetupCall(mocks.contentPersistenceHelper, ReleaseVersionId, releaseVersion);

        mocks
            .cacheService.Setup(s => s.GetItemAsync(cacheKey, typeof(SubjectMetaViewModel)))
            .ReturnsAsync((object?)null);

        mocks
            .cacheService.Setup(s => s.SetItemAsync<object>(cacheKey, subjectMetaViewModel))
            .Returns(Task.CompletedTask);

        mocks.releaseSubjectService.Setup(mock => mock.Find(SubjectId, null)).ReturnsAsync(releaseFile);

        mocks.subjectMetaService.Setup(s => s.GetSubjectMeta(releaseFile)).ReturnsAsync(subjectMetaViewModel);

        var result = await controller.GetSubjectMeta(SubjectId);
        VerifyAllMocks(mocks);

        result.AssertOkResult(subjectMetaViewModel);
    }

    [Fact]
    public async Task GetSubjectMeta_SpecificRelease()
    {
        ReleaseVersion releaseVersion = _dataFixture
            .DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

        ReleaseFile releaseFile = _dataFixture
            .DefaultReleaseFile()
            .WithReleaseVersionId(ReleaseVersionId)
            .WithFile(_dataFixture.DefaultFile(FileType.Data).WithSubjectId(SubjectId));

        var subjectMetaViewModel = new SubjectMetaViewModel();

        var cacheKey = GetCacheKey(releaseVersion, releaseFile);

        var (controller, mocks) = BuildControllerAndMocks();

        SetupCall(mocks.contentPersistenceHelper, ReleaseVersionId, releaseVersion);

        mocks
            .cacheService.Setup(s => s.GetItemAsync(cacheKey, typeof(SubjectMetaViewModel)))
            .ReturnsAsync((object?)null);

        mocks
            .cacheService.Setup(s => s.SetItemAsync<object>(cacheKey, subjectMetaViewModel))
            .Returns(Task.CompletedTask);

        mocks.releaseSubjectService.Setup(s => s.Find(SubjectId, ReleaseVersionId)).ReturnsAsync(releaseFile);

        mocks.subjectMetaService.Setup(s => s.GetSubjectMeta(releaseFile)).ReturnsAsync(subjectMetaViewModel);

        var result = await controller.GetSubjectMeta(releaseVersionId: ReleaseVersionId, subjectId: SubjectId);
        VerifyAllMocks(mocks);

        result.AssertOkResult(subjectMetaViewModel);
    }

    [Fact]
    public async Task GetSubjectMeta_LatestRelease_ReleaseSubjectNotFound()
    {
        var (controller, mocks) = BuildControllerAndMocks();

        mocks.releaseSubjectService.Setup(mock => mock.Find(SubjectId, null)).ReturnsAsync(new NotFoundResult());

        var result = await controller.GetSubjectMeta(SubjectId);
        VerifyAllMocks(mocks);

        result.AssertNotFoundResult();
    }

    [Fact]
    public async Task GetSubjectMeta_SpecificRelease_ReleaseSubjectNotFound()
    {
        var (controller, mocks) = BuildControllerAndMocks();

        mocks
            .releaseSubjectService.Setup(mock => mock.Find(SubjectId, ReleaseVersionId))
            .ReturnsAsync(new NotFoundResult());

        var result = await controller.GetSubjectMeta(releaseVersionId: ReleaseVersionId, subjectId: SubjectId);
        VerifyAllMocks(mocks);

        result.AssertNotFoundResult();
    }

    [Fact]
    public async Task FilterSubjectMeta_LatestRelease()
    {
        var subjectMetaViewModel = new SubjectMetaViewModel();
        var cancellationToken = new CancellationTokenSource().Token;

        var (controller, mocks) = BuildControllerAndMocks();

        mocks
            .subjectMetaService.Setup(s => s.FilterSubjectMeta(null, QueryRequest, cancellationToken))
            .ReturnsAsync(subjectMetaViewModel);

        var result = await controller.FilterSubjectMeta(QueryRequest, cancellationToken);
        VerifyAllMocks(mocks);

        result.AssertOkResult(subjectMetaViewModel);
    }

    [Fact]
    public async Task FilterSubjectMeta_SpecificRelease()
    {
        var subjectMetaViewModel = new SubjectMetaViewModel();
        var cancellationToken = new CancellationTokenSource().Token;

        var (controller, mocks) = BuildControllerAndMocks();

        mocks
            .subjectMetaService.Setup(s => s.FilterSubjectMeta(ReleaseVersionId, QueryRequest, cancellationToken))
            .ReturnsAsync(subjectMetaViewModel);

        var result = await controller.FilterSubjectMeta(ReleaseVersionId, QueryRequest, cancellationToken);
        VerifyAllMocks(mocks);

        result.AssertOkResult(subjectMetaViewModel);
    }

    private static SubjectMetaCacheKey GetCacheKey(ReleaseVersion releaseVersion, ReleaseFile releaseFile)
    {
        return new SubjectMetaCacheKey(
            publicationSlug: releaseVersion.Release.Publication.Slug,
            releaseSlug: releaseVersion.Release.Slug,
            releaseFile.File.SubjectId!.Value
        );
    }

    private (
        TableBuilderMetaController controller,
        (
            Mock<IPersistenceHelper<ContentDbContext>> contentPersistenceHelper,
            Mock<IReleaseSubjectService> releaseSubjectService,
            Mock<ISubjectMetaService> subjectMetaService,
            Mock<IPublicBlobCacheService> cacheService
        ) mocks
    ) BuildControllerAndMocks()
    {
        var contentPersistenceHelper = MockPersistenceHelper<ContentDbContext>();
        var releaseSubjectService = new Mock<IReleaseSubjectService>(Strict);
        var subjectMetaService = new Mock<ISubjectMetaService>(Strict);
        var publicBlobCacheService = new Mock<IPublicBlobCacheService>(Strict);

        var controller = new TableBuilderMetaController(
            contentPersistenceHelper.Object,
            releaseSubjectService.Object,
            subjectMetaService.Object,
            publicBlobCacheService.Object,
            Mock.Of<ILogger<TableBuilderMetaController>>()
        );

        return (
            controller,
            (contentPersistenceHelper, releaseSubjectService, subjectMetaService, publicBlobCacheService)
        );
    }
}
