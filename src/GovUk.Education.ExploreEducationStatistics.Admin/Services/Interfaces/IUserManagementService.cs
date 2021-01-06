using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;
using EnumExtensions = GovUk.Education.ExploreEducationStatistics.Common.Extensions.EnumExtensions;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IUserManagementService
    {
        Task<Either<ActionResult, UserViewModel>> GetUser(string userId);
        
        Task<Either<ActionResult, List<UserViewModel>>> ListAllUsers();

        Task<Either<ActionResult, List<UserReleaseRoleViewModel>>> GetUserReleaseRoles(string userId);
        
        Task<Either<ActionResult, Unit>> AddUserReleaseRole(Guid userId, UserReleaseRoleRequest userReleaseRole);

        Task<Either<ActionResult, Unit>> RemoveUserReleaseRole(Guid userReleaseRoleId);

        Task<Either<ActionResult, List<IdTitlePair>>> ListReleases();

        Task<Either<ActionResult, List<RoleViewModel>>> ListRoles();

        Task<Either<ActionResult, List<EnumExtensions.EnumValue>>> ListReleaseRoles();
        
        Task<List<UserViewModel>> ListPreReleaseUsersAsync();
        
        Task<Either<ActionResult, List<UserViewModel>>> ListPendingInvites();

        Task<Either<ActionResult, UserInvite>> InviteUser(string email, string user, string roleId);
        
        Task<Either<ActionResult, Unit>> CancelInvite(string email);

        Task<Either<ActionResult, Unit>> UpdateUser(string userId, string roleId);
    }
}
