using System;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class SchoolService : AbstractRepository<School, Guid>, ISchoolService
    {
        public SchoolService(StatisticsDbContext context, ILogger<SchoolService> logger)
            : base(context, logger)
        {
        }
    }
}