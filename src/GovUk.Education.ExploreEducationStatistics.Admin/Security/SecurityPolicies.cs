#nullable enable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security
{
    public enum SecurityPolicies
    {
        /**
         * General role-based page access
         */
        CanAccessSystem,
        CanAccessAnalystPages,
        CanAccessPrereleasePages,
        CanManageUsersOnSystem,
        CanAccessAllImports,

        /**
         * Publication management
         */
        CanViewSpecificPublication,
        CanUpdatePublicationTitles,
        CanUpdateSpecificPublication,
        CanCreatePublicationForSpecificTopic,

        /**
         * Release management
         */
        CanCreateReleaseForSpecificPublication,
        CanViewAllReleases,
        CanUpdateSpecificRelease,
        CanMarkSpecificReleaseAsDraft,
        CanSubmitSpecificReleaseToHigherReview,
        CanApproveSpecificRelease,
        CanMakeAmendmentOfSpecificRelease,
        CanRunReleaseMigrations,
        CanPublishSpecificRelease,
        CanDeleteSpecificRelease,
        CanViewSpecificPreReleaseSummary,
        CanResolveSpecificComment,
        CanUpdateSpecificComment,
        CanCancelOngoingImports,
        CanViewReleaseStatusHistory,

        /**
         * Legacy release management
         */
        CanCreateLegacyRelease,
        CanViewLegacyRelease,
        CanUpdateLegacyRelease,
        CanDeleteLegacyRelease,

        /**
         * Pre Release management
         */
        CanViewPrereleaseContacts,
        CanAssignPrereleaseContactsToSpecificRelease,

        /**
         * Publication management
         */
        CanManageAllTaxonomy,

        /**
         * Methodology management
         */
        CanAdoptMethodologyForSpecificPublication,
        CanCreateMethodologyForSpecificPublication,
        CanDropMethodologyLink,
        CanManageExternalMethodologyForSpecificPublication,
        CanViewSpecificMethodology,
        CanUpdateSpecificMethodology,
        CanMarkSpecificMethodologyAsDraft,
        CanApproveSpecificMethodology,
        CanMakeAmendmentOfSpecificMethodology,
        CanDeleteSpecificMethodology,
    }
}
