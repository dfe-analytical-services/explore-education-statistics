using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;

public interface IDbContextService
{
    Task SaveChangesAsync(DbContext context);
}
