using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class CreatePublicationForSpecificTopicRequirement : IAuthorizationRequirement
    {}
    
    public class CreatePublicationForSpecificTopicCanCreateForAnyTopicAuthorizationHandler : HasClaimAuthorizationHandler<
        CreatePublicationForSpecificTopicRequirement>
    {
        public CreatePublicationForSpecificTopicCanCreateForAnyTopicAuthorizationHandler() 
            : base(SecurityClaimTypes.CreateAnyPublication) {}
    }
}