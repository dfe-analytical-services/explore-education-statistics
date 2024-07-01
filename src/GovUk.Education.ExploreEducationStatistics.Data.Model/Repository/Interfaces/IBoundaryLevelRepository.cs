#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces
{
    public interface IBoundaryLevelRepository
    {
        Task<IEnumerable<BoundaryLevel>> ListBoundaryLevels();

        Task<BoundaryLevel?> GetBoundaryLevel(long id);

        Task<BoundaryLevel> CreateBoundaryLevel(
            GeographicLevel level,
            string label,
            DateTime published);

        Task UpdateBoundaryLevel(
            long id,
            string label);

        IEnumerable<BoundaryLevel> FindByGeographicLevels(IEnumerable<GeographicLevel> geographicLevels);
    }
}
