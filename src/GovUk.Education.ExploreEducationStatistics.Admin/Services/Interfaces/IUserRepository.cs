#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IUserRepository
{
    Task<User?> FindById(Guid userId, CancellationToken cancellationToken = default);
    Task<User?> FindByEmail(string email);
}
