#nullable enable
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;

public interface IBoundaryDataRepository
{
    Dictionary<string, BoundaryData> FindByBoundaryLevelAndCodes(long boundaryLevelId, IEnumerable<string> codes);
}
