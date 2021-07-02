using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils
{
    public static class StartupUtils
    {
        public static void AddPersistenceHelper<TDbContext>(IServiceCollection services)
            where TDbContext : DbContext
        {
            services.AddTransient<IPersistenceHelper<TDbContext>, PersistenceHelper<TDbContext>>(
                s =>
                {
                    var dbContext = s.GetService<TDbContext>();
                    return new PersistenceHelper<TDbContext>(dbContext);
                }
            );
        }
    }
}
