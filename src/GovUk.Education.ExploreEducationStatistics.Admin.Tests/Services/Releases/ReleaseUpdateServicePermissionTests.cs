#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Releases;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Security;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Releases;

public class ReleaseUpdateServicePermissionTests
{
    private readonly DataFixture _dataFixture = new();

    [Fact]
    public async Task CreateReleaseUpdate()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion();

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher<ReleaseVersion>(
                rv => rv.Id == releaseVersion.Id,
                SecurityPolicies.CanUpdateSpecificReleaseVersion
            )
            .AssertForbidden(async userService =>
            {
                var contentDbContextId = Guid.NewGuid().ToString();
                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    contentDbContext.ReleaseVersions.Add(releaseVersion);
                    await contentDbContext.SaveChangesAsync();
                }

                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    var service = BuildService(contentDbContext, userService.Object);

                    return await service.CreateReleaseUpdate(
                        releaseVersionId: releaseVersion.Id,
                        date: null,
                        reason: "",
                        cancellationToken: CancellationToken.None
                    );
                }
            });
    }

    [Fact]
    public async Task GetReleaseUpdates()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion();

        await PolicyCheckBuilder<ContentSecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher<ReleaseVersion>(
                rv => rv.Id == releaseVersion.Id,
                ContentSecurityPolicies.CanViewSpecificReleaseVersion
            )
            .AssertForbidden(async userService =>
            {
                var contentDbContextId = Guid.NewGuid().ToString();
                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    contentDbContext.ReleaseVersions.Add(releaseVersion);
                    await contentDbContext.SaveChangesAsync();
                }

                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    var service = BuildService(contentDbContext, userService.Object);

                    return await service.GetReleaseUpdates(
                        releaseVersionId: releaseVersion.Id,
                        cancellationToken: CancellationToken.None
                    );
                }
            });
    }

    [Fact]
    public async Task DeleteReleaseUpdate()
    {
        Update update = _dataFixture.DefaultUpdate().WithReleaseVersion(_dataFixture.DefaultReleaseVersion());

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher<ReleaseVersion>(
                rv => rv.Id == update.ReleaseVersionId,
                SecurityPolicies.CanUpdateSpecificReleaseVersion
            )
            .AssertForbidden(async userService =>
            {
                var contentDbContextId = Guid.NewGuid().ToString();
                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    contentDbContext.Update.Add(update);
                    await contentDbContext.SaveChangesAsync();
                }

                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    var service = BuildService(contentDbContext, userService.Object);

                    return await service.DeleteReleaseUpdate(
                        releaseVersionId: update.ReleaseVersionId,
                        releaseNoteId: update.Id,
                        cancellationToken: CancellationToken.None
                    );
                }
            });
    }

    [Fact]
    public async Task UpdateReleaseUpdate()
    {
        Update update = _dataFixture.DefaultUpdate().WithReleaseVersion(_dataFixture.DefaultReleaseVersion());

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher<ReleaseVersion>(
                rv => rv.Id == update.ReleaseVersionId,
                SecurityPolicies.CanUpdateSpecificReleaseVersion
            )
            .AssertForbidden(async userService =>
            {
                var contentDbContextId = Guid.NewGuid().ToString();
                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    contentDbContext.Update.Add(update);
                    await contentDbContext.SaveChangesAsync();
                }

                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    var service = BuildService(contentDbContext, userService.Object);

                    return await service.UpdateReleaseUpdate(
                        releaseVersionId: update.ReleaseVersionId,
                        releaseNoteId: update.Id,
                        date: null,
                        reason: "",
                        cancellationToken: CancellationToken.None
                    );
                }
            });
    }

    private static ReleaseUpdatesService BuildService(
        ContentDbContext contentDbContext,
        IUserService? userService = null
    ) => new(contentDbContext, userService ?? MockUtils.AlwaysTrueUserService().Object);
}
