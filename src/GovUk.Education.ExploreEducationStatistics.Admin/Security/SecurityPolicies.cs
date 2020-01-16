namespace GovUk.Education.ExploreEducationStatistics.Admin.Security
{
    public enum SecurityPolicies
    {
        CanAccessSystem,

        CanViewAllTopics,
        
        CanCreatePublicationForSpecificTopic,
        CanCreateReleaseForSpecificPublication,
        
        CanViewAllReleases,
        CanViewSpecificRelease,
        
        CanUpdateSpecificRelease,
        
        CanMarkSpecificReleaseAsDraft,
        CanSubmitSpecificReleaseToHigherReview,
        CanApproveSpecificRelease,
    }
}