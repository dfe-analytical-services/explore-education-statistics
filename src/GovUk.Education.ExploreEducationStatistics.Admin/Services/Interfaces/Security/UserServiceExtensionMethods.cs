using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security
{
    public static class UserServiceExtensionMethods
    {
        public static Task<Either<ActionResult, Unit>> CheckCanAccessSystem(this IUserService userService)
        {
            return userService.DoCheck(SecurityPolicies.CanAccessSystem);
        }

        public static Task<Either<ActionResult, Unit>> CheckCanAccessAnalystPages(this IUserService userService)
        {
            return userService.DoCheck(SecurityPolicies.CanAccessAnalystPages);
        }

        public static Task<Either<ActionResult, Unit>> CheckCanAccessPrereleasePages(this IUserService userService)
        {
            return userService.DoCheck(SecurityPolicies.CanAccessPrereleasePages);
        }

        public static Task<Either<ActionResult, Unit>> CheckCanManageAllUsers(this IUserService userService)
        {
            return userService.DoCheck(SecurityPolicies.CanManageUsersOnSystem);
        }

        public static Task<Either<ActionResult, Unit>> CheckCanManageAllMethodologies(this IUserService userService)
        {
            return userService.DoCheck(SecurityPolicies.CanManageMethodologiesOnSystem);
        }

        public static Task<Either<ActionResult, Unit>> CheckCanViewAllMethodologies(this IUserService userService)
        {
            return userService.DoCheck(SecurityPolicies.CanViewAllMethodologies);
        }

        public static Task<Either<ActionResult, Unit>> CheckCanCreateMethodology(
            this IUserService userService)
        {
            return userService.DoCheck(SecurityPolicies.CanCreateMethodologies);
        }

        public static Task<Either<ActionResult, Methodology>> CheckCanViewMethodology(
            this IUserService userService, Methodology methodology)
        {
            return userService.DoCheck(methodology, SecurityPolicies.CanViewSpecificMethodology);
        }

        public static Task<Either<ActionResult, Methodology>> CheckCanUpdateMethodology(
            this IUserService userService, Methodology methodology)
        {
            return userService.DoCheck(methodology, SecurityPolicies.CanUpdateSpecificMethodology);
        }

        public static Task<Either<ActionResult, Methodology>> CheckCanMarkMethodologyAsDraft(
            this IUserService userService, Methodology methodology)
        {
            return userService.DoCheck(methodology, SecurityPolicies.CanMarkSpecificMethodologyAsDraft);
        }

        public static Task<Either<ActionResult, Methodology>> CheckCanApproveMethodology(
            this IUserService userService, Methodology methodology)
        {
            return userService.DoCheck(methodology, SecurityPolicies.CanApproveSpecificMethodology);
        }

        public static Task<Either<ActionResult, Unit>> CheckCanViewAllTopics(this IUserService userService)
        {
            return userService.DoCheck(SecurityPolicies.CanViewAllTopics);
        }

        public static Task<Either<ActionResult, Unit>> CheckCanViewAllReleases(this IUserService userService)
        {
            return userService.DoCheck(SecurityPolicies.CanViewAllReleases);
        }

        public static Task<Either<ActionResult, Theme>> CheckCanViewTheme(
            this IUserService userService, Theme theme)
        {
            return userService.DoCheck(theme, SecurityPolicies.CanViewSpecificTheme);
        }

        public static Task<Either<ActionResult, Topic>> CheckCanViewTopic(
            this IUserService userService, Topic topic)
        {
            return userService.DoCheck(topic, SecurityPolicies.CanViewSpecificTopic);
        }

        public static Task<Either<ActionResult, Unit>> CheckCanManageAllTaxonomy(this IUserService userService)
        {
            return userService.DoCheck(SecurityPolicies.CanManageAllTaxonomy);
        }

        // Publication

        public static Task<Either<ActionResult, Topic>> CheckCanCreatePublicationForTopic(
            this IUserService userService, Topic topic)
        {
            return userService.DoCheck(topic, SecurityPolicies.CanCreatePublicationForSpecificTopic);
        }

        public static Task<Either<ActionResult, Publication>> CheckCanUpdatePublication(
            this IUserService userService, Publication publication)
        {
            return userService.DoCheck(publication, SecurityPolicies.CanUpdatePublication);
        }

        public static Task<Either<ActionResult, Publication>> CheckCanCreateReleaseForPublication(
            this IUserService userService, Publication publication)
        {
            return userService.DoCheck(publication, SecurityPolicies.CanCreateReleaseForSpecificPublication);
        }

        public static Task<Either<ActionResult, Publication>> CheckCanViewPublication(
            this IUserService userService, Publication publication)
        {
            return userService.DoCheck(publication, SecurityPolicies.CanViewSpecificPublication);
        }

        public static Task<Either<ActionResult, Release>> CheckCanViewRelease(
            this IUserService userService, Release release)
        {
            return userService.DoCheck(release, SecurityPolicies.CanViewSpecificRelease);
        }

        public static Task<Either<ActionResult, Release>> CheckCanUpdateRelease(
            this IUserService userService, Release release)
        {
            return userService.DoCheck(release, SecurityPolicies.CanUpdateSpecificRelease);
        }

        public static Task<Either<ActionResult, Release>> CheckCanDeleteRelease(
            this IUserService userService, Release release)
        {
            return userService.DoCheck(release, SecurityPolicies.CanDeleteSpecificRelease);
        }

        public static Task<Either<ActionResult, Release>> CheckCanUpdateReleaseStatus(
            this IUserService userService, Release release, ReleaseStatus status)
        {
            switch (status)
            {
                case ReleaseStatus.Draft:
                {
                    return userService.CheckCanMarkReleaseAsDraft(release);
                }
                case ReleaseStatus.HigherLevelReview:
                {
                    return userService.CheckCanSubmitReleaseToHigherApproval(release);
                }
                case ReleaseStatus.Approved:
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
            return userService.DoCheck(release, SecurityPolicies.CanMarkSpecificReleaseAsDraft);
        }

        public static Task<Either<ActionResult, Release>> CheckCanSubmitReleaseToHigherApproval(
            this IUserService userService, Release release)
        {
            return userService.DoCheck(release, SecurityPolicies.CanSubmitSpecificReleaseToHigherReview);
        }

        public static Task<Either<ActionResult, Release>> CheckCanApproveRelease(
            this IUserService userService, Release release)
        {
            return userService.DoCheck(release, SecurityPolicies.CanApproveSpecificRelease);
        }

        public static Task<Either<ActionResult, Release>> CheckCanMakeAmendmentOfRelease(
            this IUserService userService, Release release)
        {
            return userService.DoCheck(release, SecurityPolicies.CanMakeAmendmentOfSpecificRelease);
        }

        public static Task<Either<ActionResult, Release>> CheckCanRunReleaseMigrations(
            this IUserService userService, Release release)
        {
            return userService.DoCheck(release, SecurityPolicies.CanRunReleaseMigrations);
        }

        public static Task<Either<ActionResult, Unit>> CheckCanViewPrereleaseContactsList(
            this IUserService userService)
        {
            return userService.DoCheck(SecurityPolicies.CanViewPrereleaseContacts);
        }

        public static Task<Either<ActionResult, Release>> CheckCanAssignPrereleaseContactsToRelease(
            this IUserService userService, Release release)
        {
            return userService.DoCheck(release, SecurityPolicies.CanAssignPrereleaseContactsToSpecificRelease);
        }

        public static Task<Either<ActionResult, Release>> CheckCanViewPreReleaseSummary(
            this IUserService userService, Release release)
        {
            return userService.DoCheck(release, SecurityPolicies.CanViewSpecificPreReleaseSummary);
        }

        public static Task<Either<ActionResult, Release>> CheckCanPublishRelease(
            this IUserService userService, Release release)
        {
            return userService.DoCheck(release, SecurityPolicies.CanPublishSpecificRelease);
        }

        public static Task<Either<ActionResult, Comment>> CheckCanUpdateComment(
            this IUserService userService, Comment comment)
        {
            return userService.DoCheck(comment, SecurityPolicies.CanUpdateSpecificComment);
        }

        public static Task<Either<ActionResult, Publication>> CheckCanCreateLegacyRelease(
            this IUserService userService, Publication publication)
        {
            return userService.DoCheck(publication, SecurityPolicies.CanCreateLegacyRelease);
        }

        public static Task<Either<ActionResult, LegacyRelease>> CheckCanViewLegacyRelease(
            this IUserService userService, LegacyRelease legacyRelease)
        {
            return userService.DoCheck(legacyRelease, SecurityPolicies.CanViewLegacyRelease);
        }

        public static Task<Either<ActionResult, LegacyRelease>> CheckCanUpdateLegacyRelease(
            this IUserService userService, LegacyRelease legacyRelease)
        {
            return userService.DoCheck(legacyRelease, SecurityPolicies.CanUpdateLegacyRelease);
        }

        public static Task<Either<ActionResult, LegacyRelease>> CheckCanDeleteLegacyRelease(
            this IUserService userService, LegacyRelease legacyRelease)
        {
            return userService.DoCheck(legacyRelease, SecurityPolicies.CanDeleteLegacyRelease);
        }

        private static async Task<Either<ActionResult, T>> DoCheck<T>(this IUserService userService, T resource, SecurityPolicies policy)
        {
            var result = await userService.MatchesPolicy(resource, policy);
            return result ? new Either<ActionResult, T>(resource) : new ForbidResult();
        }

        private static async Task<Either<ActionResult, Unit>> DoCheck(this IUserService userService, SecurityPolicies policy)
        {
            var result = await userService.MatchesPolicy(policy);
            return result ? new Either<ActionResult, Unit>(Unit.Instance) : new ForbidResult();
        }
    }
}