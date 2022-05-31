#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces
{
    public interface IIndicatorGroupRepository
    {
        Task<List<IndicatorGroup>> GetIndicatorGroups(Guid subjectId);
    }
}
