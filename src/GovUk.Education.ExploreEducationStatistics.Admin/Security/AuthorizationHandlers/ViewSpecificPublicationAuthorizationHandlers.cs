using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class ViewSpecificPublicationRequirement : IAuthorizationRequirement { }

public class ViewSpecificPublicationAuthorizationHandler
    : AuthorizationHandler<ViewSpecificPublicationRequirement, Publication>
{
    private readonly ContentDbContext _contentDbContext;
    private readonly AuthorizationHandlerService _authorizationHandlerService;

    public ViewSpecificPublicationAuthorizationHandler(
        ContentDbContext contentDbContext,
        AuthorizationHandlerService authorizationHandlerService
    )
    {
        _contentDbContext = contentDbContext;
        _authorizationHandlerService = authorizationHandlerService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ViewSpecificPublicationRequirement requirement,
        Publication publication
    )
    {
        // If the user has the "AccessAllPublications" Claim, they can see any Publication.
        if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.AccessAllPublications))
        {
            context.Succeed(requirement);
            return;
        }

        // This will be changed when we start introducing the use of the NEW publication roles in the
        // authorisation handlers, in STEP 8 (EES-6194) of the Permissions Rework. For now, we want to
        // filter out any usage of the NEW roles.
        var validPublicationRoles = EnumUtil
            .GetEnums<PublicationRole>()
            .Except([PublicationRole.Approver, PublicationRole.Drafter]);

        // If the user has any PublicationRole on the Publication, they can see it.
        if (
            await _authorizationHandlerService.UserHasAnyPublicationRoleOnPublication(
                context.User.GetUserId(),
                publication.Id,
                [.. validPublicationRoles]
            )
        )
        {
            context.Succeed(requirement);
            return;
        }

        // If the user has any ReleaseRoles on any of the Publication's Releases, they can see it.
        if (
            await _contentDbContext
                .UserReleaseRoles.Include(r => r.ReleaseVersion)
                .Where(r => r.UserId == context.User.GetUserId())
                .AnyAsync(r => r.ReleaseVersion.PublicationId == publication.Id)
        )
        {
            context.Succeed(requirement);
        }
    }
}
