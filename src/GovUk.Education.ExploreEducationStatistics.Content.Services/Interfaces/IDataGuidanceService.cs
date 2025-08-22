#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;

public interface IDataGuidanceService
{
    public Task<Either<ActionResult, DataGuidanceViewModel>> GetDataGuidance(string publicationSlug,
        string? releaseSlug = null);
}
