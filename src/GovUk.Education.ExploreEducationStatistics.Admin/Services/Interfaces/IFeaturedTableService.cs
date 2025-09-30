#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IFeaturedTableService
{
    public Task<Either<ActionResult, FeaturedTableViewModel>> Get(
        Guid releaseVersionId,
        Guid dataBlockId
    );

    public Task<Either<ActionResult, List<FeaturedTableViewModel>>> List(Guid releaseVersionId);

    public Task<Either<ActionResult, FeaturedTableViewModel>> Create(
        Guid releaseVersionId,
        FeaturedTableCreateRequest featuredTableCreateRequest
    );

    public Task<Either<ActionResult, FeaturedTableViewModel>> Update(
        Guid releaseVersionId,
        Guid dataBlockId,
        FeaturedTableUpdateRequest request
    );

    public Task<Either<ActionResult, Unit>> Delete(Guid releaseVersionId, Guid dataBlockId);

    public Task<Either<ActionResult, List<FeaturedTableViewModel>>> Reorder(
        Guid releaseVersionId,
        List<Guid> newOrder
    );
}
