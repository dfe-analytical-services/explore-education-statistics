#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
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
        Publication publication = _dataFixture.DefaultPublication()
            .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)]);

        var releaseVersion = publication.Releases.Single().Versions.Single();

        var releaseSubject = new ReleaseSubject
        {
            ReleaseVersionId = releaseVersion.Id,
            SubjectId = Guid.NewGuid(),
        };

        var statisticsPersistenceHelper = MockUtils.MockPersistenceHelper<StatisticsDbContext>();
        MockUtils.SetupCall(statisticsPersistenceHelper, releaseSubject);

        await using var contextDbContext = InMemoryContentDbContext();
        contextDbContext.Publications.Add(publication);
        await contextDbContext.SaveChangesAsync();

        var subjectRepository = new Mock<ISubjectRepository>(Strict);

        subjectRepository
            .Setup(s => s.FindPublicationIdForSubject(releaseSubject.SubjectId, default))
            .ReturnsAsync(publication.Id);

        await PolicyCheckBuilder<DataSecurityPolicies>()
            .SetupResourceCheckToFail(releaseSubject, DataSecurityPolicies.CanViewSubjectData)
            .AssertForbidden(
                async userService =>
                {
                    var service = BuildTableBuilderService(
                        contextDbContext,
                        userService: userService.Object,
                        subjectRepository: subjectRepository.Object,
                        statisticsPersistenceHelper: statisticsPersistenceHelper.Object
                    );

                    return await service.Query(
                        new FullTableQuery { SubjectId = releaseSubject.SubjectId }
                    );
                }
            );
    }

    [Fact]
    public async Task Query_ReleaseVersionId_CanViewSubjectData()
    {
        Publication publication = _dataFixture.DefaultPublication()
            .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)]);

        var releaseVersion = publication.Releases.Single().Versions.Single();

        var releaseSubject = new ReleaseSubject
        {
            ReleaseVersionId = releaseVersion.Id,
            SubjectId = Guid.NewGuid(),
        };

        var statisticsPersistenceHelper = MockUtils.MockPersistenceHelper<StatisticsDbContext>();
        MockUtils.SetupCall(statisticsPersistenceHelper, releaseSubject);

        await using var contextDbContext = InMemoryContentDbContext();
        contextDbContext.Publications.Add(publication);
        await contextDbContext.SaveChangesAsync();

        await PolicyCheckBuilder<DataSecurityPolicies>()
            .SetupResourceCheckToFail(releaseSubject, DataSecurityPolicies.CanViewSubjectData)
            .AssertForbidden(
                async userService =>
                {
                    var service = BuildTableBuilderService(
                        contextDbContext,
                        userService: userService.Object,
                        statisticsPersistenceHelper: statisticsPersistenceHelper.Object
                    );
                    return await service.Query(
                        releaseVersionId: releaseVersion.Id,
                        new FullTableQuery { SubjectId = releaseSubject.SubjectId }
                    );
                }
            );
    }

    private TableBuilderService BuildTableBuilderService(
        ContentDbContext contentDbContext,
        IFilterItemRepository? filterItemRepository = null,
        ILocationService? locationService = null,
        IObservationService? observationService = null,
        IPersistenceHelper<StatisticsDbContext>? statisticsPersistenceHelper = null,
        ISubjectResultMetaService? subjectResultMetaService = null,
        ISubjectCsvMetaService? subjectCsvMetaService = null,
        ISubjectRepository? subjectRepository = null,
        IUserService? userService = null,
        IOptions<TableBuilderOptions>? tableBuilderOptions = null,
        IOptions<LocationsOptions>? locationsOptions = null)
    {
        return new(
            Mock.Of<StatisticsDbContext>(),
            contentDbContext,
            filterItemRepository ?? Mock.Of<IFilterItemRepository>(Strict),
            locationService ?? Mock.Of<ILocationService>(Strict),
            observationService ?? Mock.Of<IObservationService>(Strict),
            statisticsPersistenceHelper ?? MockUtils.MockPersistenceHelper<StatisticsDbContext>().Object,
            subjectResultMetaService ?? Mock.Of<ISubjectResultMetaService>(Strict),
            subjectCsvMetaService ?? Mock.Of<ISubjectCsvMetaService>(Strict),
            subjectRepository ?? Mock.Of<ISubjectRepository>(Strict),
            userService ?? Mock.Of<IUserService>(Strict),
            tableBuilderOptions ?? new TableBuilderOptions().ToOptionsWrapper(),
            locationsOptions ?? new LocationsOptions().ToOptionsWrapper()
        );
    }
}
