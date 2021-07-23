using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class IndicatorGroupService : AbstractRepository<IndicatorGroup, Guid>, IIndicatorGroupService
    {
        public IndicatorGroupService(StatisticsDbContext context) : base(context)
        {
        }

        public IEnumerable<IndicatorGroup> GetIndicatorGroups(Guid subjectId)
        {
            return FindMany(group => group.SubjectId == subjectId,
                new List<Expression<Func<IndicatorGroup, object>>> {group => group.Indicators});
        }
    }
}