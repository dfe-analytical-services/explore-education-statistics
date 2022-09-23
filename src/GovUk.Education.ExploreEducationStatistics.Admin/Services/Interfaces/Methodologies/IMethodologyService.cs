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

        Task<Either<ActionResult, MethodologyVersionViewModel>> CreateMethodology(Guid publicationId);

        Task<Either<ActionResult, Unit>> DeleteMethodology(Guid methodologyId, bool forceDelete = false);

        Task<Either<ActionResult, Unit>> DeleteMethodologyVersion(Guid methodologyVersionId, bool forceDelete = false);

        Task<Either<ActionResult, Unit>> DropMethodology(Guid publicationId, Guid methodologyId);

        Task<Either<ActionResult, List<MethodologyVersionViewModel>>> GetAdoptableMethodologies(
            Guid publicationId);

        Task<Either<ActionResult, MethodologyVersionViewModel>> GetMethodology(Guid methodologyVersionId);

        Task<Either<ActionResult, List<MethodologyVersionSummaryViewModel>>> ListMethodologies(Guid publicationId);

        Task<Either<ActionResult, List<IdTitleViewModel>>> GetUnpublishedReleasesUsingMethodology(
            Guid methodologyVersionId);

        Task<Either<ActionResult, MethodologyVersionViewModel>> UpdateMethodology(
            Guid methodologyVersionId,
            MethodologyUpdateRequest request);
    }
}
