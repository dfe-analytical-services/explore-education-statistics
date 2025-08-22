#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using static GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.MethodologyVersionSummaryViewModel;
using static GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.PublicationViewModel;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Util;

public static class PermissionsUtils
{
    public static async Task<ReleasePermissions> GetReleasePermissions(IUserService userService,
        ReleaseVersion releaseVersion)
    {
        return new ReleasePermissions
        {
            CanAddPrereleaseUsers =
                await userService.CheckCanAssignPrereleaseContactsToReleaseVersion(releaseVersion).IsRight(),
            CanUpdateRelease = await userService.CheckCanUpdateRelease(releaseVersion.Release).IsRight(),
            CanViewReleaseVersion = await userService.CheckCanViewReleaseVersion(releaseVersion).IsRight(),
            CanUpdateReleaseVersion = await userService.CheckCanUpdateReleaseVersion(releaseVersion).IsRight(),
            CanDeleteReleaseVersion = await userService.CheckCanDeleteReleaseVersion(releaseVersion).IsRight(),
            CanMakeAmendmentOfReleaseVersion = await userService.CheckCanMakeAmendmentOfReleaseVersion(releaseVersion).IsRight()
        };
    }

    public static async Task<PublicationPermissions> GetPublicationPermissions(
        IUserService userService,
        Publication publication)
    {
        return new PublicationPermissions
        {
            CanUpdatePublication = await userService.CheckCanUpdatePublication().IsRight(),
            CanUpdatePublicationSummary = await userService.CheckCanUpdatePublicationSummary(publication).IsRight(),
            CanCreateReleases = await userService.CheckCanCreateReleaseForPublication(publication).IsRight(),
            CanAdoptMethodologies = await userService.CheckCanAdoptMethodologyForPublication(publication).IsRight(),
            CanCreateMethodologies = await userService.CheckCanCreateMethodologyForPublication(publication).IsRight(),
            CanManageExternalMethodology =
                await userService.CheckCanManageExternalMethodologyForPublication(publication).IsRight(),
            CanManageReleaseSeries = await userService.CheckCanManageReleaseSeries(publication).IsRight(),
            CanUpdateContact = await userService.CheckCanUpdateContact(publication).IsRight(),
            CanUpdateContributorReleaseRole =
                await userService.CheckCanUpdateReleaseRole(publication, Contributor).IsRight(),
            CanViewReleaseTeamAccess =
                await userService.CheckCanViewReleaseTeamAccess(publication).IsRight()
        };
    }

    public static async Task<MethodologyVersionPermissions> GetMethodologyVersionPermissions(
        IUserService userService,
        MethodologyVersion methodologyVersion,
        PublicationMethodology publicationMethodology)
    {
        return new MethodologyVersionPermissions
        {
            CanDeleteMethodology =
                await userService.CheckCanDeleteMethodologyVersion(methodologyVersion).IsRight(),
            CanUpdateMethodology =
                await userService.CheckCanUpdateMethodologyVersion(methodologyVersion).IsRight(),
            CanApproveMethodology =
                await userService.CheckCanApproveMethodologyVersion(methodologyVersion).IsRight(),
            CanSubmitMethodologyForHigherReview =
                await userService.CheckCanSubmitMethodologyForHigherReview(methodologyVersion).IsRight(),
            CanMarkMethodologyAsDraft =
                await userService.CheckCanMarkMethodologyVersionAsDraft(methodologyVersion).IsRight(),
            CanMakeAmendmentOfMethodology =
                await userService.CheckCanMakeAmendmentOfMethodology(methodologyVersion).IsRight(),
            CanRemoveMethodologyLink =
                await userService.CheckCanDropMethodologyLink(publicationMethodology).IsRight()
        };
    }
}
