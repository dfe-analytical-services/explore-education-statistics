#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces
{
    public interface ILocationRepository
    {
        Task<IList<Location>> GetDistinctForSubject(Guid subjectId);

        Task<Dictionary<GeographicLevel, IEnumerable<ILocationAttribute>>> GetLocationAttributes(Guid subjectId);

        IEnumerable<ILocationAttribute> GetLocationAttributes(
            GeographicLevel level,
            IEnumerable<string> codes);
    }
}
