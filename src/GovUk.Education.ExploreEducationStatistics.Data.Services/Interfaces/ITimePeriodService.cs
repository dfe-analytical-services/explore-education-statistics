using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces
{
    public interface ITimePeriodService
    {
        IEnumerable<(int Year, TimeIdentifier TimeIdentifier)> GetTimePeriods(Guid subjectId);

        IEnumerable<(int Year, TimeIdentifier TimeIdentifier)> GetTimePeriods(IQueryable<Observation> observations);

        IEnumerable<(int Year, TimeIdentifier TimeIdentifier)> GetTimePeriodRange(IQueryable<Observation> observations);

        TimePeriodLabels GetTimePeriodLabels(Guid subjectId);
    }
}
