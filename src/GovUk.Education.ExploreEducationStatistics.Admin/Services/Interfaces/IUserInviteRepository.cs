#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data.Models;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
    
public interface IUserInviteRepository
{
    Task<UserInvite> CreateOrUpdate(
        string email, 
        Role role, 
        Guid createdById, 
        DateTime? createdDate = null);

    Task<UserInvite> CreateOrUpdate(
        string email, 
        string roleId, 
        Guid createdById, 
        DateTime? createdDate = null);
}