#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;
using ContentSectionId = System.Guid;
using DataBlockId = System.Guid;
using ReleaseId = System.Guid;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IFeaturedTableService
{
    public Task<Either<ActionResult, FeaturedTableViewModel>> Get(Guid releaseId, Guid dataBlockId);

    public Task<Either<ActionResult, List<FeaturedTableViewModel>>> List(Guid releaseId);

    public Task<Either<ActionResult, FeaturedTableViewModel>> Create(Guid releaseId,
        FeaturedTableCreateRequest featuredTableCreateRequest);

    public Task<Either<ActionResult, FeaturedTableViewModel>> Update(
        Guid releaseId,
        Guid dataBlockId,
        FeaturedTableUpdateRequest request);

    public Task<Either<ActionResult, Unit>> Delete(Guid releaseId, Guid dataBlockId);

    public Task<Either<ActionResult, List<FeaturedTableViewModel>>> Reorder(
        Guid releaseId,
        List<Guid> newOrder);
}
