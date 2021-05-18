using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class UserPublicationRoleRepository : IUserPublicationRoleRepository
    {
        private readonly ContentDbContext _contentDbContext;

        public UserPublicationRoleRepository(ContentDbContext contentDbContext)
        {
            _contentDbContext = contentDbContext;
        }

        public async Task<UserPublicationRole> GetByRole(Guid userId, Guid publicationId, PublicationRole role)
        {
            return await _contentDbContext.UserPublicationRoles.FirstOrDefaultAsync(r =>
                r.UserId == userId &&
                r.PublicationId == publicationId &&
                r.Role == role);
        }
    }
}
