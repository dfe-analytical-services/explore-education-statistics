#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security
{
    public static class UserPublicationRoleRepositoryExtensions
    {
        public static async Task<bool> IsUserPublicationOwner(
            this IUserPublicationRoleRepository repository,
            AuthorizationHandlerContext context,
            Guid publicationId)
        {
            return await repository
                .UserHasRoleOnPublication(context.User.GetUserId(), publicationId, Owner);
        }
    }
}
