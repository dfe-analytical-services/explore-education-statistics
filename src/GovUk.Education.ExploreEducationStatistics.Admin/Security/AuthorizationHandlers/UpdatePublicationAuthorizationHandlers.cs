using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class UpdatePublicationRequirement : IAuthorizationRequirement
    {
    }

    public class UpdatePublicationAuthorizationHandler : CompoundAuthorizationHandler<
        UpdatePublicationRequirement, Publication>
    {
        public UpdatePublicationAuthorizationHandler(IUserPublicationRoleRepository userPublicationRoleRepository)
            : base(new CanUpdateAllPublications(),
                new HasOwnerRoleOnPublicationAuthorizationHandler(userPublicationRoleRepository))
        {
        }

        public class CanUpdateAllPublications : HasClaimAuthorizationHandler<
            UpdatePublicationRequirement>
        {
            public CanUpdateAllPublications()
                : base(SecurityClaimTypes.UpdateAllPublications)
            {
            }
        }

        public class HasOwnerRoleOnPublicationAuthorizationHandler
            : HasRoleOnPublicationAuthorizationHandler<UpdatePublicationRequirement>
        {
            public HasOwnerRoleOnPublicationAuthorizationHandler(
                IUserPublicationRoleRepository publicationRoleRepository) : base(publicationRoleRepository, context =>
                ContainPublicationOwnerRole(context.Roles))
            {
            }
        }
    }
}
