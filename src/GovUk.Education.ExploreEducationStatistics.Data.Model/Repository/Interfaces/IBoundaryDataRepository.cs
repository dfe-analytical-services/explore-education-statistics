#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
public interface IBoundaryDataRepository
{
    Task AddRange(List<BoundaryData> boundaryData);
}
