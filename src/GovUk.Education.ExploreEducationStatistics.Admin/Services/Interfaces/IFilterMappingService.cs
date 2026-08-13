#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IFilterMappingService
{
    (FilterMapping? FilterMapping, ErrorViewModel? Error) UpdateFilterMapping(
        DataSetMapping dataSetMapping,
        Guid originalId,
        Guid? newReplacementId = null
    );
}
