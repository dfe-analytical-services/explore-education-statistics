using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface IIndicatorService : IRepository<Indicator, Guid>
    {
        IEnumerable<Indicator> GetIndicators(Guid subjectId, IEnumerable<Guid> indicatorIds = null);
    }
}