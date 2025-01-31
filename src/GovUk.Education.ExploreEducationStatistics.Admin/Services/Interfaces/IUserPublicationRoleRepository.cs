#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IUserPublicationRoleRepository
    {
        Task<UserPublicationRole> Create(Guid userId,
            Guid publicationId,
            PublicationRole role,
            Guid createdById);

        Task<List<PublicationRole>> GetDistinctRolesByUser(Guid userId);

        Task<List<PublicationRole>> GetAllRolesByUserAndPublication(Guid userId,
            Guid publicationId);

        Task<bool> UserHasRoleOnPublication(Guid userId,
            Guid publicationId,
            PublicationRole role);
        
        Task RemoveMany(List<UserPublicationRole> userPublicationRoles, Guid deletedById);
    }
}