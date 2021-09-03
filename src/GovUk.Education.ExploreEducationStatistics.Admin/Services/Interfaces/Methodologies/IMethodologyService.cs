#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies
{
    public interface IMethodologyService
    {
        Task<Either<ActionResult, Unit>> AdoptMethodology(Guid publicationId, Guid methodologyId);

        Task<Either<ActionResult, MethodologySummaryViewModel>> CreateMethodology(Guid publicationId);

        Task<Either<ActionResult, Unit>> DropMethodology(Guid publicationId, Guid methodologyId);

        Task<Either<ActionResult, List<MethodologySummaryViewModel>>> GetAdoptableMethodologies(Guid publicationId);

        Task<Either<ActionResult, MethodologySummaryViewModel>> GetSummary(Guid methodologyVersionId);

        Task<Either<ActionResult, List<TitleAndIdViewModel>>> GetUnpublishedReleasesUsingMethodology(
            Guid methodologyVersionId);

        Task<Either<ActionResult, MethodologySummaryViewModel>> UpdateMethodology(Guid methodologyVersionId,
            MethodologyUpdateRequest request);

        Task<Either<ActionResult, Unit>> DeleteMethodologyVersion(Guid methodologyVersionId, bool forceDelete = false);
    }
}
