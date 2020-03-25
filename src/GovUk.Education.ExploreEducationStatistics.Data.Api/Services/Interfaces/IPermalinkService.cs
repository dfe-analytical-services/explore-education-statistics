using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces
{
    public interface IPermalinkService
    {
        Task<Either<ActionResult, PermalinkViewModel>> GetAsync(Guid id);

        Task<Either<ActionResult, PermalinkViewModel>> CreateAsync(TableBuilderQueryContext query);
    }
}