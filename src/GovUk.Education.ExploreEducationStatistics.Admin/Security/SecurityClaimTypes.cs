namespace GovUk.Education.ExploreEducationStatistics.Admin.Security
{
    public enum SecurityClaimTypes
    {
        ApplicationAccessGranted,

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