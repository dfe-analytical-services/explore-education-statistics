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
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class MethodologyStatusAuthorizationHandlers
    {
        public class ApproveSpecificMethodologyRequirement : IAuthorizationRequirement
        {
        }

        public class ApproveSpecificMethodologyAuthorizationHandler :
            AuthorizationHandler<ApproveSpecificMethodologyRequirement, Methodology>
        {
            private readonly ContentDbContext _contentDbContext;
            private readonly IPublicationRepository _publicationRepository;
            private readonly IUserReleaseRoleRepository _userReleaseRoleRepository;

            public ApproveSpecificMethodologyAuthorizationHandler(
                ContentDbContext contentDbContext, 
                IPublicationRepository publicationRepository, 
                IUserReleaseRoleRepository userReleaseRoleRepository)
            {
                _contentDbContext = contentDbContext;
                _publicationRepository = publicationRepository;
                _userReleaseRoleRepository = userReleaseRoleRepository;
            }

            protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
                ApproveSpecificMethodologyRequirement requirement,
                Methodology methodology)
            {
                if (methodology.Status == Approved)
                {
                    return;
                }

                if (SecurityUtils.HasClaim(context.User, ApproveAllMethodologies))
                {
                    context.Succeed(requirement);
                    return;
                }
                
                var owningPublicationId = await GetOwningPublicationIdForMethodology(_contentDbContext, methodology);

                // If the user is an Approver of the latest (Live or non-Live) Release for the owning Publication of
                // this Methodology, they can update it.
                if (await IsApproverOfOwningPublicationsLatestRelease(
                    _publicationRepository, _userReleaseRoleRepository, context, owningPublicationId))
                {
                    context.Succeed(requirement);
                }
            }
        }

        public class MarkSpecificMethodologyAsDraftRequirement : IAuthorizationRequirement
        {
        }

        public class MarkSpecificMethodologyAsDraftAuthorizationHandler : AuthorizationHandler<MarkSpecificMethodologyAsDraftRequirement, Methodology>
        {
            private readonly ContentDbContext _contentDbContext;
            private readonly IMethodologyRepository _methodologyRepository;
            private readonly IPublicationRepository _publicationRepository;
            private readonly IUserReleaseRoleRepository _userReleaseRoleRepository;

            public MarkSpecificMethodologyAsDraftAuthorizationHandler(
                ContentDbContext contentDbContext, 
                IMethodologyRepository methodologyRepository, 
                IPublicationRepository publicationRepository, 
                IUserReleaseRoleRepository userReleaseRoleRepository)
            {
                _contentDbContext = contentDbContext;
                _methodologyRepository = methodologyRepository;
                _publicationRepository = publicationRepository;
                _userReleaseRoleRepository = userReleaseRoleRepository;
            }

            protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
                MarkSpecificMethodologyAsDraftRequirement requirement,
                Methodology methodology)
            {
                // If the Methodology is already public, it cannot be marked as draft
                if (await _methodologyRepository.IsPubliclyAccessible(methodology.Id))
                {
                    return;
                }

                if (SecurityUtils.HasClaim(context.User, MarkAllMethodologiesDraft))
                {
                    context.Succeed(requirement);
                    return;
                }
                
                var owningPublicationId = await GetOwningPublicationIdForMethodology(_contentDbContext, methodology);

                // If the user is an Approver of the latest (Live or non-Live) Release for the owning Publication of
                // this Methodology, they can update it.
                if (await IsApproverOfOwningPublicationsLatestRelease(
                    _publicationRepository, _userReleaseRoleRepository, context, owningPublicationId))
                {
                    context.Succeed(requirement);
                }
            }
        }

        // TODO SOW4 EES-2160 - DW - this could do with tidying up and merging with the similar code in
        // UpdateSpecificMethodologyAuthorizationHandler.  The number of services / repositories in use here suggests
        // that we could be putting a lot of this code into an existing Service or Repository and giving that to the
        // handler to reduce the amount of duplication we have.
        private static async Task<Guid> GetOwningPublicationIdForMethodology(
            ContentDbContext contentDbContext, 
            Methodology methodology)
        {
            await contentDbContext
                .Entry(methodology)
                .Reference(m => m.MethodologyParent)
                .LoadAsync();

            await contentDbContext
                .Entry(methodology.MethodologyParent)
                .Collection(mp => mp.Publications)
                .LoadAsync();

            return methodology
                .MethodologyParent
                .Publications
                .Single(p => p.Owner)
                .PublicationId;
        }

        // TODO SOW4 EES-2160 - DW - this could do with tidying up and merging with the similar code in
        // UpdateSpecificMethodologyAuthorizationHandler.  The number of services / repositories in use here suggests
        // that we could be putting a lot of this code into an existing Service or Repository and giving that to the
        // handler to reduce the amount of duplication we have.
        private static async Task<bool> IsApproverOfOwningPublicationsLatestRelease(
            IPublicationRepository publicationRepository,
            IUserReleaseRoleRepository userReleaseRoleRepository,
            AuthorizationHandlerContext context, 
            Guid owningPublicationId)
        {
            var latestRelease = await publicationRepository.GetLatestReleaseForPublication(owningPublicationId);

            if (latestRelease == null)
            {
                return false;
            }
            
            var rolesForLatestRelease = await userReleaseRoleRepository
                .GetAllRolesByUser(context.User.GetUserId(), latestRelease.Id);

            return ContainsApproverRole(rolesForLatestRelease);
        }
    }
}
