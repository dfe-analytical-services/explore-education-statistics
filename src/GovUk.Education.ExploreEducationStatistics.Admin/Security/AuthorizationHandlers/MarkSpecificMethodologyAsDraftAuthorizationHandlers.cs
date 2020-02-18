using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class MarkSpecificMethodologyAsDraftRequirement : IAuthorizationRequirement
    {}
    
    public class MarkSpecificMethodologyAsDraftAuthorizationHandler : CompoundAuthorizationHandler<MarkSpecificMethodologyAsDraftRequirement, Methodology>
    {
        public MarkSpecificMethodologyAsDraftAuthorizationHandler(ContentDbContext context) : base(
            new CanMarkAllMethodologiesAsDraftAuthorizationHandler())
        {}
    }
    
    public class CanMarkAllMethodologiesAsDraftAuthorizationHandler : 
        EntityAuthorizationHandler<MarkSpecificMethodologyAsDraftRequirement, Methodology>
    {
        // TODO - for now, no-one can unapprove a Methodology, and you cannot mark it as Draft if it is already in
        // Draft, so currently there is no way to get it back into Draft (until work is done to allow BAU to do this 
        // via the UI)
        public CanMarkAllMethodologiesAsDraftAuthorizationHandler() 
            : base(ctx => false) {}
    }
}