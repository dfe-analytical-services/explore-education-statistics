#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Util;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Util;

public class PublicationRoleUtilsTests
{
    [Theory]
    [InlineData(PublicationRole.Owner, PublicationRole.Drafter)]
    [InlineData(PublicationRole.Allower, PublicationRole.Approver)]
    public void ConvertToNewPermissionsSystemPublicationRole_WithOldRole(
        PublicationRole publicationRoleToConvert,
        PublicationRole expectedPublicationRole
    )
    {
        var convertedPublicationRole = publicationRoleToConvert.ConvertToNewPermissionsSystemPublicationRole();

        Assert.Equal(expectedPublicationRole, convertedPublicationRole);
    }

    [Theory]
    [InlineData(PublicationRole.Drafter)]
    [InlineData(PublicationRole.Approver)]
    public void ConvertToNewPermissionsSystemPublicationRole_WithNewRole_Throws(
        PublicationRole publicationRoleToConvert
    )
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            publicationRoleToConvert.ConvertToNewPermissionsSystemPublicationRole()
        );
    }

    [Theory]
    [InlineData(ReleaseRole.PrereleaseViewer, null, false)]
    [InlineData(ReleaseRole.Contributor, PublicationRole.Drafter, true)]
    [InlineData(ReleaseRole.Approver, PublicationRole.Approver, true)]
    public void TryConvertToNewPermissionsSystemPublicationRole(
        ReleaseRole releaseRoleToConvert,
        PublicationRole? expectedPublicationRole,
        bool expectedConversionSuccessful
    )
    {
        var conversionSuccessful = releaseRoleToConvert.TryConvertToNewPermissionsSystemPublicationRole(
            out var convertedPublicationRole
        );

        Assert.Equal(expectedConversionSuccessful, conversionSuccessful);
        Assert.Equal(expectedPublicationRole, convertedPublicationRole);
    }

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
