#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;

public interface IMethodologyService
{
    public Task<Either<ActionResult, MethodologyVersionViewModel>> GetLatestMethodologyBySlug(string slug);

    public Task<Either<ActionResult, List<AllMethodologiesThemeViewModel>>> GetSummariesTree();
}
