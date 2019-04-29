using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Data.Model.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface IObservationService : IDataService<Observation, long>
    {
        IEnumerable<Observation> FindObservations(Expression<Func<Observation, bool>> findExpression,
            IEnumerable<long> filters);

        LocationMeta GetLocationMeta(long subjectId);

        IEnumerable<(TimeIdentifier TimePeriod, int Year)> GetTimePeriodsMeta(long subjectId);
    }
}