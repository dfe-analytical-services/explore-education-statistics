#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces
{
    public interface ITimePeriodService
    {
        Task<IList<(int Year, TimeIdentifier TimeIdentifier)>> GetTimePeriods(Guid subjectId);

        Task<IList<(int Year, TimeIdentifier TimeIdentifier)>>
            GetTimePeriods(IQueryable<Observation> observationsQuery);

        IList<(int Year, TimeIdentifier TimeIdentifier)> GetTimePeriodRange(IList<Observation> observations);

        Task<TimePeriodLabels> GetTimePeriodLabels(Guid subjectId);
    }
}
