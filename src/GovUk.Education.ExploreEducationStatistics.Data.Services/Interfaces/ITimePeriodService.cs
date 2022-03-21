#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces
{
    public interface ITimePeriodService
    {
        IList<(int Year, TimeIdentifier TimeIdentifier)> GetTimePeriods(Guid subjectId);

        IList<(int Year, TimeIdentifier TimeIdentifier)> GetTimePeriods(IQueryable<Observation> observations);

        IList<(int Year, TimeIdentifier TimeIdentifier)> GetTimePeriodRange(IList<Observation> observations);

        TimePeriodLabels GetTimePeriodLabels(Guid subjectId);
    }
}
