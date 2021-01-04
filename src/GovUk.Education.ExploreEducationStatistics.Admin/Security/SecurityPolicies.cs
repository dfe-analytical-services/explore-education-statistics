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
        CanManageMethodologiesOnSystem,
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
         * Topic / Theme management
         */
        CanViewAllTopics,
        CanViewSpecificTheme,
        CanViewSpecificTopic,

        /**
         * Methodology management
         */
        CanCreateMethodologies,
        CanViewAllMethodologies,
        CanViewSpecificMethodology,
        CanUpdateSpecificMethodology,
        CanMarkSpecificMethodologyAsDraft,
        CanApproveSpecificMethodology
    }
}