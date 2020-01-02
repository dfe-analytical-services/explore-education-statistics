using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IUserManagementService
    {
        Task<List<UserViewModel>> ListAsync();

        Task<bool> InviteAsync(string email);
    }
}