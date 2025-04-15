namespace GovUk.Education.ExploreEducationStatistics.Admin.Security
{
    public enum SecurityPolicies
    {
        /**
         * Scope access when checking JWTs.
         */
        AuthenticatedByIdentityProvider,
        RegisteredUser,

        /**
         * General role-based page access
         */
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
        CanCreatePublicationForSpecificTheme,
        CanViewReleaseTeamAccess,
        CanManagePublicationReleaseSeries,

        /**
         * Release management
         */
        CanCreateReleaseForSpecificPublication,
        CanViewAllReleases,
        CanUpdateSpecificRelease,
        CanUpdateSpecificReleaseVersion,
        CanMarkSpecificReleaseAsDraft,
        CanSubmitSpecificReleaseToHigherReview,
        CanApproveSpecificRelease,
        CanMakeAmendmentOfSpecificReleaseVersion,
        CanPublishSpecificRelease,
        CanDeleteSpecificReleaseVersion,
        CanDeleteTestRelease,
        CanViewSpecificPreReleaseSummary,
        CanResolveSpecificComment,
        CanUpdateSpecificComment,
        CanDeleteSpecificComment,
        CanCancelOngoingImports,
        CanViewReleaseStatusHistory,

        /**
         * Pre Release management
         */
        CanAssignPreReleaseUsersToSpecificRelease,

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
        CanSubmitSpecificMethodologyToHigherReview,
        CanApproveSpecificMethodology,
        CanMakeAmendmentOfSpecificMethodology,
        CanDeleteSpecificMethodology,
    }
}
