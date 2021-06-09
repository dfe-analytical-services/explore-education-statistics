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
        CanUpdatePublication,
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
        CanUpdateSpecificComment,
        CanCancelOngoingImports,

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
        CanCreateMethodologies,
        CanViewAllMethodologies,
        CanViewSpecificMethodology,
        CanUpdateSpecificMethodology,
        CanMarkSpecificMethodologyAsDraft,
        CanApproveSpecificMethodology,
        CanMakeAmendmentOfSpecificMethodology
    }
}
