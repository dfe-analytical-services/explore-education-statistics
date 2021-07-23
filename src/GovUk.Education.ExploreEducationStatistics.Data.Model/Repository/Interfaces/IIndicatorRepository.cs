using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces
{
    public interface IIndicatorRepository : IRepository<Indicator, Guid>
    {
        IEnumerable<Indicator> GetIndicators(Guid subjectId, IEnumerable<Guid> indicatorIds = null);
    }
}