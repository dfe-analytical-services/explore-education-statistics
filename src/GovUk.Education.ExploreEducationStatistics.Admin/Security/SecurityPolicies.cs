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
        IsBauUser,

        /**
         * Publication management
         */
        CanViewAllPublications,
        CanViewSpecificPublication,
        CanUpdatePublication,
        CanUpdateContact,
        CanUpdateSpecificPublicationSummary,
        CanUpdateSpecificReleaseRole,
        CanCreatePublicationForSpecificTopic,
        CanViewReleaseTeamAccess,

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
        CanPublishSpecificRelease,
        CanDeleteSpecificRelease,
        CanViewSpecificPreReleaseSummary,
        CanResolveSpecificComment,
        CanUpdateSpecificComment,
        CanDeleteSpecificComment,
        CanCancelOngoingImports,
        CanViewReleaseStatusHistory,

        /**
         * Legacy release management
         */
        CanManageLegacyReleases,
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
