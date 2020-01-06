using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface IIndicatorGroupService : IRepository<IndicatorGroup, Guid>
    {
        IEnumerable<IndicatorGroup> GetIndicatorGroups(Guid subjectId);
    }
}