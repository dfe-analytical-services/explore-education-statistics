using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class SchoolService : AbstractDataService<School, long>, ISchoolService
    {
        public SchoolService(ApplicationDbContext context, ILogger<SchoolService> logger)
            : base(context, logger)
        {
        }
    }
}