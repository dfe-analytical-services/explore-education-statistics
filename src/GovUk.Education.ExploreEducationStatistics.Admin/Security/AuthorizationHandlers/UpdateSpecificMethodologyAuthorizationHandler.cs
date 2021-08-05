using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class UpdateSpecificMethodologyRequirement : IAuthorizationRequirement
    {
    }

    public class UpdateSpecificMethodologyAuthorizationHandler : 
        AuthorizationHandler<UpdateSpecificMethodologyRequirement, Methodology>
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IMethodologyRepository _methodologyRepository;
        private readonly IPublicationRepository _publicationRepository;
        private readonly IUserPublicationRoleRepository _userPublicationRoleRepository;
        private readonly IUserReleaseRoleRepository _userReleaseRoleRepository;

        public UpdateSpecificMethodologyAuthorizationHandler(
            ContentDbContext contentDbContext,
            IMethodologyRepository methodologyRepository, 
            IPublicationRepository publicationRepository,
            IUserPublicationRoleRepository userPublicationRoleRepository, 
            IUserReleaseRoleRepository userReleaseRoleRepository)
        {
            _contentDbContext = contentDbContext;
            _methodologyRepository = methodologyRepository;
            _userPublicationRoleRepository = userPublicationRoleRepository;
            _userReleaseRoleRepository = userReleaseRoleRepository;
            _publicationRepository = publicationRepository;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            UpdateSpecificMethodologyRequirement requirement,
            Methodology methodology)
        {
            // An Approved Methodology cannot be updated.  Instead, it should firstly be unapproved if permissions
            // allow and then updated.
            if (methodology.Approved)
            {
                return;
            }
            
            // If the Methodology is already public, it cannot be updated.
            if (await _methodologyRepository.IsPubliclyAccessible(methodology.Id))
            {
                return;
            }

            // If the user has a global Claim that allows them to update any Methodology, allow it.
            if (SecurityUtils.HasClaim(context.User, UpdateAllMethodologies))
            {
                context.Succeed(requirement);
                return;
            }

            await _contentDbContext
                .Entry(methodology)
                .Reference(m => m.MethodologyParent)
                .LoadAsync();
            
            await _contentDbContext
                .Entry(methodology.MethodologyParent)
                .Collection(mp => mp.Publications)
                .LoadAsync();

            // A Publication Owner for the owning Publication of this Methodology can 
            var owningPublicationId = methodology
                .MethodologyParent
                .Publications
                .Single(p => p.Owner)
                .PublicationId;

            // If the user is a Publication Owner of the Publication that owns this Methodology, they can update it.
            if (await IsPublicationOwnerOfOwningPublication(context, owningPublicationId))
            {
                context.Succeed(requirement);
                return;
            }
            
            // If the user is an Editor (Contributor, Lead) or an Approver of the latest Release belonging to the
            // latest (Live or non-Live) Release for the owning Publication of this Methodology, they can update it.
            if (await IsEditorOrApproverOfOwningPublicationsLatestRelease(context, owningPublicationId))
            {
                context.Succeed(requirement);
            }

            // TODO SOW4 EES-2166 DON'T IMPLEMENT YET - When Status is Approved, succeed for Approvers on the latest Release of the owning Publication
        }

        private async Task<bool> IsPublicationOwnerOfOwningPublication(
            AuthorizationHandlerContext context, Guid owningPublicationId)
        {
            var publicationRoles = await _userPublicationRoleRepository
                .GetAllRolesByUser(context.User.GetUserId(), owningPublicationId);

            return ContainPublicationOwnerRole(publicationRoles);
        }
        
        private async Task<bool> IsEditorOrApproverOfOwningPublicationsLatestRelease(
            AuthorizationHandlerContext context, Guid owningPublicationId)
        {
            var latestRelease = await _publicationRepository.GetLatestReleaseForPublication(owningPublicationId);

            if (latestRelease == null)
            {
                return false;
            }
            
            var rolesForLatestRelease = await _userReleaseRoleRepository
                .GetAllRolesByUser(context.User.GetUserId(), latestRelease.Id);

            return ContainsEditorOrApproverRole(rolesForLatestRelease);
        }
    }
}
