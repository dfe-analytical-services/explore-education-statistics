#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces
{
    public interface ILocationRepository
    {
        Task<IList<Location>> GetDistinctForSubject(Guid subjectId);
    }
}
