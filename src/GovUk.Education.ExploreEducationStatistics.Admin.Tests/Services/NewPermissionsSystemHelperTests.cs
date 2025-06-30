#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Util;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class NewPermissionsSystemHelperTests
{
    public static TheoryData<
        PublicationRole, 
        IReadOnlyList<PublicationRole>, 
        PublicationRole?, 
        PublicationRole?> PublicationRoleCreationData =>
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

    public static TheoryData<
        ReleaseRole, 
        IReadOnlyList<PublicationRole>, 
        PublicationRole?, 
        PublicationRole?> ReleaseRoleCreationData =>
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

    public static TheoryData<
        UserPublicationRole,
        IReadOnlyList<UserPublicationRole>, 
        IReadOnlyList<ReleaseRole>, 
        UserPublicationRole?> PublicationRoleDeletionData()
    {
        var oldOwnerUserPublicationRole = new UserPublicationRole
        {
            Role = PublicationRole.Owner,
        };

        // Duplicate role. In theory, this should never happen, but we include it for completeness.
        var oldOwnerUserPublicationRole2 = new UserPublicationRole
        {
            Role = PublicationRole.Owner,
        };

        var oldAllowerUserPublicationRole = new UserPublicationRole
        {
            Role = PublicationRole.Allower,
        };

        // Duplicate role. In theory, this should never happen, but we include it for completeness.
        var oldAllowerUserPublicationRole2 = new UserPublicationRole
        {
            Role = PublicationRole.Allower,
        };

        var newDrafterUserPublicationRole = new UserPublicationRole
        {
            Role = PublicationRole.Drafter,
        };

        var newApproverUserPublicationRole = new UserPublicationRole
        {
            Role = PublicationRole.Approver,
        };

        return new()
        {
            // Tests for the NEW `Drafter` publication role
            // old role, role exists, no OTHER OLD publication role maps to same new role, no release role for same publication maps to same role, the NEW publication role does NOT exist => Does NOTHING
            { oldOwnerUserPublicationRole, [ oldOwnerUserPublicationRole, oldAllowerUserPublicationRole ], [ ReleaseRole.PrereleaseViewer, ReleaseRole.Approver ], null },
            // old role, role exists, no OTHER OLD publication role maps to same new role, 1 release role for same publication maps to same role, the NEW publication role does NOT exist => Does NOTHING
            { oldOwnerUserPublicationRole, [ oldOwnerUserPublicationRole, oldAllowerUserPublicationRole ], [ ReleaseRole.Contributor ], null },
            // old role, role exists, 1 OTHER OLD publication role maps to same new role, no release role for same publication maps to same role, the NEW publication role does NOT exist => Does NOTHING
            { oldOwnerUserPublicationRole, [ oldOwnerUserPublicationRole, oldOwnerUserPublicationRole2 ], [ ReleaseRole.PrereleaseViewer, ReleaseRole.Approver ], null },
            // old role, role exists, 1 OTHER OLD publication role maps to same new role, 1 release role for same publication maps to same role, the NEW publication role does NOT exist => Does NOTHING
            { oldOwnerUserPublicationRole, [ oldOwnerUserPublicationRole, oldOwnerUserPublicationRole2 ], [ ReleaseRole.Contributor ], null },
            // old role, role exists, no OTHER OLD publication role maps to same new role, no release role for same publication maps to same role, the NEW publication role DOES exist => Deletes NEW role
            { oldOwnerUserPublicationRole, [ oldOwnerUserPublicationRole, oldAllowerUserPublicationRole, newDrafterUserPublicationRole ], [ ReleaseRole.PrereleaseViewer, ReleaseRole.Approver ], newDrafterUserPublicationRole },
            // old role, role exists, no OTHER OLD publication role maps to same new role, 1 release role for same publication maps to same role, the NEW publication role DOES exist => Does NOTHING
            { oldOwnerUserPublicationRole, [ oldOwnerUserPublicationRole, oldAllowerUserPublicationRole, newDrafterUserPublicationRole ], [ ReleaseRole.Contributor ], null },
            // old role, role exists, 1 OTHER OLD publication role maps to same new role, no release role for same publication maps to same role, the NEW publication role DOES exist => Does NOTHING
            { oldOwnerUserPublicationRole, [ oldOwnerUserPublicationRole, oldOwnerUserPublicationRole2, newDrafterUserPublicationRole ], [ ReleaseRole.PrereleaseViewer, ReleaseRole.Approver ], null },
            // old role, role exists, 1 OTHER OLD publication role maps to same new role, 1 release role for same publication maps to same role, the NEW publication role DOES exist => Does NOTHING
            { oldOwnerUserPublicationRole, [ oldOwnerUserPublicationRole, oldOwnerUserPublicationRole2, newDrafterUserPublicationRole ], [ ReleaseRole.Contributor ], null },

            // Tests for the NEW `Approver` publication role
            // old role, role exists, no OTHER OLD publication role maps to same new role, no release role for same publication maps to same role, the NEW publication role does NOT exist => Does NOTHING
            { oldAllowerUserPublicationRole, [ oldAllowerUserPublicationRole, oldOwnerUserPublicationRole ], [ ReleaseRole.PrereleaseViewer, ReleaseRole.Contributor ], null },
            // old role, role exists, no OTHER OLD publication role maps to same new role, 1 release role for same publication maps to same role, the NEW publication role does NOT exist => Does NOTHING
            { oldAllowerUserPublicationRole, [ oldAllowerUserPublicationRole, oldOwnerUserPublicationRole ], [ ReleaseRole.Approver ], null },
            // old role, role exists, 1 OTHER OLD publication role maps to same new role, no release role for same publication maps to same role, the NEW publication role does NOT exist => Does NOTHING
            { oldAllowerUserPublicationRole, [ oldAllowerUserPublicationRole, oldAllowerUserPublicationRole2 ], [ ReleaseRole.PrereleaseViewer, ReleaseRole.Contributor ], null },
            // old role, role exists, 1 OTHER OLD publication role maps to same new role, 1 release role for same publication maps to same role, the NEW publication role does NOT exist => Does NOTHING
            { oldAllowerUserPublicationRole, [ oldAllowerUserPublicationRole, oldAllowerUserPublicationRole2 ], [ ReleaseRole.Approver ], null },
            // old role, role exists, no OTHER OLD publication role maps to same new role, no release role for same publication maps to same role, the NEW publication role DOES exist => Deletes NEW role
            { oldAllowerUserPublicationRole, [ oldAllowerUserPublicationRole, oldOwnerUserPublicationRole, newDrafterUserPublicationRole ], [ ReleaseRole.PrereleaseViewer, ReleaseRole.Contributor ], newDrafterUserPublicationRole },
            // old role, role exists, no OTHER OLD publication role maps to same new role, 1 release role for same publication maps to same role, the NEW publication role DOES exist => Does NOTHING
            { oldAllowerUserPublicationRole, [ oldAllowerUserPublicationRole, oldOwnerUserPublicationRole, newDrafterUserPublicationRole ], [ ReleaseRole.Approver ], null },
            // old role, role exists, 1 OTHER OLD publication role maps to same new role, no release role for same publication maps to same role, the NEW publication role DOES exist => Does NOTHING
            { oldAllowerUserPublicationRole, [ oldAllowerUserPublicationRole, oldAllowerUserPublicationRole2, newDrafterUserPublicationRole ], [ ReleaseRole.PrereleaseViewer, ReleaseRole.Contributor ], null },
            // old role, role exists, 1 OTHER OLD publication role maps to same new role, 1 release role for same publication maps to same role, the NEW publication role DOES exist => Does NOTHING
            { oldAllowerUserPublicationRole, [ oldAllowerUserPublicationRole, oldAllowerUserPublicationRole2, newDrafterUserPublicationRole ], [ ReleaseRole.Approver ], null },
        };
    }

    public static TheoryData<
        UserReleaseRole,
        IReadOnlyList<PublicationRole>,
        IReadOnlyList<UserReleaseRole>,
        UserPublicationRole?> ReleaseRoleDeletionData()
    {
        var releaseVersion1Id = Guid.NewGuid();
        var releaseVersion2Id = Guid.NewGuid();

        var oldContributorUserReleaseRole = new UserReleaseRole
        {
            Role = ReleaseRole.Contributor,
            ReleaseVersionId = releaseVersion1Id,
        };

        // Duplicate role for a different release version. Don't strictly need to set the ID
        // here, but it's included for completeness.
        var oldContributorUserReleaseRole2 = new UserReleaseRole
        {
            Role = ReleaseRole.Contributor,
            ReleaseVersionId = releaseVersion2Id,
        };

        var oldApproverUserReleaseRole = new UserReleaseRole
        {
            Role = ReleaseRole.Approver,
            ReleaseVersionId = releaseVersion1Id,
        };

        // Duplicate role for a different release version. Don't strictly need to set the ID
        // here, but it's included for completeness.
        var oldApproverUserReleaseRole2 = new UserReleaseRole
        {
            Role = ReleaseRole.Approver,
            ReleaseVersionId = releaseVersion2Id,
        };

        var newDrafterUserPublicationRole = new UserPublicationRole
        {
            Role = PublicationRole.Drafter,
        };

        var newApproverUserPublicationRole = new UserPublicationRole
        {
            Role = PublicationRole.Approver,
        };

        return new()
        {
            // Tests for the NEW `Drafter` publication role
            // role exists, no OLD publication role maps to same new role, no OTHER release role for same publication maps to same role, the NEW publication role does NOT exist => Does NOTHING
            { oldContributorUserReleaseRole, [ PublicationRole.Allower ], [ oldContributorUserReleaseRole, oldApproverUserReleaseRole ], null },
             // role exists, no OLD publication role maps to same new role, 1 OTHER release role for same publication maps to same role, the NEW publication role does NOT exist => Does NOTHING
            { oldContributorUserReleaseRole, [ PublicationRole.Allower ], [ oldContributorUserReleaseRole, oldContributorUserReleaseRole2 ], null },
            // role exists, 1 OLD publication role maps to same new role, no OTHER release role for same publication maps to same role, the NEW publication role does NOT exist => Does NOTHING
            { oldContributorUserReleaseRole, [ PublicationRole.Owner ], [ oldContributorUserReleaseRole, oldApproverUserReleaseRole ], null },
            // role exists, 1 OLD publication role maps to same new role, 1 OTHER release role for same publication maps to same role, the NEW publication role does NOT exist => Does NOTHING
            { oldContributorUserReleaseRole, [ PublicationRole.Owner ] , [ oldContributorUserReleaseRole, oldContributorUserReleaseRole2 ], null },
            // role exists, no OLD publication role maps to same new role, no OTHER release role for same publication maps to same role, the NEW publication role DOES exist => Deletes NEW role
            { oldContributorUserReleaseRole, [ PublicationRole.Allower, PublicationRole.Drafter ], [ oldContributorUserReleaseRole, oldApproverUserReleaseRole ], newDrafterUserPublicationRole },
            // role exists, no OLD publication role maps to same new role, 1 OTHER release role for same publication maps to same role, the NEW publication role DOES exist => Does NOTHING
            { oldContributorUserReleaseRole, [ PublicationRole.Allower, PublicationRole.Drafter ] , [ oldContributorUserReleaseRole, oldContributorUserReleaseRole2 ], null },
            // role exists, 1 OLD publication role maps to same new role, no OTHER release role for same publication maps to same role, the NEW publication role DOES exist => Does NOTHING
            { oldContributorUserReleaseRole, [ PublicationRole.Owner, PublicationRole.Drafter ] , [ oldContributorUserReleaseRole, oldApproverUserReleaseRole ], null },
            // role exists, 1 OLD publication role maps to same new role, 1 OTHER release role for same publication maps to same role, the NEW publication role DOES exist => Does NOTHING
            { oldContributorUserReleaseRole, [ PublicationRole.Owner, PublicationRole.Drafter ] , [ oldContributorUserReleaseRole, oldContributorUserReleaseRole2 ], null },

            // Tests for the NEW `Approver` publication role
            // role exists, no OLD publication role maps to same new role, no OTHER release role for same publication maps to same role, the NEW publication role does NOT exist => Does NOTHING
            { oldContributorUserReleaseRole, [ PublicationRole.Owner ], [ oldApproverUserReleaseRole, oldContributorUserReleaseRole ], null },
             // role exists, no OLD publication role maps to same new role, 1 OTHER release role for same publication maps to same role, the NEW publication role does NOT exist => Does NOTHING
            { oldContributorUserReleaseRole, [ PublicationRole.Owner ], [ oldApproverUserReleaseRole, oldApproverUserReleaseRole2 ], null },
            // role exists, 1 OLD publication role maps to same new role, no OTHER release role for same publication maps to same role, the NEW publication role does NOT exist => Does NOTHING
            { oldContributorUserReleaseRole, [ PublicationRole.Allower ], [ oldApproverUserReleaseRole, oldContributorUserReleaseRole ], null },
            // role exists, 1 OLD publication role maps to same new role, 1 OTHER release role for same publication maps to same role, the NEW publication role does NOT exist => Does NOTHING
            { oldContributorUserReleaseRole, [ PublicationRole.Allower] , [ oldApproverUserReleaseRole, oldApproverUserReleaseRole2 ], null },
            // role exists, no OLD publication role maps to same new role, no OTHER release role for same publication maps to same role, the NEW publication role DOES exist => Deletes NEW role
            { oldContributorUserReleaseRole, [ PublicationRole.Owner, PublicationRole.Approver ], [ oldApproverUserReleaseRole, oldContributorUserReleaseRole ], newApproverUserPublicationRole },
            // role exists, no OLD publication role maps to same new role, 1 OTHER release role for same publication maps to same role, the NEW publication role DOES exist => Does NOTHING
            { oldContributorUserReleaseRole, [ PublicationRole.Owner, PublicationRole.Approver ] , [ oldApproverUserReleaseRole, oldApproverUserReleaseRole2 ], null },
            // role exists, 1 OLD publication role maps to same new role, no OTHER release role for same publication maps to same role, the NEW publication role DOES exist => Does NOTHING
            { oldContributorUserReleaseRole, [ PublicationRole.Allower, PublicationRole.Approver ] , [ oldApproverUserReleaseRole, oldContributorUserReleaseRole ], null },
            // role exists, 1 OLD publication role maps to same new role, 1 OTHER release role for same publication maps to same role, the NEW publication role DOES exist => Does NOTHING
            { oldContributorUserReleaseRole, [ PublicationRole.Allower, PublicationRole.Approver ] , [ oldApproverUserReleaseRole, oldApproverUserReleaseRole2 ], null },
        };
    }

    // new role, doesn't exist => throws exception
    // new role, role exists => throws exception
    // old role, doesn't exist => throws exception

    // role doesn't exist => throws exception
    // role exists, pre-release => Does NOTHING


    [Theory]
    [MemberData(nameof(PublicationRoleCreationData))]
    public async Task DetermineNewPermissionsSystemChanges_ForPublicationRole(
        PublicationRole publicationRoleToCreate,
        IReadOnlyList<PublicationRole> existingPublicationRoles,
        PublicationRole? expectedNewSystemPublicationRoleToRemove,
        PublicationRole? expectedNewSystemPublicationRoleToCreate)
    {
        var userId = Guid.NewGuid();
        var publicationId = Guid.NewGuid();

        var userPublicationRoleRepositoryMock = new Mock<IUserPublicationRoleRepository>();
        userPublicationRoleRepositoryMock
            .Setup(rvr => rvr.GetAllRolesByUserAndPublication(userId, publicationId))
            .ReturnsAsync([.. existingPublicationRoles]);

        var newPermissionsSystemHelper = SetupNewPermissionsSystemHelper(
            userPublicationRoleRepository: userPublicationRoleRepositoryMock.Object);

        var (newSystemPublicationRoleToRemove, newSystemPublicationRoleToCreate) = 
            await newPermissionsSystemHelper.DetermineNewPermissionsSystemChanges(
                publicationRoleToCreate: publicationRoleToCreate,
                userId: userId,
                publicationId: publicationId);

        Assert.Equal(expectedNewSystemPublicationRoleToRemove, newSystemPublicationRoleToRemove);
        Assert.Equal(expectedNewSystemPublicationRoleToCreate, newSystemPublicationRoleToCreate);
    }

    [Theory]
    [MemberData(nameof(ReleaseRoleCreationData))]
    public async Task DetermineNewPermissionsSystemChanges_ForReleaseRole(
        ReleaseRole releaseRoleToCreate,
        IReadOnlyList<PublicationRole> existingPublicationRoles,
        PublicationRole? expectedNewSystemPublicationRoleToRemove,
        PublicationRole? expectedNewSystemPublicationRoleToCreate)
    {
        var userId = Guid.NewGuid();
        var publicationId = Guid.NewGuid();

        var userPublicationRoleRepositoryMock = new Mock<IUserPublicationRoleRepository>();
        userPublicationRoleRepositoryMock
            .Setup(rvr => rvr.GetAllRolesByUserAndPublication(userId, publicationId))
            .ReturnsAsync([.. existingPublicationRoles]);

        var newPermissionsSystemHelper = SetupNewPermissionsSystemHelper(
            userPublicationRoleRepository: userPublicationRoleRepositoryMock.Object);

        var (newSystemPublicationRoleToRemove, newSystemPublicationRoleToCreate) =
            await newPermissionsSystemHelper.DetermineNewPermissionsSystemChanges(
                releaseRoleToCreate: releaseRoleToCreate,
                userId: userId,
                publicationId: publicationId);

        Assert.Equal(expectedNewSystemPublicationRoleToRemove, newSystemPublicationRoleToRemove);
        Assert.Equal(expectedNewSystemPublicationRoleToCreate, newSystemPublicationRoleToCreate);
    }

    [Theory]
    [MemberData(nameof(PublicationRoleDeletionData))]
    public async Task DetermineNewPermissionsSystemRoleToDelete_ForPublicationRole(
        UserPublicationRole userPublicationRoleToRemove,
        IReadOnlyList<UserPublicationRole> existingUserPublicationRoles,
        IReadOnlyList<ReleaseRole> existingReleaseRoles,
        UserPublicationRole? expectedNewSystemPublicationRoleToRemove)
    {
        var userId = Guid.NewGuid();
        var publicationId = Guid.NewGuid();

        var userPublicationRoleRepositoryMock = new Mock<IUserPublicationRoleRepository>();
        userPublicationRoleRepositoryMock
            .Setup(rvr => rvr.ListUserPublicationRolesByUserAndPublication(userId, publicationId))
            .ReturnsAsync([.. existingUserPublicationRoles]);

        if (expectedNewSystemPublicationRoleToRemove is not null) {
            var equivalentNewPermissionsSystemPublicationRoleToRemove = 
                PublicationRoleUtils.ConvertToNewPermissionsSystemPublicationRole(userPublicationRoleToRemove.Role);

            userPublicationRoleRepositoryMock
                .Setup(rvr => rvr.GetUserPublicationRole(
                    userId, 
                    publicationId, 
                    equivalentNewPermissionsSystemPublicationRoleToRemove))
                .ReturnsAsync(expectedNewSystemPublicationRoleToRemove);
        }

        var userReleaseRoleRepositoryMock = new Mock<IUserReleaseRoleRepository>();
        userReleaseRoleRepositoryMock
            .Setup(rvr => rvr.GetAllRolesByUserAndPublication(userId, publicationId))
            .ReturnsAsync([.. existingReleaseRoles]);

        var newPermissionsSystemHelper = SetupNewPermissionsSystemHelper(
            userPublicationRoleRepository: userPublicationRoleRepositoryMock.Object,
            userReleaseRoleRepository: userReleaseRoleRepositoryMock.Object);

        var newSystemPublicationRoleToRemove =
            await newPermissionsSystemHelper.DetermineNewPermissionsSystemRoleToDelete(userPublicationRoleToRemove);

        Assert.Equal(expectedNewSystemPublicationRoleToRemove, newSystemPublicationRoleToRemove);
    }

    [Theory]
    [MemberData(nameof(ReleaseRoleDeletionData))]
    public async Task DetermineNewPermissionsSystemRoleToDelete_ForReleaseRole(
        UserReleaseRole userReleaseRoleToRemove,
        IReadOnlyList<PublicationRole> existingUserPublicationRoles,
        IReadOnlyList<UserReleaseRole> existingReleaseRoles,
        UserPublicationRole? expectedNewSystemPublicationRoleToRemove)
    {
        var userId = Guid.NewGuid();
        var publicationId = Guid.NewGuid();

        var userPublicationRoleRepositoryMock = new Mock<IUserPublicationRoleRepository>();
        userPublicationRoleRepositoryMock
            .Setup(rvr => rvr.GetAllRolesByUserAndPublication(userId, publicationId))
            .ReturnsAsync([.. existingUserPublicationRoles]);

        if (expectedNewSystemPublicationRoleToRemove is not null)
        {
            PublicationRoleUtils.TryConvertToNewPermissionsSystemPublicationRole(
                userReleaseRoleToRemove.Role, 
                out var equivalentNewPermissionsSystemPublicationRoleToRemove);

            userPublicationRoleRepositoryMock
                .Setup(rvr => rvr.GetUserPublicationRole(
                    userId, 
                    publicationId, 
                    equivalentNewPermissionsSystemPublicationRoleToRemove!.Value))
                .ReturnsAsync(expectedNewSystemPublicationRoleToRemove);
        }

        var userReleaseRoleRepositoryMock = new Mock<IUserReleaseRoleRepository>();
        userReleaseRoleRepositoryMock
            .Setup(rvr => rvr.ListUserReleaseRolesByUserAndPublication(userId, publicationId))
            .ReturnsAsync([.. existingReleaseRoles]);

        var newPermissionsSystemHelper = SetupNewPermissionsSystemHelper(
            userPublicationRoleRepository: userPublicationRoleRepositoryMock.Object,
            userReleaseRoleRepository: userReleaseRoleRepositoryMock.Object);

        var newSystemPublicationRoleToRemove =
            await newPermissionsSystemHelper.DetermineNewPermissionsSystemRoleToDelete(userReleaseRoleToRemove);

        Assert.Equal(expectedNewSystemPublicationRoleToRemove, newSystemPublicationRoleToRemove);
    }

    private static NewPermissionsSystemHelper SetupNewPermissionsSystemHelper(
        IUserPublicationRoleRepository? userPublicationRoleRepository = null,
        IUserReleaseRoleRepository? userReleaseRoleRepository = null)
    {
        var userPublicationRoleRepositoryMock = new Mock<IUserPublicationRoleRepository>();
        userPublicationRoleRepositoryMock
            .Setup(rvr => rvr.GetAllRolesByUserAndPublication(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync([]);

        var userReleaseRoleRepositoryMock = new Mock<IUserReleaseRoleRepository>();
        userReleaseRoleRepositoryMock
            .Setup(rvr => rvr.GetAllRolesByUserAndPublication(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync([]);

        return new NewPermissionsSystemHelper(
            userPublicationRoleRepository ?? userPublicationRoleRepositoryMock.Object,
            userReleaseRoleRepository ?? userReleaseRoleRepositoryMock.Object);
    }
}
