using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class CreatePublicationForSpecificTopicRequirement : IAuthorizationRequirement
    {}
    
    public class CreatePublicationForSpecificTopicAuthorizationHandler : CompoundAuthorizationHandler<
        CreatePublicationForSpecificTopicRequirement, Topic>
    {
        public CreatePublicationForSpecificTopicAuthorizationHandler() 
            : base(new CanCreateForAnyTopicAuthorizationHandler()) {}
        
        public class CanCreateForAnyTopicAuthorizationHandler : HasClaimAuthorizationHandler<
            CreatePublicationForSpecificTopicRequirement>
        {
            public CanCreateForAnyTopicAuthorizationHandler() 
                : base(SecurityClaimTypes.CreateAnyPublication) {}
        }
    }
}