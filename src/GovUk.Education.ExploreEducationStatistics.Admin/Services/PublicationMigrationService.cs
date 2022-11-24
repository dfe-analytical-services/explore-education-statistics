#nullable enable
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

/// <summary>
/// TODO EES-3882 Remove after migration has been run by EES-3894
/// </summary>
public class PublicationMigrationService : IPublicationMigrationService
{
    private readonly ContentDbContext _contentDbContext;
    private readonly IUserService _userService;
    private readonly ILogger<PublicationMigrationService> _logger;

    public PublicationMigrationService(
        ContentDbContext contentDbContext,
        IUserService userService,
        ILogger<PublicationMigrationService> logger)
    {
        _contentDbContext = contentDbContext;
        _userService = userService;
        _logger = logger;
    }

    public async Task<Either<ActionResult, Unit>> SetLatestPublishedReleases()
    {
        return await _userService.CheckCanRunMigrations()
            .OnSuccessVoid(async () =>
            {
                // Get the list of publication id's to migrate filtering out
                // publications with a LatestPublishedReleaseId already set and publications without Releases
                var publicationsIdsToMigrate = await _contentDbContext.Publications
                    .Where(p => !p.LatestPublishedReleaseId.HasValue && p.Releases.Any())
                    .Select(p => p.Id)
                    .ToListAsync();

                var totalCount = publicationsIdsToMigrate.Count;
                _logger.LogInformation("Found {count} Publications to migrate", totalCount);

                // Iterate through the publications setting LatestPublishedReleaseId
                await publicationsIdsToMigrate
                    .ToAsyncEnumerable()
                    .ForEachAwaitAsync(async (publicationId, index) =>
                    {
                        _logger.LogInformation("Migrating Publication {index}/{totalCount}: {publicationId}",
                            index + 1,
                            totalCount,
                            publicationId);

                        var publication = await _contentDbContext.Publications
                            .SingleAsync(publication => publication.Id == publicationId);

                        await _contentDbContext
                            .Entry(publication)
                            .Collection(p => p.Releases)
                            .LoadAsync();

                        var publishedReleases = publication.GetPublishedReleases();
                        if (publishedReleases.Any())
                        {
                            var latestPublishedRelease = publishedReleases
                                .OrderBy(r => r.Year)
                                .ThenBy(r => r.TimePeriodCoverage)
                                .Last();

                            _logger.LogDebug(
                                "Setting LatestPublishedRelease: {latestPublishedReleaseId} for Publication: {publicationId}",
                                latestPublishedRelease.Id,
                                publicationId);

                            publication.LatestPublishedReleaseId = latestPublishedRelease.Id;
                            _contentDbContext.Update(publication);
                            await _contentDbContext.SaveChangesAsync();
                        }
                        else
                        {
                            _logger.LogDebug("Ignoring Publication with no published Release: {publicationId}",
                                publicationId);
                        }
                    });
            });
    }
}
