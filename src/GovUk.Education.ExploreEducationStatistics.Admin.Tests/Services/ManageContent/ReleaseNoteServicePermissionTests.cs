#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
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
        // Arrange
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion();

        // Act & Assert
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
                    var sut = BuildService(contentDbContext, userService.Object);

                    return await sut.CreateReleaseNote(
                        releaseVersionId: releaseVersion.Id,
                        new ReleaseNoteCreateRequest { Reason = "" }
                    );
                }
            });
    }

    [Fact]
    public async Task DeleteReleaseNote()
    {
        // Arrange
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion();
        var releaseNoteId = Guid.NewGuid();

        // Act & Assert
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
                    var sut = BuildService(contentDbContext, userService.Object);

                    return await sut.DeleteReleaseNote(
                        releaseVersionId: releaseVersion.Id,
                        releaseNoteId: releaseNoteId
                    );
                }
            });
    }

    [Fact]
    public async Task UpdateReleaseNote()
    {
        // Arrange
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion();
        var releaseNoteId = Guid.NewGuid();

        // Act & Assert
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
                    var sut = BuildService(contentDbContext, userService.Object);

                    return await sut.UpdateReleaseNote(
                        releaseVersionId: releaseVersion.Id,
                        releaseNoteId: releaseNoteId,
                        new ReleaseNoteUpdateRequest { Reason = "", On = DateTimeOffset.UtcNow }
                    );
                }
            });
    }

    private static ReleaseNoteService BuildService(ContentDbContext contentDbContext, IUserService userService) =>
        new(contentDbContext: contentDbContext, userService: userService);
}
