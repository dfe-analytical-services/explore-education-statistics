#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Content.Security.AuthorizationHandlers;

public class ViewMethodologyVersionRequirement : IAuthorizationRequirement
{
}

public class ViewMethodologyVersionAuthorizationHandler :
    AuthorizationHandler<ViewMethodologyVersionRequirement, MethodologyVersion>
{
    private readonly IMethodologyVersionRepository _methodologyVersionRepository;

    public ViewMethodologyVersionAuthorizationHandler(IMethodologyVersionRepository methodologyVersionRepository)
    {
        _methodologyVersionRepository = methodologyVersionRepository;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        ViewMethodologyVersionRequirement requirement,
        MethodologyVersion methodologyVersion)
    {
        if (await _methodologyVersionRepository.IsPubliclyAccessible(methodologyVersion.Id))
        {
            context.Succeed(requirement);
        }
    }
}
