#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.ManageContent;

public class ReleaseNoteServicePermissionTests
{
    private readonly DataFixture _dataFixture = new();

    [Fact]
    public async Task AddReleaseNote()
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

                    return await service.AddReleaseNote(
                        releaseVersionId: releaseVersion.Id,
                        new ReleaseNoteSaveRequest(),
                        CancellationToken.None
                    );
                }
            });
    }

    [Fact]
    public async Task DeleteReleaseNote()
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

                    return await service.DeleteReleaseNote(
                        releaseVersionId: update.ReleaseVersionId,
                        releaseNoteId: update.Id,
                        CancellationToken.None
                    );
                }
            });
    }

    [Fact]
    public async Task UpdateReleaseNote()
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

                    return await service.UpdateReleaseNote(
                        releaseVersionId: update.ReleaseVersionId,
                        releaseNoteId: update.Id,
                        new ReleaseNoteSaveRequest(),
                        CancellationToken.None
                    );
                }
            });
    }

    private static ReleaseNoteService BuildService(
        ContentDbContext contentDbContext,
        IUserService? userService = null
    ) => new(contentDbContext, userService ?? MockUtils.AlwaysTrueUserService().Object);
}
