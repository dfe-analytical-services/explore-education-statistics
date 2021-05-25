namespace GovUk.Education.ExploreEducationStatistics.Admin.Security
{
    // A set of Claims specific to the service that can be placed on Users' JWTs
    public enum UserClaimTypes
    {
        // the User Id for the user in the "internal" Users table as opposed to the AspNetUsers table 
        InternalUserId
    }
}