namespace GovUk.Education.ExploreEducationStatistics.Admin.Security
{
    public enum SecurityClaimTypes
    {
        ApplicationAccessGranted,
        AnalystPagesAccessGranted,
        PrereleasePagesAccessGranted,

        ManageAnyUser,
        ManageAnyMethodology,
        UpdateAllMethodologies,
        
        CreateAnyMethodology,
        CreateAnyPublication,
        CreateAnyRelease,

        AccessAllMethodologies,
        AccessAllReleases,
        AccessAllTopics,
        
        UpdateAllReleases,
        
        MarkAllReleasesAsDraft,
        SubmitAllReleasesToHigherReview,
        ApproveAllReleases,
        
        CanViewPrereleaseContacts
    }
}