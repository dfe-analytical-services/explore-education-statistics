#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces
{
    public interface IPermalinkService
    {
        Task<Either<ActionResult, PermalinkViewModel>> Get(Guid id);
        Task<Either<ActionResult, PermalinkViewModel>> Create(CreatePermalinkRequest request);
        Task<Either<ActionResult, PermalinkViewModel>> Create(Guid releaseId, CreatePermalinkRequest request);
    }
}
