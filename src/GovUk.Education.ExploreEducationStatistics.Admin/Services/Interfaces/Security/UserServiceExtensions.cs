#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Security;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security
{
    public static class UserServiceExtensions
    {
        public static Task<Either<ActionResult, Unit>> CheckCanAccessSystem(this IUserService userService)
        {
            return userService.CheckPolicy(SecurityPolicies.CanAccessSystem);
        }

        public static Task<Either<ActionResult, Unit>> CheckIsBauUser(this IUserService userService)
        {
            return userService.CheckPolicy(SecurityPolicies.IsBauUser);
        }

        public static Task<Either<ActionResult, Unit>> CheckCanAccessAnalystPages(this IUserService userService)
        {
            return userService.CheckPolicy(SecurityPolicies.CanAccessAnalystPages);
        }

        public static Task<Either<ActionResult, Unit>> CheckCanAccessPrereleasePages(this IUserService userService)
        {
            return userService.CheckPolicy(SecurityPolicies.CanAccessPrereleasePages);
        }

        public static Task<Either<ActionResult, PublicationMethodology>> CheckCanDropMethodologyLink(
            this IUserService userService, PublicationMethodology methodology)
        {
            return userService.CheckPolicy(methodology, SecurityPolicies.CanDropMethodologyLink);
        }

        public static Task<Either<ActionResult, Unit>> CheckCanManageAllUsers(this IUserService userService)
        {
            return userService.CheckPolicy(SecurityPolicies.CanManageUsersOnSystem);
        }

        public static Task<Either<ActionResult, Unit>> CheckCanViewAllImports(this IUserService userService)
        {
            return userService.CheckPolicy(SecurityPolicies.CanAccessAllImports);
        }

        public static Task<Either<ActionResult, Publication>> CheckCanAdoptMethodologyForPublication(
            this IUserService userService, Publication publication)
        {
            return userService.CheckPolicy(publication, SecurityPolicies.CanAdoptMethodologyForSpecificPublication);
        }

        public static Task<Either<ActionResult, Publication>> CheckCanCreateMethodologyForPublication(
            this IUserService userService, Publication publication)
        {
            return userService.CheckPolicy(publication, SecurityPolicies.CanCreateMethodologyForSpecificPublication);
        }

        public static Task<Either<ActionResult, Publication>> CheckCanManageExternalMethodologyForPublication(
            this IUserService userService, Publication publication)
        {
            return userService.CheckPolicy(publication, SecurityPolicies.CanManageExternalMethodologyForSpecificPublication);
        }

        public static Task<Either<ActionResult, MethodologyVersion>> CheckCanViewMethodology(
            this IUserService userService, MethodologyVersion methodologyVersion)
        {
            return userService.CheckPolicy(methodologyVersion, SecurityPolicies.CanViewSpecificMethodology);
        }

        public static Task<Either<ActionResult, MethodologyVersion>> CheckCanUpdateMethodologyVersion(
            this IUserService userService, MethodologyVersion methodologyVersion)
        {
            return userService.CheckPolicy(methodologyVersion, SecurityPolicies.CanUpdateSpecificMethodology);
        }

        public static Task<Either<ActionResult, MethodologyVersion>> CheckCanUpdateMethodologyVersion(
            this IUserService userService, MethodologyVersion methodologyVersion, bool ignoreCheck)
        {
            return ignoreCheck
                ? Task.FromResult(new Either<ActionResult, MethodologyVersion>(methodologyVersion))
                : userService.CheckCanUpdateMethodologyVersion(methodologyVersion);
        }

        public static Task<Either<ActionResult, MethodologyVersion>> CheckCanMarkMethodologyVersionAsDraft(
            this IUserService userService, MethodologyVersion methodologyVersion)
        {
            return userService.CheckPolicy(methodologyVersion, SecurityPolicies.CanMarkSpecificMethodologyAsDraft);
        }

        public static Task<Either<ActionResult, MethodologyVersion>> CheckCanApproveMethodologyVersion(
            this IUserService userService, MethodologyVersion methodologyVersion)
        {
            return userService.CheckPolicy(methodologyVersion, SecurityPolicies.CanApproveSpecificMethodology);
        }

        public static Task<Either<ActionResult, MethodologyVersion>> CheckCanMakeAmendmentOfMethodology(
            this IUserService userService, MethodologyVersion methodologyVersion)
        {
            return userService.CheckPolicy(methodologyVersion, SecurityPolicies.CanMakeAmendmentOfSpecificMethodology);
        }

        public static Task<Either<ActionResult, MethodologyVersion>> CheckCanDeleteMethodologyVersion(
            this IUserService userService, MethodologyVersion methodologyVersion, bool ignoreCheck = false)
        {
            return ignoreCheck
                ? Task.FromResult(new Either<ActionResult, MethodologyVersion>(methodologyVersion))
                : userService.CheckPolicy(methodologyVersion, SecurityPolicies.CanDeleteSpecificMethodology);
        }

        public static Task<Either<ActionResult, Unit>> CheckCanViewAllReleases(this IUserService userService)
        {
            return userService.CheckPolicy(SecurityPolicies.CanViewAllReleases);
        }

        public static Task<Either<ActionResult, Unit>> CheckCanViewAllPublications(this IUserService userService)
        {
            return userService.CheckPolicy(SecurityPolicies.CanViewAllPublications);
        }

        public static Task<Either<ActionResult, Unit>> CheckCanManageAllTaxonomy(this IUserService userService)
        {
            return userService.CheckPolicy(SecurityPolicies.CanManageAllTaxonomy);
        }

        // Publication

        public static Task<Either<ActionResult, Topic>> CheckCanCreatePublicationForTopic(
            this IUserService userService, Topic topic)
        {
            return userService.CheckPolicy(topic, SecurityPolicies.CanCreatePublicationForSpecificTopic);
        }

        public static Task<Either<ActionResult, Publication>> CheckCanUpdatePublicationSummary(
            this IUserService userService, Publication publication)
        {
            return userService.CheckPolicy(publication, SecurityPolicies.CanUpdateSpecificPublicationSummary);
        }

        public static Task<Either<ActionResult, Unit>> CheckCanUpdatePublication(
            this IUserService userService)
        {
            return userService.CheckPolicy(SecurityPolicies.CanUpdatePublication);
        }

        public static Task<Either<ActionResult, Publication>> CheckCanUpdateContact(
            this IUserService userService, Publication publication)
        {
            return userService.CheckPolicy(publication, SecurityPolicies.CanUpdateContact);
        }

        public static Task<Either<ActionResult, Publication>> CheckCanViewReleaseTeamAccess(
            this IUserService userService, Publication publication)
        {
            return userService.CheckPolicy(publication, SecurityPolicies.CanViewReleaseTeamAccess);
        }

        public static Task<Either<ActionResult, Tuple<Publication, ReleaseRole>>> CheckCanUpdateReleaseRole(
            this IUserService userService, Publication publication, ReleaseRole role)
        {
            return userService.CheckPolicy(TupleOf(publication, role),
                SecurityPolicies.CanUpdateSpecificReleaseRole);
        }

        public static Task<Either<ActionResult, Publication>> CheckCanCreateReleaseForPublication(
            this IUserService userService, Publication publication)
        {
            return userService.CheckPolicy(publication, SecurityPolicies.CanCreateReleaseForSpecificPublication);
        }

        public static Task<Either<ActionResult, Publication>> CheckCanViewPublication(
            this IUserService userService, Publication publication)
        {
            return userService.CheckPolicy(publication, SecurityPolicies.CanViewSpecificPublication);
        }

        public static Task<Either<ActionResult, Release>> CheckCanViewRelease(
            this IUserService userService, Release release)
        {
            return userService.CheckPolicy(release, ContentSecurityPolicies.CanViewSpecificRelease);
        }

        public static Task<Either<ActionResult, Release>> CheckCanUpdateRelease(
            this IUserService userService, Release release)
        {
            return userService.CheckPolicy(release, SecurityPolicies.CanUpdateSpecificRelease);
        }

        public static Task<Either<ActionResult, Release>> CheckCanUpdateRelease(
            this IUserService userService, Release release, bool ignoreCheck)
        {
            return ignoreCheck
                ? Task.FromResult(new Either<ActionResult, Release>(release))
                : userService.CheckCanUpdateRelease(release);
        }

        public static Task<Either<ActionResult, Release>> CheckCanDeleteRelease(
            this IUserService userService, Release release)
        {
            return userService.CheckPolicy(release, SecurityPolicies.CanDeleteSpecificRelease);
        }

        public static Task<Either<ActionResult, Release>> CheckCanViewReleaseStatusHistory(
            this IUserService userService, Release release)
        {
            return userService.CheckPolicy(release, SecurityPolicies.CanViewReleaseStatusHistory);
        }

        public static Task<Either<ActionResult, Release>> CheckCanUpdateReleaseStatus(
            this IUserService userService, Release release, ReleaseApprovalStatus approvalStatus)
        {
            switch (approvalStatus)
            {
                case ReleaseApprovalStatus.Draft:
                {
                    return userService.CheckCanMarkReleaseAsDraft(release);
                }
                case ReleaseApprovalStatus.HigherLevelReview:
                {
                    return userService.CheckCanSubmitReleaseToHigherApproval(release);
                }
                case ReleaseApprovalStatus.Approved:
                {
                    return userService.CheckCanApproveRelease(release);
                }
                default:
                {
                    return Task.FromResult(new Either<ActionResult, Release>(release));
                }
            }
        }

        public static Task<Either<ActionResult, Release>> CheckCanMarkReleaseAsDraft(
            this IUserService userService, Release release)
        {
            return userService.CheckPolicy(release, SecurityPolicies.CanMarkSpecificReleaseAsDraft);
        }

        public static Task<Either<ActionResult, Release>> CheckCanSubmitReleaseToHigherApproval(
            this IUserService userService, Release release)
        {
            return userService.CheckPolicy(release, SecurityPolicies.CanSubmitSpecificReleaseToHigherReview);
        }

        public static Task<Either<ActionResult, Release>> CheckCanApproveRelease(
            this IUserService userService, Release release)
        {
            return userService.CheckPolicy(release, SecurityPolicies.CanApproveSpecificRelease);
        }

        public static Task<Either<ActionResult, Release>> CheckCanMakeAmendmentOfRelease(
            this IUserService userService, Release release)
        {
            return userService.CheckPolicy(release, SecurityPolicies.CanMakeAmendmentOfSpecificRelease);
        }

        public static Task<Either<ActionResult, Unit>> CheckCanViewPrereleaseContactsList(
            this IUserService userService)
        {
            return userService.CheckPolicy(SecurityPolicies.CanViewPrereleaseContacts);
        }

        public static Task<Either<ActionResult, Release>> CheckCanAssignPrereleaseContactsToRelease(
            this IUserService userService, Release release)
        {
            return userService.CheckPolicy(release, SecurityPolicies.CanAssignPrereleaseContactsToSpecificRelease);
        }

        public static Task<Either<ActionResult, Release>> CheckCanViewPreReleaseSummary(
            this IUserService userService, Release release)
        {
            return userService.CheckPolicy(release, SecurityPolicies.CanViewSpecificPreReleaseSummary);
        }

        public static Task<Either<ActionResult, Release>> CheckCanPublishRelease(
            this IUserService userService, Release release)
        {
            return userService.CheckPolicy(release, SecurityPolicies.CanPublishSpecificRelease);
        }

        public static Task<Either<ActionResult, Comment>> CheckCanResolveComment(
            this IUserService userService, Comment comment)
        {
            return userService.CheckPolicy(comment, SecurityPolicies.CanResolveSpecificComment);
        }

        public static Task<Either<ActionResult, Comment>> CheckCanUpdateComment(
            this IUserService userService, Comment comment)
        {
            return userService.CheckPolicy(comment, SecurityPolicies.CanUpdateSpecificComment);
        }

        public static Task<Either<ActionResult, Comment>> CheckCanDeleteComment(
            this IUserService userService, Comment comment)
        {
            return userService.CheckPolicy(comment, SecurityPolicies.CanDeleteSpecificComment);
        }

        public static Task<Either<ActionResult, File>> CheckCanCancelFileImport(
            this IUserService userService, File file)
        {
            return userService.CheckPolicy(file, SecurityPolicies.CanCancelOngoingImports);
        }

        public static Task<Either<ActionResult, Publication>> CheckCanManageLegacyReleases(
            this IUserService userService, Publication publication)
        {
            return userService.CheckPolicy(publication, SecurityPolicies.CanManageLegacyReleases);
        }

        public static Task<Either<ActionResult, LegacyRelease>> CheckCanViewLegacyRelease(
            this IUserService userService, LegacyRelease legacyRelease)
        {
            return userService.CheckPolicy(legacyRelease, SecurityPolicies.CanViewLegacyRelease);
        }

        public static Task<Either<ActionResult, LegacyRelease>> CheckCanUpdateLegacyRelease(
            this IUserService userService, LegacyRelease legacyRelease)
        {
            return userService.CheckPolicy(legacyRelease, SecurityPolicies.CanUpdateLegacyRelease);
        }

        public static Task<Either<ActionResult, LegacyRelease>> CheckCanDeleteLegacyRelease(
            this IUserService userService, LegacyRelease legacyRelease)
        {
            return userService.CheckPolicy(legacyRelease, SecurityPolicies.CanDeleteLegacyRelease);
        }

        public static async Task<DataFilePermissions> GetDataFilePermissions(this IUserService userService, File file)
        {
            return new DataFilePermissions
            {
                CanCancelImport = (await userService.CheckCanCancelFileImport(file)).IsRight
            };
        }
    }
}
