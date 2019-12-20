namespace GovUk.Education.ExploreEducationStatistics.Admin.Security
{
    public enum SecurityPolicies
    {
        CanAccessSystem,
        
        CanViewAllTopics,
        
        CanViewAllReleases,
        CanViewSpecificRelease,
        
        CanMarkSpecificReleaseAsDraft,
        CanSubmitSpecificReleaseToHigherReview,
        CanApproveSpecificRelease,
    }
}