namespace GovUk.Education.ExploreEducationStatistics.Admin.Security
{
    public enum SecurityPolicies
    {
        CanAccessSystem,

        CanManageUsersOnSystem,
        CanManageMethodologiesOnSystem,

        CanViewAllTopics,
        CanViewSpecificTheme,

        CanCreatePublicationForSpecificTopic,
        CanCreateReleaseForSpecificPublication,
        
        CanViewSpecificPublication,
        
        CanViewAllReleases,
        CanViewSpecificRelease,
        
        CanUpdateSpecificRelease,
        
        CanMarkSpecificReleaseAsDraft,
        CanSubmitSpecificReleaseToHigherReview,
        CanApproveSpecificRelease,
    }
}