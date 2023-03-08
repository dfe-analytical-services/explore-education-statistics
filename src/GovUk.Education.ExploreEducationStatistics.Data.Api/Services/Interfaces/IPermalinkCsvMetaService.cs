#nullable enable
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;

public interface IPermalinkCsvMetaService
{
    Task<Either<ActionResult, PermalinkCsvMetaViewModel>> GetCsvMeta(
        LegacyPermalink permalink,
        CancellationToken cancellationToken = default);
}
