#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class NewPermissionsSystemHelperTests
{
    // (publicationRoleToCreate, existingPublicationRoles, expectedNewSystemPublicationRoleToRemove, expectedNewSystemPublicationRoleToCreate)
    public static TheoryData<
        PublicationRole,
        HashSet<PublicationRole>,
        PublicationRole?,
        PublicationRole?
    > PublicationRoleCreationData =>
        new()
        {
            { PublicationRole.Owner, [], null, PublicationRole.Drafter },
            { PublicationRole.Owner, [PublicationRole.Allower], null, PublicationRole.Drafter },
            { PublicationRole.Owner, [PublicationRole.Approver], null, null },
            { PublicationRole.Owner, [PublicationRole.Drafter], null, null },
            { PublicationRole.Drafter, [], null, PublicationRole.Drafter },
            { PublicationRole.Drafter, [PublicationRole.Allower], null, PublicationRole.Drafter },
            { PublicationRole.Drafter, [PublicationRole.Approver], null, null },
            { PublicationRole.Drafter, [PublicationRole.Drafter], null, null },
            { PublicationRole.Allower, [], null, PublicationRole.Approver },
            { PublicationRole.Allower, [PublicationRole.Owner], null, PublicationRole.Approver },
            { PublicationRole.Allower, [PublicationRole.Approver], null, null },
            { PublicationRole.Allower, [PublicationRole.Drafter], PublicationRole.Drafter, PublicationRole.Approver },
            { PublicationRole.Approver, [], null, PublicationRole.Approver },
            { PublicationRole.Approver, [PublicationRole.Owner], null, PublicationRole.Approver },
            { PublicationRole.Approver, [PublicationRole.Approver], null, null },
            { PublicationRole.Approver, [PublicationRole.Drafter], PublicationRole.Drafter, PublicationRole.Approver },
        };

    // (releaseRoleToCreate, existingPublicationRoles, expectedNewSystemPublicationRoleToRemove, expectedNewSystemPublicationRoleToCreate)
    public static TheoryData<
        ReleaseRole,
        HashSet<PublicationRole>,
        PublicationRole?,
        PublicationRole?
    > ReleaseRoleCreationData =>
        new()
        {
            { ReleaseRole.PrereleaseViewer, [], null, null },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Allower], null, null },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Approver], null, null },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Drafter], null, null },
            { ReleaseRole.Contributor, [], null, PublicationRole.Drafter },
            { ReleaseRole.Contributor, [PublicationRole.Allower], null, PublicationRole.Drafter },
            { ReleaseRole.Contributor, [PublicationRole.Approver], null, null },
            { ReleaseRole.Contributor, [PublicationRole.Drafter], null, null },
            { ReleaseRole.Approver, [], null, PublicationRole.Approver },
            { ReleaseRole.Approver, [PublicationRole.Owner], null, PublicationRole.Approver },
            { ReleaseRole.Approver, [PublicationRole.Approver], null, null },
            { ReleaseRole.Approver, [PublicationRole.Drafter], PublicationRole.Drafter, PublicationRole.Approver },
        };

    // (oldPublicationRoleToRemove, existingPublicationRoles, existingReleaseRoles, expectedNewSystemPublicationRoleToRemove)
    public static TheoryData<
        PublicationRole,
        HashSet<PublicationRole>,
        HashSet<ReleaseRole>,
        PublicationRole?
    > PublicationRoleRemovalData =>
        new()
        {
            // Tests for the NEW `Drafter` publication role
            // old role, role exists, no release role for same publication maps to same role, the NEW publication role does NOT exist => Does NOTHING
            {
                PublicationRole.Owner,
                [PublicationRole.Owner, PublicationRole.Allower],
                [ReleaseRole.PrereleaseViewer, ReleaseRole.Approver],
                null
            },
            // old role, role exists, 1 release role for same publication maps to same role, the NEW publication role does NOT exist => Does NOTHING
            {
                PublicationRole.Owner,
                [PublicationRole.Owner, PublicationRole.Allower],
                [ReleaseRole.Contributor],
                null
            },
            // old role, role exists, no release role for same publication maps to same role, the NEW publication role DOES exist => Deletes NEW role
            {
                PublicationRole.Owner,
                [PublicationRole.Owner, PublicationRole.Allower, PublicationRole.Drafter],
                [ReleaseRole.PrereleaseViewer, ReleaseRole.Approver],
                PublicationRole.Drafter
            },
            // old role, role exists, 1 release role for same publication maps to same role, the NEW publication role DOES exist => Does NOTHING
            {
                PublicationRole.Owner,
                [PublicationRole.Owner, PublicationRole.Allower, PublicationRole.Drafter],
                [ReleaseRole.Contributor],
                null
            },
            // Tests for the NEW `Approver` publication role
            // old role, role exists, no release role for same publication maps to same role, the NEW publication role does NOT exist => Does NOTHING
            {
                PublicationRole.Allower,
                [PublicationRole.Allower, PublicationRole.Owner],
                [ReleaseRole.PrereleaseViewer, ReleaseRole.Contributor],
                null
            },
            // old role, role exists, 1 release role for same publication maps to same role, the NEW publication role does NOT exist => Does NOTHING
            {
                PublicationRole.Allower,
                [PublicationRole.Allower, PublicationRole.Owner],
                [ReleaseRole.Approver],
                null
            },
            // old role, role exists, no release role for same publication maps to same role, the NEW publication role DOES exist => Deletes NEW role
            {
                PublicationRole.Allower,
                [PublicationRole.Allower, PublicationRole.Owner, PublicationRole.Approver],
                [ReleaseRole.PrereleaseViewer, ReleaseRole.Contributor],
                PublicationRole.Approver
            },
            // old role, role exists, 1 release role for same publication maps to same role, the NEW publication role DOES exist => Does NOTHING
            {
                PublicationRole.Allower,
                [PublicationRole.Allower, PublicationRole.Owner, PublicationRole.Approver],
                [ReleaseRole.Approver],
                null
            },
        };

    // (releaseRoleToRemove, existingPublicationRoles, existingReleaseRoles, expectedNewSystemPublicationRoleToRemove)
    public static TheoryData<
        ReleaseRole,
        HashSet<PublicationRole>,
        HashSet<ReleaseRole>,
        PublicationRole?
    > ReleaseRoleDeletionData =>
        new()
        {
            // Tests for the NEW `Drafter` publication role
            // role exists, no OLD publication role maps to same new role, the NEW publication role does NOT exist => Does NOTHING
            {
                ReleaseRole.Contributor,
                [PublicationRole.Allower],
                [ReleaseRole.Contributor, ReleaseRole.Approver],
                null
            },
            // role exists, 1 OLD publication role maps to same new role, the NEW publication role does NOT exist => Does NOTHING
            {
                ReleaseRole.Contributor,
                [PublicationRole.Owner],
                [ReleaseRole.Contributor, ReleaseRole.Approver],
                null
            },
            // role exists, no OLD publication role maps to same new role, the NEW publication role DOES exist => Deletes NEW role
            {
                ReleaseRole.Contributor,
                [PublicationRole.Allower, PublicationRole.Drafter],
                [ReleaseRole.Contributor, ReleaseRole.Approver],
                PublicationRole.Drafter
            },
            // role exists, 1 OLD publication role maps to same new role, the NEW publication role DOES exist => Does NOTHING
            {
                ReleaseRole.Contributor,
                [PublicationRole.Owner, PublicationRole.Drafter],
                [ReleaseRole.Contributor, ReleaseRole.Approver],
                null
            },
            // Tests for the NEW `Approver` publication role
            // role exists, no OLD publication role maps to same new role, the NEW publication role does NOT exist => Does NOTHING
            { ReleaseRole.Approver, [PublicationRole.Owner], [ReleaseRole.Approver, ReleaseRole.Contributor], null },
            // role exists, 1 OLD publication role maps to same new role, the NEW publication role does NOT exist => Does NOTHING
            { ReleaseRole.Approver, [PublicationRole.Allower], [ReleaseRole.Approver, ReleaseRole.Contributor], null },
            // role exists, no OLD publication role maps to same new role, the NEW publication role DOES exist => Deletes NEW role
            {
                ReleaseRole.Approver,
                [PublicationRole.Owner, PublicationRole.Approver],
                [ReleaseRole.Approver, ReleaseRole.Contributor],
                PublicationRole.Approver
            },
            // role exists, 1 OLD publication role maps to same new role, the NEW publication role DOES exist => Does NOTHING
            {
                ReleaseRole.Approver,
                [PublicationRole.Allower, PublicationRole.Approver],
                [ReleaseRole.Approver, ReleaseRole.Contributor],
                null
            },
        };

    [Theory]
    [MemberData(nameof(PublicationRoleCreationData))]
    public void DetermineNewPermissionsSystemChanges_ForPublicationRole(
        PublicationRole publicationRoleToCreate,
        HashSet<PublicationRole> existingPublicationRoles,
        PublicationRole? expectedNewSystemPublicationRoleToRemove,
        PublicationRole? expectedNewSystemPublicationRoleToCreate
    )
    {
        var newPermissionsSystemHelper = SetupNewPermissionsSystemHelper();

        var (newSystemPublicationRoleToRemove, newSystemPublicationRoleToCreate) =
            newPermissionsSystemHelper.DetermineNewPermissionsSystemChanges(
                publicationRoleToCreate: publicationRoleToCreate,
                existingPublicationRoles: existingPublicationRoles
            );

        Assert.Equal(expectedNewSystemPublicationRoleToRemove, newSystemPublicationRoleToRemove);
        Assert.Equal(expectedNewSystemPublicationRoleToCreate, newSystemPublicationRoleToCreate);
    }

    [Theory]
    [MemberData(nameof(ReleaseRoleCreationData))]
    public void DetermineNewPermissionsSystemChanges_ForReleaseRole(
        ReleaseRole releaseRoleToCreate,
        HashSet<PublicationRole> existingPublicationRoles,
        PublicationRole? expectedNewSystemPublicationRoleToRemove,
        PublicationRole? expectedNewSystemPublicationRoleToCreate
    )
    {
        var newPermissionsSystemHelper = SetupNewPermissionsSystemHelper();

        var (newSystemPublicationRoleToRemove, newSystemPublicationRoleToCreate) =
            newPermissionsSystemHelper.DetermineNewPermissionsSystemChanges(
                releaseRoleToCreate: releaseRoleToCreate,
                existingPublicationRoles: existingPublicationRoles
            );

        Assert.Equal(expectedNewSystemPublicationRoleToRemove, newSystemPublicationRoleToRemove);
        Assert.Equal(expectedNewSystemPublicationRoleToCreate, newSystemPublicationRoleToCreate);
    }

    [Theory]
    [MemberData(nameof(PublicationRoleRemovalData))]
    public void DetermineNewPermissionsSystemRoleToRemove_ForPublicationRole(
        PublicationRole oldPublicationRoleToRemove,
        HashSet<PublicationRole> existingPublicationRoles,
        HashSet<ReleaseRole> existingReleaseRoles,
        PublicationRole? expectedNewSystemPublicationRoleToRemove
    )
    {
        var newPermissionsSystemHelper = SetupNewPermissionsSystemHelper();

        var newSystemPublicationRoleToRemove = newPermissionsSystemHelper.DetermineNewPermissionsSystemRoleToRemove(
            oldPublicationRoleToRemove: oldPublicationRoleToRemove,
            existingPublicationRoles: existingPublicationRoles,
            existingReleaseRoles: existingReleaseRoles
        );

        Assert.Equal(expectedNewSystemPublicationRoleToRemove, newSystemPublicationRoleToRemove);
    }

    [Theory]
    [InlineData(PublicationRole.Owner)]
    [InlineData(PublicationRole.Allower)]
    public void DetermineNewPermissionsSystemRoleToRemove_ForOldPublicationRoleWhichDoesNotExist_Throws(
        PublicationRole publicationRole
    )
    {
        var newPermissionsSystemHelper = SetupNewPermissionsSystemHelper();

        var exception = Assert.Throws<ArgumentException>(() =>
            newPermissionsSystemHelper.DetermineNewPermissionsSystemRoleToRemove(
                oldPublicationRoleToRemove: publicationRole,
                existingPublicationRoles: [],
                existingReleaseRoles: []
            )
        );

        Assert.Equal(
            $"The publication role '{publicationRole}' is not in the existing list of publication roles.",
            exception.Message
        );
    }

    [Theory]
    [InlineData(PublicationRole.Drafter, false)]
    [InlineData(PublicationRole.Approver, false)]
    [InlineData(PublicationRole.Drafter, true)]
    [InlineData(PublicationRole.Approver, true)]
    public void DetermineNewPermissionsSystemRoleToRemove_ForNewPublicationRole_Throws(
        PublicationRole publicationRole,
        bool roleExists
    )
    {
        var newPermissionsSystemHelper = SetupNewPermissionsSystemHelper();

        var exception = Assert.Throws<ArgumentException>(() =>
            newPermissionsSystemHelper.DetermineNewPermissionsSystemRoleToRemove(
                oldPublicationRoleToRemove: publicationRole,
                existingPublicationRoles: roleExists ? [publicationRole] : [],
                existingReleaseRoles: []
            )
        );

        Assert.Equal(
            $"Unexpected publication role: '{publicationRole}'. Expected an OLD permissions system role.",
            exception.Message
        );
    }

    [Theory]
    [MemberData(nameof(ReleaseRoleDeletionData))]
    public void DetermineNewPermissionsSystemRoleToRemove_ForReleaseRole(
        ReleaseRole releaseRoleToRemove,
        HashSet<PublicationRole> existingPublicationRoles,
        HashSet<ReleaseRole> existingReleaseRoles,
        PublicationRole? expectedNewSystemPublicationRoleToRemove
    )
    {
        var newPermissionsSystemHelper = SetupNewPermissionsSystemHelper();

        var newSystemPublicationRoleToRemove = newPermissionsSystemHelper.DetermineNewPermissionsSystemRoleToRemove(
            releaseRoleToRemove: releaseRoleToRemove,
            existingPublicationRoles: existingPublicationRoles,
            existingReleaseRoles: existingReleaseRoles
        );

        Assert.Equal(expectedNewSystemPublicationRoleToRemove, newSystemPublicationRoleToRemove);
    }

    [Fact]
    public void DetermineNewPermissionsSystemRoleToRemove_ForReleaseRoleWhichDoesNotExist_Throws()
    {
        var newPermissionsSystemHelper = SetupNewPermissionsSystemHelper();

        var exception = Assert.Throws<ArgumentException>(() =>
            newPermissionsSystemHelper.DetermineNewPermissionsSystemRoleToRemove(
                releaseRoleToRemove: ReleaseRole.Contributor,
                existingPublicationRoles: [],
                existingReleaseRoles: []
            )
        );

        Assert.Equal(
            $"The release role '{ReleaseRole.Contributor}' is not in the existing list of release roles.",
            exception.Message
        );
    }

    [Fact]
    public void DetermineNewPermissionsSystemRoleToRemove_ForPrereleaseReleaseRole_ReturnsNull()
    {
        var newPermissionsSystemHelper = SetupNewPermissionsSystemHelper();

        var newSystemPublicationRoleToRemove = newPermissionsSystemHelper.DetermineNewPermissionsSystemRoleToRemove(
            releaseRoleToRemove: ReleaseRole.PrereleaseViewer,
            existingPublicationRoles: [],
            existingReleaseRoles: [ReleaseRole.PrereleaseViewer]
        );

        Assert.Null(newSystemPublicationRoleToRemove);
    }

    private static NewPermissionsSystemHelper SetupNewPermissionsSystemHelper()
    {
        return new NewPermissionsSystemHelper();
    }
}
