#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IUserRepository
{
    Task<User?> FindPendingUserInviteByEmail(string email, CancellationToken cancellationToken = default);

    Task<User?> FindActiveUserByEmail(string email, CancellationToken cancellationToken = default);

    Task<User?> FindActiveUserById(Guid userId, CancellationToken cancellationToken = default);

    Task<User?> FindUserByEmail(string email, CancellationToken cancellationToken = default);

    Task<User> FindDeletedUserPlaceholder(CancellationToken cancellationToken = default);

    Task<User> CreateOrUpdate(
        string email,
        Role role,
        Guid createdById,
        DateTimeOffset? createdDate = null,
        CancellationToken cancellationToken = default
    );

    Task<User> CreateOrUpdate(
        string email,
        string roleId,
        Guid createdById,
        DateTimeOffset? createdDate = null,
        CancellationToken cancellationToken = default
    );

    Task SoftDeleteUser(User activeUser, Guid deletedById, CancellationToken cancellationToken = default);
}
