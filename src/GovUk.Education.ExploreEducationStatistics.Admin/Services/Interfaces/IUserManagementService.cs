using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IUserManagementService
    {
        Task<UserViewModel> GetAsync(string userId);
        
        Task<List<UserViewModel>> ListAsync();

        List<UserReleaseRoleViewModel> GetUserReleaseRoles(string userId);
        
        Task<Either<ActionResult, UserReleaseRoleViewModel>> AddUserReleaseRole(Guid userId, UserReleaseRoleRequest userReleaseRole);

        Task<List<IdTitlePair>> ListReleasesAsync();

        Task<Either<ActionResult, bool>> RemoveUserReleaseRole(Guid userReleaseRoleId);

        Task<List<RoleViewModel>> ListRolesAsync();
        
        Task<List<UserViewModel>> ListPreReleaseUsersAsync();
        
        Task<Either<ActionResult, List<UserViewModel>>> ListPendingInvites();

        Task<Either<ActionResult, UserInvite>> InviteUser(string email, string user, string roleId);
        
        Task<Either<ActionResult, Unit>> CancelInvite(string email);

        Task<bool> UpdateAsync(string userId, string roleId);

    }
}
