using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces
{
    public interface IIndicatorGroupRepository : IRepository<IndicatorGroup, Guid>
    {
        IEnumerable<IndicatorGroup> GetIndicatorGroups(Guid subjectId);
    }
}