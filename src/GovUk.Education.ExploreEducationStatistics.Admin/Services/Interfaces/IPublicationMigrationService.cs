#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

/// <summary>
/// TODO EES-3882 Remove after migration has been run by EES-3894
/// </summary>
public interface IPublicationMigrationService
{
    Task<Either<ActionResult, Unit>> SetLatestPublishedReleases();
}
