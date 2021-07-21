#nullable enable
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Database
{
    public class PublicStatisticsDbContext : StatisticsDbContext
    {
        public PublicStatisticsDbContext()
        {
        }

        public PublicStatisticsDbContext(DbContextOptions<PublicStatisticsDbContext> options, int? timeout = null) :
            base(options, timeout)
        {
        }
    }
}