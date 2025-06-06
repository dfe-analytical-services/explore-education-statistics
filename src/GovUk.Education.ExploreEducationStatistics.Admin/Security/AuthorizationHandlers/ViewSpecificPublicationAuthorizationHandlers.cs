using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class ViewSpecificPublicationRequirement : IAuthorizationRequirement
{
}

public class
    ViewSpecificPublicationAuthorizationHandler : AuthorizationHandler<ViewSpecificPublicationRequirement,
    Publication>
{
    private readonly ContentDbContext _contentDbContext;
    private readonly AuthorizationHandlerService _authorizationHandlerService;

    public ViewSpecificPublicationAuthorizationHandler(
        ContentDbContext contentDbContext,
        AuthorizationHandlerService authorizationHandlerService)
    {
        _contentDbContext = contentDbContext;
        _authorizationHandlerService = authorizationHandlerService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ViewSpecificPublicationRequirement requirement,
        Publication publication)
    {
        // If the user has the "AccessAllPublications" Claim, they can see any Publication.
        if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.AccessAllPublications))
        {
            context.Succeed(requirement);
            return;
        }

        // If the user has any PublicationRole on the Publication, they can see it.
        if (await _authorizationHandlerService
                .HasRolesOnPublication(
                    context.User.GetUserId(),
                    publication.Id,
                    EnumUtil.GetEnumsArray<PublicationRole>()))
        {
            context.Succeed(requirement);
            return;
        }

        // If the user has any ReleaseRoles on any of the Publication's Releases, they can see it.
        if (await _contentDbContext
                .UserReleaseRoles
                .Include(r => r.ReleaseVersion)
                .Where(r => r.UserId == context.User.GetUserId())
                .AnyAsync(r => r.ReleaseVersion.PublicationId == publication.Id))
        {
            context.Succeed(requirement);
        }
    }
}
