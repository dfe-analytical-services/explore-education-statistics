#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Options;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using IReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class ReleaseVersionServicePermissionTests
{
    private readonly DataFixture _dataFixture = new();

    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public async Task GetRelease()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()));

        await PolicyCheckBuilder<ContentSecurityPolicies>()
            .SetupResourceCheckToFail(releaseVersion, ContentSecurityPolicies.CanViewSpecificReleaseVersion)
            .AssertForbidden(
                async userService =>
                {
                    await using var contextDbContext = InMemoryApplicationDbContext();
                    contextDbContext.ReleaseVersions.Add(releaseVersion);
                    await contextDbContext.SaveChangesAsync();

                    var service = BuildService(
                        contentDbContext: contextDbContext,
                        userService: userService.Object);

                    return await service.GetRelease(releaseVersion.Id);
                }
            );
    }

    [Fact]
    public async Task GetLatestPublishedRelease()
    {
        Publication publication = _dataFixture.DefaultPublication()
            .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]);

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher<Publication>(p => p.Id == publication.Id,
                CanViewSpecificPublication)
            .AssertForbidden(
                async userService =>
                {
                    await using var contextDbContext = InMemoryApplicationDbContext();
                    contextDbContext.Publications.Add(publication);
                    await contextDbContext.SaveChangesAsync();

                    var service = BuildService(
                        contentDbContext: contextDbContext,
                        userService: userService.Object);

                    return await service.GetLatestPublishedRelease(publication.Id);
                }
            );
    }

    [Fact]
    public async Task GetDeleteReleaseVersionPlan()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()));

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher<ReleaseVersion>(rv => rv.Id == releaseVersion.Id,
                CanDeleteSpecificReleaseVersion)
            .AssertForbidden(
                async userService =>
                {
                    await using var contextDbContext = InMemoryApplicationDbContext();
                    contextDbContext.ReleaseVersions.Add(releaseVersion);
                    await contextDbContext.SaveChangesAsync();

                    var service = BuildService(
                        contentDbContext: contextDbContext,
                        userService: userService.Object);

                    return await service.GetDeleteReleaseVersionPlan(releaseVersion.Id);
                }
            );
    }

    [Fact]
    public async Task DeleteRelease()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()));

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher<ReleaseVersion>(rv => rv.Id == releaseVersion.Id,
                CanDeleteSpecificReleaseVersion)
            .AssertForbidden(
                async userService =>
                {
                    await using var contextDbContext = InMemoryApplicationDbContext();
                    contextDbContext.ReleaseVersions.Add(releaseVersion);
                    await contextDbContext.SaveChangesAsync();

                    var service = BuildService(
                        contentDbContext: contextDbContext,
                        userService: userService.Object);

                    return await service.DeleteReleaseVersion(releaseVersion.Id);
                }
            );
    }

    [Fact]
    public async Task DeleteTestRelease()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()));

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher<ReleaseVersion>(rv => rv.Id == releaseVersion.Id,
                CanDeleteTestRelease)
            .AssertForbidden(
                async userService =>
                {
                    await using var contextDbContext = InMemoryApplicationDbContext();
                    contextDbContext.ReleaseVersions.Add(releaseVersion);
                    await contextDbContext.SaveChangesAsync();

                    var service = BuildService(
                        contentDbContext: contextDbContext,
                        userService: userService.Object);

                    return await service.DeleteTestReleaseVersion(releaseVersion.Id);
                }
            );
    }

    [Fact]
    public async Task ListReleasesWithStatuses_CanViewAllReleases()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()))
            .WithApprovalStatus(ReleaseApprovalStatus.Approved);

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupCheck(RegisteredUser)
            .SetupCheck(CanViewAllReleases)
            .AssertSuccess(
                async userService =>
                {
                    userService
                        .Setup(s => s.MatchesPolicy(releaseVersion, CanAssignPreReleaseUsersToSpecificRelease))
                        .ReturnsAsync(true);

                    userService
                        .Setup(s => s.MatchesPolicy(releaseVersion, ContentSecurityPolicies.CanViewSpecificReleaseVersion))
                        .ReturnsAsync(true);

                    userService
                        .Setup(s => s.MatchesPolicy(releaseVersion, CanUpdateSpecificReleaseVersion))
                        .ReturnsAsync(true);

                    userService
                        .Setup(s => s.MatchesPolicy(releaseVersion, CanDeleteSpecificReleaseVersion))
                        .ReturnsAsync(true);

                    userService
                        .Setup(s => s.MatchesPolicy(releaseVersion, CanMakeAmendmentOfSpecificReleaseVersion))
                        .ReturnsAsync(true);

                    userService
                        .Setup(s => s.MatchesPolicy(releaseVersion.Release, CanUpdateSpecificRelease))
                        .ReturnsAsync(true);

                    await using var contextDbContext = InMemoryApplicationDbContext();
                    contextDbContext.ReleaseVersions.Add(releaseVersion);
                    await contextDbContext.SaveChangesAsync();

                    var service = BuildService(
                        contentDbContext: contextDbContext,
                        userService: userService.Object);

                    var result = await service.ListReleasesWithStatuses(ReleaseApprovalStatus.Approved);

                    var viewModel = result.AssertRight();
                    Assert.Single(viewModel);
                    Assert.Equal(releaseVersion.Id, viewModel[0].Id);

                    return result;
                }
            );
    }

    [Fact]
    public async Task ListReleasesWithStatuses_CanViewRelatedReleases()
    {
        var (releaseVersion, otherReleaseVersion) = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()))
            .WithApprovalStatus(ReleaseApprovalStatus.Approved)
            .GenerateTuple2();

        UserReleaseRole userReleaseRole = _dataFixture.DefaultUserReleaseRole()
            .WithReleaseVersion(releaseVersion)
            .WithUser(new User { Id = _userId })
            .WithRole(ReleaseRole.Contributor);

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupCheck(RegisteredUser)
            .ExpectCheckToFail(CanViewAllReleases)
            .AssertSuccess(
                async userService =>
                {
                    userService
                        .Setup(s => s.MatchesPolicy(releaseVersion, CanAssignPreReleaseUsersToSpecificRelease))
                        .ReturnsAsync(true);

                    userService
                        .Setup(s => s.MatchesPolicy(releaseVersion, ContentSecurityPolicies.CanViewSpecificReleaseVersion))
                        .ReturnsAsync(true);

                    userService
                        .Setup(s => s.MatchesPolicy(releaseVersion, CanUpdateSpecificReleaseVersion))
                        .ReturnsAsync(true);

                    userService
                        .Setup(s => s.MatchesPolicy(releaseVersion, CanDeleteSpecificReleaseVersion))
                        .ReturnsAsync(true);

                    userService
                        .Setup(s => s.MatchesPolicy(releaseVersion, CanMakeAmendmentOfSpecificReleaseVersion))
                        .ReturnsAsync(true);

                    userService
                        .Setup(s => s.MatchesPolicy(releaseVersion.Release, CanUpdateSpecificRelease))
                        .ReturnsAsync(true);

                    userService
                        .Setup(s => s.GetUserId())
                        .Returns(_userId);

                    await using var contextDbContext = InMemoryApplicationDbContext();
                    contextDbContext.ReleaseVersions.AddRange(releaseVersion, otherReleaseVersion);
                    contextDbContext.UserReleaseRoles.AddRange(userReleaseRole);
                    await contextDbContext.SaveChangesAsync();

                    var service = BuildService(
                        contentDbContext: contextDbContext,
                        userService: userService.Object);

                    var result = await service.ListReleasesWithStatuses(ReleaseApprovalStatus.Approved);

                    var viewModel = result.AssertRight();
                    Assert.Single(viewModel);
                    Assert.Equal(releaseVersion.Id, viewModel[0].Id);

                    return result;
                }
            );
    }

    [Fact]
    public async Task ListReleasesWithStatuses_NoAccessToSystem()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .ExpectCheckToFail(RegisteredUser)
            .AssertForbidden(
                async userService =>
                {
                    var service = BuildService(userService: userService.Object);
                    return await service.ListReleasesWithStatuses(ReleaseApprovalStatus.Approved);
                }
            );
    }

    [Fact]
    public async Task RemoveDataFiles()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()));

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher<ReleaseVersion>(rv => rv.Id == releaseVersion.Id,
                CanUpdateSpecificReleaseVersion)
            .AssertForbidden(
                async userService =>
                {
                    await using var contextDbContext = InMemoryApplicationDbContext();
                    contextDbContext.ReleaseVersions.Add(releaseVersion);
                    await contextDbContext.SaveChangesAsync();

                    var service = BuildService(
                        contentDbContext: contextDbContext,
                        userService: userService.Object);

                    return await service.RemoveDataFiles(releaseVersion.Id, Guid.NewGuid());
                }
            );
    }

    [Fact]
    public async Task UpdateReleasePublished()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()));

        await PolicyCheckBuilder<SecurityPolicies>()
            .ExpectCheckToFail(IsBauUser)
            .AssertForbidden(
                async userService =>
                {
                    await using var contextDbContext = InMemoryApplicationDbContext();
                    contextDbContext.ReleaseVersions.Add(releaseVersion);
                    await contextDbContext.SaveChangesAsync();

                    var service = BuildService(
                        contentDbContext: contextDbContext,
                        userService: userService.Object);

                    return await service.UpdateReleasePublished(releaseVersion.Id,
                        new ReleasePublishedUpdateRequest());
                }
            );
    }

    private static ReleaseVersionService BuildService(
        IUserService userService,
        ContentDbContext? contentDbContext = null,
        StatisticsDbContext? statisticsDbContext = null,
        IReleaseVersionRepository? releaseVersionRepository = null,
        bool enableReplacementOfPublicApiDataSets = false)
    {
        contentDbContext ??= Mock.Of<ContentDbContext>();
        statisticsDbContext ??= Mock.Of<StatisticsDbContext>();

        return new ReleaseVersionService(
            contentDbContext,
            statisticsDbContext,
            AdminMapper(),
            MockUtils.MockPersistenceHelper<ContentDbContext>().Object,
            userService,
            releaseVersionRepository ?? new ReleaseVersionRepository(contentDbContext, statisticsDbContext),
            Mock.Of<IReleaseCacheService>(),
            Mock.Of<IReleaseFileRepository>(),
            Mock.Of<IReleaseDataFileService>(),
            Mock.Of<IReleaseFileService>(),
            Mock.Of<IDataImportService>(),
            Mock.Of<IFootnoteRepository>(),
            Mock.Of<IDataBlockService>(),
            Mock.Of<IReleasePublishingStatusRepository>(),
            Mock.Of<IReleaseSubjectRepository>(),
            Mock.Of<IDataSetVersionService>(),
            Mock.Of<IProcessorClient>(),
            Mock.Of<IPrivateBlobCacheService>(),
            Mock.Of<IReleaseSlugValidator>(),
             featureFlags: Microsoft.Extensions.Options.Options.Create(new FeatureFlags()
             {
                 EnableReplacementOfPublicApiDataSets = enableReplacementOfPublicApiDataSets
             }),
            Mock.Of<IDataSetVersionRepository>()
        );
    }
}
