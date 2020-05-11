using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces
{
    public interface IEES17PermalinkMigrationService
    {
        Task<bool> MigrateAll();
    }
}