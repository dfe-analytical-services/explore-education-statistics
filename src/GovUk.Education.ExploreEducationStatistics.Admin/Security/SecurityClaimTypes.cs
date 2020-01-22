namespace GovUk.Education.ExploreEducationStatistics.Admin.Security
{
    public enum SecurityClaimTypes
    {
        ApplicationAccessGranted,
        AnalystPagesAccessGranted,
        PrereleasePagesAccessGranted,

        ManageAnyUser,
        ManageAnyMethodology,
        
        CreateAnyPublication,
        CreateAnyRelease,

        AccessAllReleases,
        AccessAllTopics,
        
        UpdateAllReleases,
        
        MarkAllReleasesAsDraft,
        SubmitAllReleasesToHigherReview,
        ApproveAllReleases,
    }
}