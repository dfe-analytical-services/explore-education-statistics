using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Model.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface IObservationService : IDataService<Observation, long>
    {
        LocationMeta GetLocationMeta(long subjectId);
        IEnumerable<(TimeIdentifier TimePeriod, int Year)> GetTimePeriodsMeta(long subjectId);
    }
}