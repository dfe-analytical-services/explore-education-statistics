#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IUserRepository
{
    Task<User?> FindActiveUserByEmail(string email, CancellationToken cancellationToken = default);

    Task<User?> FindByEmail(string email, CancellationToken cancellationToken = default);

    Task<User> FindDeletedUserPlaceholder(CancellationToken cancellationToken = default);

    Task<User> CreateOrUpdate(
        string email,
        Role role,
        Guid createdById,
        DateTimeOffset? createdDate = null,
        CancellationToken cancellationToken = default);

    Task<User> CreateOrUpdate(
        string email,
        string roleId,
        Guid createdById,
        DateTimeOffset? createdDate = null,
        CancellationToken cancellationToken = default);

    Task SoftDeleteUser(
        User activeUser,
        Guid deletedById,
        CancellationToken cancellationToken = default);
}
