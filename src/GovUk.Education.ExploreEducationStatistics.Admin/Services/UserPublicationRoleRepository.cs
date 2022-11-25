#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class UserPublicationRoleRepository : 
        AbstractUserResourceRoleRepository<UserPublicationRole, Publication, PublicationRole>, 
        IUserPublicationRoleRepository
    {
        public UserPublicationRoleRepository(ContentDbContext contentDbContext) : base(contentDbContext)
        {}

        protected override IQueryable<UserPublicationRole> GetResourceRolesQueryByResourceId(Guid publicationId)
        {
            return ContentDbContext
                .UserPublicationRoles
                .Where(role => role.PublicationId == publicationId);
        }

        protected override IQueryable<UserPublicationRole> GetResourceRolesQueryByResourceIds(List<Guid> publicationIds)
        {
            return ContentDbContext
                .UserPublicationRoles
                .Where(role => publicationIds.Contains(role.PublicationId));
        }

        public Task<List<PublicationRole>> GetDistinctRolesByUser(Guid userId)
        {
            return GetDistinctResourceRolesByUser(userId);
        }

        public Task<List<PublicationRole>> GetAllRolesByUserAndPublication(Guid userId, Guid publicationId)
        {
            return GetAllResourceRolesByUserAndResource(userId, publicationId);
        }

        public Task<bool> UserHasRoleOnPublication(Guid userId, Guid publicationId, PublicationRole role)
        {
            return UserHasRoleOnResource(userId, publicationId, role);
        }
    }
}