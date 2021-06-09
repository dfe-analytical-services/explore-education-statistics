using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies
{
    public interface IMethodologyService
    {
        Task<Either<ActionResult, MethodologySummaryViewModel>> CreateMethodology(Guid publicationId);
        
        Task<Either<ActionResult, List<MethodologyPublicationsViewModel>>> ListWithPublicationsAsync();

        Task<Either<ActionResult, MethodologySummaryViewModel>> GetSummaryAsync(Guid id);

        Task<Either<ActionResult, MethodologySummaryViewModel>> UpdateMethodologyAsync(Guid id,
            MethodologyUpdateRequest request);
    }
}
