#nullable enable
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Database
{
    public class PublicStatisticsDbContext : StatisticsDbContext
    {
        public PublicStatisticsDbContext()
        {
        }
        
        public PublicStatisticsDbContext(
            DbContextOptions<PublicStatisticsDbContext> options) :
            this(options, null)
        {
        }

        public PublicStatisticsDbContext(
            DbContextOptions<PublicStatisticsDbContext> options,
            int? timeout = null,
            bool updateTimestamps = true) :
            base(options, timeout, updateTimestamps)
        {
        }
    }
}