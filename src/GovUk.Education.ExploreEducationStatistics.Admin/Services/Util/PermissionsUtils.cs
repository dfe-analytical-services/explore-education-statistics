#nullable enable
using System.Threading.Tasks;
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
    public static async Task<ReleasePermissions> GetReleasePermissions(IUserService userService, Release release)
    {
        return new ReleasePermissions
        {
            CanAddPrereleaseUsers = await userService.CheckCanAssignPrereleaseContactsToRelease(release).IsRight(),
            CanViewRelease = await userService.CheckCanViewRelease(release).IsRight(),
            CanUpdateRelease = await userService.CheckCanUpdateRelease(release).IsRight(),
            CanDeleteRelease = await userService.CheckCanDeleteRelease(release).IsRight(),
            CanMakeAmendmentOfRelease = await userService.CheckCanMakeAmendmentOfRelease(release).IsRight()
        };
    }

    public static async Task<PublicationPermissions> GetPublicationPermissions(
        IUserService userService,
        Publication publication)
    {
        var canUpdatePublication = await userService.CheckCanUpdatePublication(publication).IsRight();
        return new PublicationPermissions
        {
            CanUpdatePublication = canUpdatePublication,
            CanUpdatePublicationTitle = await userService.CheckCanUpdatePublicationTitle().IsRight(),
            CanUpdatePublicationSupersededBy = await userService.CheckCanUpdatePublicationSupersededBy().IsRight(),
            CanCreateReleases = await userService.CheckCanCreateReleaseForPublication(publication).IsRight(),
            CanAdoptMethodologies = await userService.CheckCanAdoptMethodologyForPublication(publication).IsRight(),
            CanCreateMethodologies = await userService.CheckCanCreateMethodologyForPublication(publication).IsRight(),
            CanManageExternalMethodology =
                await userService.CheckCanManageExternalMethodologyForPublication(publication).IsRight(),
            CanUpdateContact = canUpdatePublication, // EES-3596 Switch to CheckCanUpdateContact permission
            CanUpdateContributorReleaseRole =
                await userService.CheckCanUpdateReleaseRole(publication, Contributor).IsRight(),
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
            CanMarkMethodologyAsDraft =
                await userService.CheckCanMarkMethodologyVersionAsDraft(methodologyVersion).IsRight(),
            CanMakeAmendmentOfMethodology =
                await userService.CheckCanMakeAmendmentOfMethodology(methodologyVersion).IsRight(),
            CanRemoveMethodologyLink =
                await userService.CheckCanDropMethodologyLink(publicationMethodology).IsRight()
        };
    }
}
