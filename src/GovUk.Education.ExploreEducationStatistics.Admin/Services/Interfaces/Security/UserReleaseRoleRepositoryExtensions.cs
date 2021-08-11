#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security
{
    public static class UserReleaseRoleRepositoryExtensions
    {
        public static async Task<bool> IsUserApproverOnLatestRelease(
            this IUserReleaseRoleRepository repository,
            AuthorizationHandlerContext context,
            Guid publicationId)
        {
            return await repository.UserHasAnyOfRolesOnLatestRelease(
                context.User.GetUserId(),
                publicationId,
                ApproverRoles);
        }

        public static async Task<bool> IsUserEditorOrApproverOnLatestRelease(
            this IUserReleaseRoleRepository repository,
            AuthorizationHandlerContext context,
            Guid publicationId)
        {
            return await repository.UserHasAnyOfRolesOnLatestRelease(
                context.User.GetUserId(),
                publicationId,
                EditorAndApproverRoles);
        }
    }
}
