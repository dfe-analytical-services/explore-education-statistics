#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels.Meta;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
public interface ILocationService
{
    Task<Dictionary<string, List<LocationAttributeViewModel>>> GetLocationViewModels(
        List<Location> locations,
        Dictionary<GeographicLevel, List<string>>? hierarchies,
        long? boundaryLevelId = null);
}
