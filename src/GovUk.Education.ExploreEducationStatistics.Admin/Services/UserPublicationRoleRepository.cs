#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class UserPublicationRoleRepository : IUserPublicationRoleRepository
    {
        private readonly ContentDbContext _contentDbContext;

        public UserPublicationRoleRepository(ContentDbContext contentDbContext)
        {
            _contentDbContext = contentDbContext;
        }

        public async Task<UserPublicationRole> Create(Guid userId,
            Guid publicationId,
            PublicationRole role,
            Guid createdById)
        {
            var userPublicationRole = new UserPublicationRole
            {
                UserId = userId,
                PublicationId = publicationId,
                Role = role,
                Created = DateTime.UtcNow,
                CreatedById = createdById
            };

            var created = (await _contentDbContext.UserPublicationRoles.AddAsync(userPublicationRole)).Entity;
            await _contentDbContext.SaveChangesAsync();
            return created;
        }

        public async Task<List<PublicationRole>> GetAllRolesByUser(Guid userId, Guid publicationId)
        {
            return await _contentDbContext.UserPublicationRoles.Where(r =>
                    r.UserId == userId &&
                    r.PublicationId == publicationId)
                .Select(role => role.Role)
                .ToListAsync();
        }

        public async Task<bool> IsUserPublicationOwner(Guid userId, Guid publicationId)
        {
            return await UserHasRoleOnPublication(userId, publicationId, Owner);
        }

        public async Task<bool> UserHasRoleOnPublication(Guid userId, Guid publicationId, PublicationRole role)
        {
            return await _contentDbContext.UserPublicationRoles.AnyAsync(r =>
                r.UserId == userId &&
                r.PublicationId == publicationId &&
                r.Role == role);
        }
    }
}
