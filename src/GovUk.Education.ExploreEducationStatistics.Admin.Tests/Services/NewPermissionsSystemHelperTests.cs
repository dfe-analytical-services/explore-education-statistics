#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class NewPermissionsSystemHelperTests
{
    public static TheoryData<PublicationRole, IReadOnlyList<PublicationRole>, PublicationRole?, PublicationRole?> NewPublicationRoleData =>
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

    public static TheoryData<ReleaseRole, IReadOnlyList<PublicationRole>, PublicationRole?, PublicationRole?> NewReleaseRoleData =>
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

    [Theory]
    [MemberData(nameof(NewPublicationRoleData))]
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
    [MemberData(nameof(NewReleaseRoleData))]
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

    private static NewPermissionsSystemHelper SetupNewPermissionsSystemHelper(
        IUserPublicationRoleRepository? userPublicationRoleRepository = null)
    {
        var userPublicationRoleRepositoryMock = new Mock<IUserPublicationRoleRepository>();
        userPublicationRoleRepositoryMock
            .Setup(rvr => rvr.GetAllRolesByUserAndPublication(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync([]);

        return new NewPermissionsSystemHelper(
            userPublicationRoleRepository ?? userPublicationRoleRepositoryMock.Object);
    }
}
