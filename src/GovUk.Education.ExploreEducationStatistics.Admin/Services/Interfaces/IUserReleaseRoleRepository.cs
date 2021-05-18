using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IUserReleaseRoleRepository
    {
        public Task<UserReleaseRole> Create(Guid userId, Guid releaseId, ReleaseRole role);

        public Task<UserReleaseRole> GetByRole(Guid userId, Guid publicationId, ReleaseRole role);
    }
}
