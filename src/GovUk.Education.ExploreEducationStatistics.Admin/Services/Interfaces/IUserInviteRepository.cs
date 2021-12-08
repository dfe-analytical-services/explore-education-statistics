#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IUserInviteRepository
    {
        public Task<UserInvite> CreateIfNoOtherUserInvite(string email, Role role, Guid createdById);

        public Task<UserInvite> CreateIfNoOtherUserInvite(string email, string roleId, Guid createdById);
    }
}
