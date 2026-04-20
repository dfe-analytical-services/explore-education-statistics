#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IDataSetMappingService
{
    Task<DataSetMapping> GetOrCreateMapping(
        Guid originalSubjectId,
        Guid replacementSubjectId,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, List<IndicatorMappingDto>>> UpdateIndicatorMappings(
        IndicatorMappingUpdatesRequest request
    );
}
