using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public abstract class
        HasRoleOnPublicationAuthorizationHandler<TRequirement> : AuthorizationHandler<TRequirement, Publication>
        where TRequirement : IAuthorizationRequirement
    {
        private readonly IUserPublicationRoleRepository _publicationRoleRepository;
        private readonly Predicate<PublicationRolesAuthorizationContext> _roleTest;

        protected HasRoleOnPublicationAuthorizationHandler(IUserPublicationRoleRepository publicationRoleRepository,
            Predicate<PublicationRolesAuthorizationContext> roleTest)
        {
            _publicationRoleRepository = publicationRoleRepository;
            _roleTest = roleTest;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            TRequirement requirement,
            Publication publication)
        {
            var publicationRoles = await _publicationRoleRepository.GetAllRolesByUserAndPublication(context.User.GetUserId(), publication.Id);

            if (publicationRoles.Any())
            {
                if (_roleTest == null || _roleTest.Invoke(new PublicationRolesAuthorizationContext(publication, publicationRoles)))
                {
                    context.Succeed(requirement);    
                }
            }
        }
    }

    public class PublicationRolesAuthorizationContext
    {
        public PublicationRolesAuthorizationContext(Publication publication, List<PublicationRole> roles)
        {
            Publication = publication;
            Roles = roles;
        }

        public Publication Publication { get; set; }

        public List<PublicationRole> Roles { get; set; }
    }
}
