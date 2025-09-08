#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;

public interface IDataBlockService
{
    Task<Either<ActionResult, TableBuilderResultViewModel>> GetDataBlockTableResult(
        Guid releaseVersionId,
        Guid dataBlockVersionId
    );

    Task<Either<ActionResult, Dictionary<string, List<LocationAttributeViewModel>>>> GetLocationsForDataBlock(
        Guid releaseVersionId,
        Guid dataBlockVersionId,
        long boundaryLevelId
    );
}
