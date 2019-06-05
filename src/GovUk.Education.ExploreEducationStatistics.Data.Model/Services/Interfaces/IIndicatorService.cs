using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface IIndicatorService : IRepository<Indicator, long>
    {
        IEnumerable<Indicator> GetIndicators(long subjectId);
        
        IEnumerable<Indicator> GetIndicators(long subjectId, IEnumerable<long> indicatorIds);
    }
}