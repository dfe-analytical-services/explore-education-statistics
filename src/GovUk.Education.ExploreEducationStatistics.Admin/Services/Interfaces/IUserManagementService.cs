using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IUserManagementService
    {
        Task<UserViewModel> GetAsync(string userId);
        
        Task<List<UserViewModel>> ListAsync();
        
        Task<List<RoleViewModel>> ListRolesAsync();
        
        Task<List<UserViewModel>> ListPreReleaseUsersAsync();
        
        Task<List<UserViewModel>> ListPendingAsync();

        Task<bool> InviteAsync(string email, string user, string roleId);
    }
}