using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class MeasureService : AbstractDataService<Measure, long>, IMeasureService
    {
        public MeasureService(ApplicationDbContext context, ILogger<MeasureService> logger)
            : base(context, logger)
        {
        }
    }
}