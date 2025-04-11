#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Security;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyApprovalStatus;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security
{
    public static class UserServiceExtensions
    {
        public static Task<Either<ActionResult, Unit>> CheckCanAccessSystem(this IUserService userService)
        {
            return userService.CheckPolicy(SecurityPolicies.RegisteredUser);
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
            this IUserService userService,
            PublicationMethodology methodology)
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
            this IUserService userService,
            Publication publication)
        {
            return userService.CheckPolicy(publication, SecurityPolicies.CanAdoptMethodologyForSpecificPublication);
        }

        public static Task<Either<ActionResult, Publication>> CheckCanCreateMethodologyForPublication(
            this IUserService userService,
            Publication publication)
        {
            return userService.CheckPolicy(publication, SecurityPolicies.CanCreateMethodologyForSpecificPublication);
        }

        public static Task<Either<ActionResult, Publication>> CheckCanManageExternalMethodologyForPublication(
            this IUserService userService,
            Publication publication)
        {
            return userService.CheckPolicy(publication,
                SecurityPolicies.CanManageExternalMethodologyForSpecificPublication);
        }

        public static Task<Either<ActionResult, MethodologyVersion>> CheckCanViewMethodology(
            this IUserService userService,
            MethodologyVersion methodologyVersion)
        {
            return userService.CheckPolicy(methodologyVersion, SecurityPolicies.CanViewSpecificMethodology);
        }

        public static Task<Either<ActionResult, MethodologyVersion>> CheckCanUpdateMethodologyVersion(
            this IUserService userService,
            MethodologyVersion methodologyVersion)
        {
            return userService.CheckPolicy(methodologyVersion, SecurityPolicies.CanUpdateSpecificMethodology);
        }

        public static Task<Either<ActionResult, MethodologyVersion>> CheckCanUpdateMethodologyVersion(
            this IUserService userService,
            MethodologyVersion methodologyVersion,
            bool ignoreCheck)
        {
            return ignoreCheck
                ? Task.FromResult(new Either<ActionResult, MethodologyVersion>(methodologyVersion))
                : userService.CheckCanUpdateMethodologyVersion(methodologyVersion);
        }

        public static Task<Either<ActionResult, MethodologyVersion>> CheckCanUpdateMethodologyVersionStatus(
            this IUserService userService,
            MethodologyVersion methodologyVersion,
            MethodologyApprovalStatus requestedStatus)
        {
            return requestedStatus switch
            {
                Draft => userService.CheckCanMarkMethodologyVersionAsDraft(methodologyVersion),
                HigherLevelReview => userService.CheckCanSubmitMethodologyForHigherReview(methodologyVersion),
                Approved => userService.CheckCanApproveMethodologyVersion(methodologyVersion),
                _ => throw new ArgumentOutOfRangeException(nameof(requestedStatus), "Unexpected status")
            };
        }

        public static Task<Either<ActionResult, MethodologyVersion>> CheckCanMarkMethodologyVersionAsDraft(
            this IUserService userService,
            MethodologyVersion methodologyVersion)
        {
            return userService.CheckPolicy(methodologyVersion, SecurityPolicies.CanMarkSpecificMethodologyAsDraft);
        }

        public static Task<Either<ActionResult, MethodologyVersion>> CheckCanSubmitMethodologyForHigherReview(
            this IUserService userService,
            MethodologyVersion methodologyVersion)
        {
            return userService.CheckPolicy(methodologyVersion,
                SecurityPolicies.CanSubmitSpecificMethodologyToHigherReview);
        }

        public static Task<Either<ActionResult, MethodologyVersion>> CheckCanApproveMethodologyVersion(
            this IUserService userService,
            MethodologyVersion methodologyVersion)
        {
            return userService.CheckPolicy(methodologyVersion, SecurityPolicies.CanApproveSpecificMethodology);
        }

        public static Task<Either<ActionResult, MethodologyVersion>> CheckCanMakeAmendmentOfMethodology(
            this IUserService userService,
            MethodologyVersion methodologyVersion)
        {
            return userService.CheckPolicy(methodologyVersion, SecurityPolicies.CanMakeAmendmentOfSpecificMethodology);
        }

        public static Task<Either<ActionResult, MethodologyVersion>> CheckCanDeleteMethodologyVersion(
            this IUserService userService,
            MethodologyVersion methodologyVersion,
            bool ignoreCheck = false)
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

        public static Task<Either<ActionResult, Theme>> CheckCanCreatePublicationForTheme(
            this IUserService userService,
            Theme theme)
        {
            return userService.CheckPolicy(theme, SecurityPolicies.CanCreatePublicationForSpecificTheme);
        }

        public static Task<Either<ActionResult, Publication>> CheckCanUpdatePublicationSummary(
            this IUserService userService,
            Publication publication)
        {
            return userService.CheckPolicy(publication, SecurityPolicies.CanUpdateSpecificPublicationSummary);
        }

        public static Task<Either<ActionResult, Unit>> CheckCanUpdatePublication(
            this IUserService userService)
        {
            return userService.CheckPolicy(SecurityPolicies.CanUpdatePublication);
        }

        public static Task<Either<ActionResult, Publication>> CheckCanUpdateContact(
            this IUserService userService,
            Publication publication)
        {
            return userService.CheckPolicy(publication, SecurityPolicies.CanUpdateContact);
        }

        public static Task<Either<ActionResult, Publication>> CheckCanViewReleaseTeamAccess(
            this IUserService userService,
            Publication publication)
        {
            return userService.CheckPolicy(publication, SecurityPolicies.CanViewReleaseTeamAccess);
        }

        public static Task<Either<ActionResult, Tuple<Publication, ReleaseRole>>> CheckCanUpdateReleaseRole(
            this IUserService userService,
            Publication publication,
            ReleaseRole role)
        {
            return userService.CheckPolicy(TupleOf(publication, role),
                SecurityPolicies.CanUpdateSpecificReleaseRole);
        }

        public static Task<Either<ActionResult, Publication>> CheckCanCreateReleaseForPublication(
            this IUserService userService,
            Publication publication)
        {
            return userService.CheckPolicy(publication, SecurityPolicies.CanCreateReleaseForSpecificPublication);
        }

        public static Task<Either<ActionResult, Release>> CheckCanUpdateRelease(
            this IUserService userService,
            Release release)
        {
            return userService.CheckPolicy(release, SecurityPolicies.CanUpdateSpecificRelease);
        }

        public static Task<Either<ActionResult, Publication>> CheckCanViewPublication(
            this IUserService userService,
            Publication publication)
        {
            return userService.CheckPolicy(publication, SecurityPolicies.CanViewSpecificPublication);
        }

        public static Task<Either<ActionResult, ReleaseVersion>> CheckCanViewReleaseVersion(
            this IUserService userService,
            ReleaseVersion releaseVersion)
        {
            return userService.CheckPolicy(releaseVersion, ContentSecurityPolicies.CanViewSpecificReleaseVersion);
        }

        public static Task<Either<ActionResult, ReleaseVersion>> CheckCanUpdateReleaseVersion(
            this IUserService userService,
            ReleaseVersion releaseVersion)
        {
            return userService.CheckPolicy(releaseVersion, SecurityPolicies.CanUpdateSpecificReleaseVersion);
        }

        public static Task<Either<ActionResult, ReleaseVersion>> CheckCanUpdateReleaseVersion(
            this IUserService userService,
            ReleaseVersion releaseVersion,
            bool ignoreCheck)
        {
            return ignoreCheck
                ? Task.FromResult(new Either<ActionResult, ReleaseVersion>(releaseVersion))
                : userService.CheckCanUpdateReleaseVersion(releaseVersion);
        }

        public static Task<Either<ActionResult, ReleaseVersion>> CheckCanDeleteReleaseVersion(
            this IUserService userService,
            ReleaseVersion releaseVersion)
        {
            return userService.CheckPolicy(releaseVersion, SecurityPolicies.CanDeleteSpecificReleaseVersion);
        }

        public static Task<Either<ActionResult, ReleaseVersion>> CheckCanDeleteTestReleaseVersion(
            this IUserService userService,
            ReleaseVersion releaseVersion)
        {
            return userService.CheckPolicy(releaseVersion, SecurityPolicies.CanDeleteTestRelease);
        }

        public static Task<Either<ActionResult, ReleaseVersion>> CheckCanViewReleaseVersionStatusHistory(
            this IUserService userService,
            ReleaseVersion releaseVersion)
        {
            return userService.CheckPolicy(releaseVersion, SecurityPolicies.CanViewReleaseStatusHistory);
        }

        public static Task<Either<ActionResult, ReleaseVersion>> CheckCanUpdateReleaseVersionStatus(
            this IUserService userService,
            ReleaseVersion releaseVersion,
            ReleaseApprovalStatus approvalStatus)
        {
            switch (approvalStatus)
            {
                case ReleaseApprovalStatus.Draft:
                    {
                        return userService.CheckCanMarkReleaseVersionAsDraft(releaseVersion);
                    }
                case ReleaseApprovalStatus.HigherLevelReview:
                    {
                        return userService.CheckCanSubmitReleaseVersionForHigherReview(releaseVersion);
                    }
                case ReleaseApprovalStatus.Approved:
                    {
                        return userService.CheckCanApproveReleaseVersion(releaseVersion);
                    }
                default:
                    {
                        return Task.FromResult(new Either<ActionResult, ReleaseVersion>(releaseVersion));
                    }
            }
        }

        public static Task<Either<ActionResult, ReleaseVersion>> CheckCanMarkReleaseVersionAsDraft(
            this IUserService userService,
            ReleaseVersion releaseVersion)
        {
            return userService.CheckPolicy(releaseVersion, SecurityPolicies.CanMarkSpecificReleaseAsDraft);
        }

        public static Task<Either<ActionResult, ReleaseVersion>> CheckCanSubmitReleaseVersionForHigherReview(
            this IUserService userService,
            ReleaseVersion releaseVersion)
        {
            return userService.CheckPolicy(releaseVersion, SecurityPolicies.CanSubmitSpecificReleaseToHigherReview);
        }

        public static Task<Either<ActionResult, ReleaseVersion>> CheckCanApproveReleaseVersion(
            this IUserService userService,
            ReleaseVersion releaseVersion)
        {
            return userService.CheckPolicy(releaseVersion, SecurityPolicies.CanApproveSpecificRelease);
        }

        public static Task<Either<ActionResult, ReleaseVersion>> CheckCanMakeAmendmentOfReleaseVersion(
            this IUserService userService,
            ReleaseVersion releaseVersion)
        {
            return userService.CheckPolicy(releaseVersion, SecurityPolicies.CanMakeAmendmentOfSpecificReleaseVersion);
        }

        public static Task<Either<ActionResult, ReleaseVersion>> CheckCanAssignPrereleaseContactsToReleaseVersion(
            this IUserService userService,
            ReleaseVersion releaseVersion)
        {
            return userService.CheckPolicy(releaseVersion, SecurityPolicies.CanAssignPreReleaseUsersToSpecificRelease);
        }

        public static Task<Either<ActionResult, ReleaseVersion>> CheckCanViewPreReleaseSummary(
            this IUserService userService,
            ReleaseVersion releaseVersion)
        {
            return userService.CheckPolicy(releaseVersion, SecurityPolicies.CanViewSpecificPreReleaseSummary);
        }

        public static Task<Either<ActionResult, ReleaseVersion>> CheckCanPublishReleaseVersion(
            this IUserService userService,
            ReleaseVersion releaseVersion)
        {
            return userService.CheckPolicy(releaseVersion, SecurityPolicies.CanPublishSpecificRelease);
        }

        public static Task<Either<ActionResult, Comment>> CheckCanResolveComment(
            this IUserService userService,
            Comment comment)
        {
            return userService.CheckPolicy(comment, SecurityPolicies.CanResolveSpecificComment);
        }

        public static Task<Either<ActionResult, Comment>> CheckCanUpdateComment(
            this IUserService userService,
            Comment comment)
        {
            return userService.CheckPolicy(comment, SecurityPolicies.CanUpdateSpecificComment);
        }

        public static Task<Either<ActionResult, Comment>> CheckCanDeleteComment(
            this IUserService userService,
            Comment comment)
        {
            return userService.CheckPolicy(comment, SecurityPolicies.CanDeleteSpecificComment);
        }

        public static Task<Either<ActionResult, File>> CheckCanCancelFileImport(
            this IUserService userService,
            File file)
        {
            return userService.CheckPolicy(file, SecurityPolicies.CanCancelOngoingImports);
        }

        public static Task<Either<ActionResult, Publication>> CheckCanManageReleaseSeries(
            this IUserService userService,
            Publication publication)
        {
            return userService.CheckPolicy(publication, SecurityPolicies.CanManagePublicationReleaseSeries);
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
