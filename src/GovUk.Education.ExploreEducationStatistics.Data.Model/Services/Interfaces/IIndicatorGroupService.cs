using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface IIndicatorGroupService : IRepository<IndicatorGroup, long>
    {
        IEnumerable<IndicatorGroup> GetIndicatorGroups(long subjectId);
    }
}