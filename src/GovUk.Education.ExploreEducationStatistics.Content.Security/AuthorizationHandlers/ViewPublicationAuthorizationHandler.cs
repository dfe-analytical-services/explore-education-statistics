#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Content.Security.AuthorizationHandlers
{
    public class ViewPublicationRequirement : IAuthorizationRequirement
    {
    }

    public class ViewPublicationAuthorizationHandler
        : AuthorizationHandler<ViewPublicationRequirement, Publication>
    {
        private readonly ContentDbContext _context;

        public ViewPublicationAuthorizationHandler(ContentDbContext context)
        {
            _context = context;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext authContext,
            ViewPublicationRequirement requirement,
            Publication publication)
        {
            if (!_context.TryReloadEntity(publication, out var loadedPublication))
            {
                return;
            }

            await _context.Entry(loadedPublication)
                .Collection(p => p.Releases)
                .LoadAsync();

            var release = loadedPublication.LatestPublishedRelease();

            if (release is not null)
            {
                authContext.Succeed(requirement);
            }
        }
    }
}