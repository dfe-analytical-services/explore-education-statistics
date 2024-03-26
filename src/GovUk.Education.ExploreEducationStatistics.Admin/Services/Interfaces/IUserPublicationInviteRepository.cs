#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IUserPublicationInviteRepository
{
    Task CreateManyIfNotExists(
        List<UserPublicationRoleCreateRequest> userPublicationRoles,
        string email,
        Guid createdById,
        DateTime? createdDate = null);

    Task<List<UserPublicationInvite>> ListByEmail(string email);
}
