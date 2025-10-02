#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IUserRepository
{
    Task<User?> FindByEmail(string email, CancellationToken cancellationToken = default);
    Task<User> FindDeletedUserPlaceholder(CancellationToken cancellationToken = default);

    Task<User> CreateOrUpdate(
        string email,
        Role role,
        Guid createdById,
        DateTime? createdDate = null);

    Task<User> CreateOrUpdate(
        string email,
        string roleId,
        Guid createdById,
        DateTime? createdDate = null);
}
