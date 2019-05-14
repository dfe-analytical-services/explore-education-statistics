using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class IndicatorGroupService : AbstractDataService<IndicatorGroup, long>, IIndicatorGroupService
    {
        public IndicatorGroupService(ApplicationDbContext context,
            ILogger<IndicatorGroupService> logger) : base(context, logger)
        {
        }

        public IEnumerable<IndicatorGroup> GetIndicatorGroups(long subjectId)
        {
            return FindMany(group => group.SubjectId == subjectId,
                new List<Expression<Func<IndicatorGroup, object>>> {group => group.Indicators});
        }
    }
}