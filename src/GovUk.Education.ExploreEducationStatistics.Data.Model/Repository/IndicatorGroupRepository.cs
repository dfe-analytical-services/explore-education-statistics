using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository
{
    public class IndicatorGroupRepository : AbstractRepository<IndicatorGroup, Guid>, IIndicatorGroupRepository
    {
        public IndicatorGroupRepository(StatisticsDbContext context) : base(context)
        {
        }

        public IEnumerable<IndicatorGroup> GetIndicatorGroups(Guid subjectId)
        {
            return FindMany(group => group.SubjectId == subjectId,
                new List<Expression<Func<IndicatorGroup, object>>> {group => group.Indicators});
        }
    }
}