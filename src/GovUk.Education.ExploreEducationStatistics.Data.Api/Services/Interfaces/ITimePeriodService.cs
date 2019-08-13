using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces
{
    public interface ITimePeriodService
    {
        IEnumerable<(int Year, TimeIdentifier TimeIdentifier)> GetTimePeriods(IQueryable<Observation> observations);

        IEnumerable<(int Year, TimeIdentifier TimeIdentifier)>
            GetTimePeriodRange(IQueryable<Observation> observations);
    }
}