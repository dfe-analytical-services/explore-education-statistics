#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Util;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Util;

public class PublicationRoleUtilsTests
{
    [Theory]
    [InlineData(PublicationRole.Owner, false)]
    [InlineData(PublicationRole.Allower, false)]
    [InlineData(PublicationRole.Drafter, true)]
    [InlineData(PublicationRole.Approver, true)]
    public void IsNewPermissionsSystemPublicationRole(PublicationRole publicationRole, bool expectedResult)
    {
        var isNewPermissionsSystemPublicationRole = publicationRole.IsNewPermissionsSystemPublicationRole();

        Assert.Equal(expectedResult, isNewPermissionsSystemPublicationRole);
    }
}
