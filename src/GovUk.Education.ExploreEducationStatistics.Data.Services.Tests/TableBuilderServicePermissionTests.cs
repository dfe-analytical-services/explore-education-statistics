#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Options;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Security;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests;

public class TableBuilderServicePermissionTests
{
    private readonly DataFixture _dataFixture = new();

    [Fact]
    public async Task Query_LatestRelease_CanViewSubjectData()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)]);

        var releaseVersion = publication.Releases.Single().Versions.Single();

        ReleaseFile releaseFile = _dataFixture
            .DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .WithFile(_dataFixture.DefaultFile(FileType.Data));

        var subjectId = releaseFile.File.SubjectId!.Value;

        await using var contextDbContext = InMemoryContentDbContext();
        contextDbContext.Publications.Add(publication);
        contextDbContext.ReleaseFiles.Add(releaseFile);
        await contextDbContext.SaveChangesAsync();

        var subjectRepository = new Mock<ISubjectRepository>(Strict);

        subjectRepository.Setup(s => s.FindPublicationIdForSubject(subjectId, default)).ReturnsAsync(publication.Id);

        await PolicyCheckBuilder<DataSecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher<ReleaseFile>(
                rf => rf.Id == releaseFile.Id,
                DataSecurityPolicies.CanViewSubjectData
            )
            .AssertForbidden(async userService =>
            {
                var service = BuildTableBuilderService(
                    contextDbContext,
                    userService: userService.Object,
                    subjectRepository: subjectRepository.Object
                );

                return await service.Query(new FullTableQuery { SubjectId = subjectId });
            });
    }

    [Fact]
    public async Task Query_ReleaseVersionId_CanViewSubjectData()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)]);

        var releaseVersion = publication.Releases.Single().Versions.Single();

        ReleaseFile releaseFile = _dataFixture
            .DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .WithFile(_dataFixture.DefaultFile(FileType.Data));

        var subjectId = releaseFile.File.SubjectId!.Value;

        await using var contextDbContext = InMemoryContentDbContext();
        contextDbContext.Publications.Add(publication);
        contextDbContext.ReleaseFiles.Add(releaseFile);
        await contextDbContext.SaveChangesAsync();

        await PolicyCheckBuilder<DataSecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher<ReleaseFile>(
                rf => rf.Id == releaseFile.Id,
                DataSecurityPolicies.CanViewSubjectData
            )
            .AssertForbidden(async userService =>
            {
                var service = BuildTableBuilderService(contextDbContext, userService: userService.Object);
                return await service.Query(
                    releaseVersionId: releaseVersion.Id,
                    new FullTableQuery { SubjectId = subjectId }
                );
            });
    }

    private TableBuilderService BuildTableBuilderService(
        ContentDbContext contentDbContext,
        IFilterItemRepository? filterItemRepository = null,
        ILocationService? locationService = null,
        IObservationService? observationService = null,
        ISubjectResultMetaService? subjectResultMetaService = null,
        ISubjectCsvMetaService? subjectCsvMetaService = null,
        ISubjectRepository? subjectRepository = null,
        ITableBuilderQueryOptimiser? tableBuilderQueryOptimiser = null,
        IUserService? userService = null,
        IOptions<TableBuilderOptions>? tableBuilderOptions = null,
        IOptions<LocationsOptions>? locationsOptions = null
    )
    {
        return new(
            Mock.Of<StatisticsDbContext>(),
            contentDbContext,
            locationService ?? Mock.Of<ILocationService>(Strict),
            observationService ?? Mock.Of<IObservationService>(Strict),
            subjectResultMetaService ?? Mock.Of<ISubjectResultMetaService>(Strict),
            subjectCsvMetaService ?? Mock.Of<ISubjectCsvMetaService>(Strict),
            subjectRepository ?? Mock.Of<ISubjectRepository>(Strict),
            tableBuilderQueryOptimiser ?? Mock.Of<ITableBuilderQueryOptimiser>(Strict),
            userService ?? Mock.Of<IUserService>(Strict),
            tableBuilderOptions ?? new TableBuilderOptions().ToOptionsWrapper(),
            locationsOptions ?? new LocationsOptions().ToOptionsWrapper()
        );
    }
}
