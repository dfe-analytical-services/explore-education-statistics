using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies
{
    public interface IMethodologyService
    {
        Task<Either<ActionResult, List<MethodologyStatusViewModel>>> ListAsync();

        Task<Either<ActionResult, List<MethodologyPublicationsViewModel>>> ListWithPublicationsAsync();

        Task<Either<ActionResult, MethodologyStatusViewModel>> GetStatusAsync(Guid id);

        Task<Either<ActionResult, MethodologySummaryViewModel>> GetSummaryAsync(Guid id);

        Task<Either<ActionResult, MethodologyTitleViewModel>> CreateMethodologyAsync(
            CreateMethodologyRequest methodology);

        Task<Either<ActionResult, MethodologyStatusViewModel>> UpdateMethodologyStatusAsync(Guid methodologyId,
            UpdateMethodologyStatusRequest request);
    }
}