#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;

public interface IMethodologyCacheService
{
    Task<Either<ActionResult, List<AllMethodologiesThemeViewModel>>> GetMethodologyTree();
    
    Task<Either<ActionResult, List<AllMethodologiesThemeViewModel>>> UpdateMethodologyTree();

    Task<Either<ActionResult, List<MethodologyVersionSummaryViewModel>>> GetMethodologiesByPublication(
        Guid publicationId);
}
