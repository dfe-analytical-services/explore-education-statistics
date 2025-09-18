#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IUserRepository
{
    Task<User?> FindByEmail(string email, CancellationToken cancellationToken = default);
    Task<User> FindDeletedUserPlaceholder(CancellationToken cancellationToken = default);
}
