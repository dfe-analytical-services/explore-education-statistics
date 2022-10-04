#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces
{
    public interface IPermalinkService
    {
        Task<Either<ActionResult, LegacyPermalinkViewModel>> Get(Guid id);
        Task<Either<ActionResult, LegacyPermalinkViewModel>> Create(PermalinkCreateViewModel viewModel);
        Task<Either<ActionResult, LegacyPermalinkViewModel>> Create(Guid releaseId, PermalinkCreateViewModel request);
    }
}
