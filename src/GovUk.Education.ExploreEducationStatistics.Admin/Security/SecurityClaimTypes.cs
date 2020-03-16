namespace GovUk.Education.ExploreEducationStatistics.Admin.Security
{
    public enum SecurityClaimTypes
    {
        /**
         * General role-based page access
         */
        ApplicationAccessGranted,
        AnalystPagesAccessGranted,
        PrereleasePagesAccessGranted,
        ManageAnyUser,
        ManageAnyMethodology,

        /**
         * Publication management
         */
        CreateAnyPublication,
        
        /**
         * Release management
         */
        CreateAnyRelease,
        AccessAllReleases,
        UpdateAllReleases,
        MarkAllReleasesAsDraft,
        SubmitAllReleasesToHigherReview,
        ApproveAllReleases,
        MakeAmendmentsOfAllReleases,
        DeleteAllReleaseAmendments,
        
        /**
         * Pre Release management
         */
        CanViewPrereleaseContacts,

        /**
         * Topic / Theme management
         */
        AccessAllTopics,
        
        /**
         * Methodology management
         */
        CreateAnyMethodology,
        AccessAllMethodologies,
        UpdateAllMethodologies,
        ApproveAllMethodologies
    }
}