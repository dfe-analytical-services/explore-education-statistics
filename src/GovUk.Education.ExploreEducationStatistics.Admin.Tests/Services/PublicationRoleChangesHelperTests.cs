#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class PublicationRoleChangesHelperTests
{
    // (publicationRoleToCreate, existingPublicationRoleForPublication, expectedPublicationRoleToRemove, expectedPublicationRoleToCreate)
    public static TheoryData<PublicationRole, PublicationRole?, PublicationRole?, PublicationRole?> RoleCreationData =>
        new()
        {
            // csharpier-ignore-start
            { PublicationRole.Drafter, null, null, PublicationRole.Drafter },
            { PublicationRole.Approver, null, null, PublicationRole.Approver },
            { PublicationRole.Drafter, PublicationRole.Approver, null, null },
            { PublicationRole.Approver, PublicationRole.Drafter, PublicationRole.Drafter, PublicationRole.Approver }
            // csharpier-ignore-end
        };

    [Theory]
    [MemberData(nameof(RoleCreationData))]
    public void DetermineChanges(
        PublicationRole publicationRoleToCreate,
        PublicationRole? existingPublicationRoleForPublication,
        PublicationRole? expectedPublicationRoleToRemove,
        PublicationRole? expectedPublicationRoleToCreate
    )
    {
        var newPermissionsSystemHelper = SetupHelper();

        var (resultantPublicationRoleToRemove, resultantPublicationRoleToCreate) =
            newPermissionsSystemHelper.DetermineChanges(
                existingPublicationRoleForPublication: existingPublicationRoleForPublication,
                publicationRoleToCreate: publicationRoleToCreate
            );

        Assert.Equal(expectedPublicationRoleToRemove, resultantPublicationRoleToRemove);
        Assert.Equal(expectedPublicationRoleToCreate, resultantPublicationRoleToCreate);
    }

    [Theory]
    [InlineData(PublicationRole.Approver)]
    [InlineData(PublicationRole.Drafter)]
    public void DetermineChanges_RoleToCreateAlreadyExists_Throws(PublicationRole publicationRoleToCreate)
    {
        var newPermissionsSystemHelper = SetupHelper();

        var exception = Assert.Throws<ArgumentException>(() =>
            newPermissionsSystemHelper.DetermineChanges(
                existingPublicationRoleForPublication: publicationRoleToCreate,
                publicationRoleToCreate: publicationRoleToCreate
            )
        );

        Assert.Equal($"The publication role '{publicationRoleToCreate}' already exists.", exception.Message);
    }

    private static PublicationRoleChangesHelper SetupHelper()
    {
        return new PublicationRoleChangesHelper();
    }
}
