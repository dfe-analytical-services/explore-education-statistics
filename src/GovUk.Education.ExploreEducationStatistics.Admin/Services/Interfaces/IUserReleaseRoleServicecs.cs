#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IUserReleaseRoleService
    {
        Task<List<UserReleaseRole>> ListUserReleaseRolesByPublication(
            ReleaseRole role, Guid publicationId);
    }
}
