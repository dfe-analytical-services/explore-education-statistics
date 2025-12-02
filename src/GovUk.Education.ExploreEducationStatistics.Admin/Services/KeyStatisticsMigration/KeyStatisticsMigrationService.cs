#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.KeyStatisticsMigration.Dtos;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Predicates;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.KeyStatisticsMigration;

/// <summary>
/// TODO EES-6747 Remove after the Key Statistics migration is complete.
/// </summary>
public class KeyStatisticsMigrationService(ContentDbContext contentDbContext, IUserService userService)
    : IKeyStatisticsMigrationService
{
    public async Task<Either<ActionResult, KeyStatisticsMigrationReportDto>> MigrateKeyStatisticsGuidanceText(
        bool dryRun = true,
        CancellationToken cancellationToken = default
    ) =>
        await userService
            .CheckIsBauUser()
            .OnSuccess(async _ =>
            {
                // Retrieve the latest release versions for all releases (published or unpublished)
                var latestReleaseVersions = await contentDbContext
                    .ReleaseVersions.LatestReleaseVersions(publishedOnly: false)
                    .AsNoTracking()
                    .Include(rv => rv.Release.Publication.Theme)
                    .Include(rv => rv.KeyStatistics)
                    .ToListAsync(cancellationToken);

                // Identify all the latest versions that are draft, non-first versions
                var draftNonFirstLatestReleaseVersions = latestReleaseVersions
                    .Where(rv => rv is { Version: > 0, Published: null })
                    .ToList();

                // Identity all the releases with draft, non-first latest versions
                var releasesWithNonFirstDraftLatestReleaseVersions = draftNonFirstLatestReleaseVersions
                    .Select(rv => rv.ReleaseId)
                    .ToList();

                // For releases with a draft, non-first latest version, also retrieve the latest published version.
                // Note, this isn't necessarily the version directly preceding it as there can be multiple drafts
                // since the published version.
                var precedingLatestPublishedReleaseVersions = await contentDbContext
                    .ReleaseVersions.LatestReleaseVersions(publishedOnly: true)
                    .AsNoTracking()
                    .Where(rv => releasesWithNonFirstDraftLatestReleaseVersions.Contains(rv.ReleaseId))
                    .Include(rv => rv.Release.Publication.Theme)
                    .Include(rv => rv.KeyStatistics)
                    .ToListAsync(cancellationToken);

                // Combine the release versions for processing
                var releaseVersions = latestReleaseVersions.Concat(precedingLatestPublishedReleaseVersions).ToList();

                // Group the release versions by release and then by publication to build a report structure.
                // Release versions are included with their key statistics with both original and plain text guidance
                var publications = releaseVersions
                    .GroupBy(rv => rv.Release.Publication)
                    .Select(PublicationSelector())
                    .Where(p => p.Releases.Count > 0)
                    .OrderBy(p => p.Title)
                    .ToList();

                var keyStatistics = publications
                    .SelectMany(p => p.Releases)
                    .SelectMany(r => r.ReleaseVersions)
                    .SelectMany(rv => rv.KeyStatistics)
                    .ToList();

                // Identify the key statistics where the guidance text changed after conversion to plain text
                var keyStatisticsToUpdate = keyStatistics
                    .Where(ks => ks.HasGuidanceTextChanged)
                    .Select(ks => new KeyStatisticsMigrationReportKeyStatisticUpdateDto
                    {
                        KeyStatisticId = ks.KeyStatisticId,
                        GuidanceTextOriginal = ks.GuidanceTextOriginal!,
                        GuidanceTextPlain = ks.GuidanceTextPlain!,
                    })
                    .OrderBy(ks => ks.KeyStatisticId) // For consistent ordering during the migration and in the report
                    .ToList();

                if (!dryRun)
                {
                    await keyStatisticsToUpdate
                        .ToAsyncEnumerable()
                        .ForEachAwaitAsync(
                            ks => UpdateKeyStatisticGuidanceText(ks, cancellationToken),
                            cancellationToken
                        );

                    await contentDbContext.SaveChangesAsync(cancellationToken);
                }

                return new KeyStatisticsMigrationReportDto
                {
                    DryRun = dryRun,
                    KeyStatisticsCount = keyStatistics.Count,
                    MigrationResults = keyStatisticsToUpdate,
                    Publications = publications,
                };
            });

    private static Func<
        IGrouping<Publication, ReleaseVersion>,
        KeyStatisticsMigrationReportPublicationDto
    > PublicationSelector() =>
        publicationGroup =>
        {
            var publication = publicationGroup.Key;
            return new KeyStatisticsMigrationReportPublicationDto
            {
                PublicationId = publication.Id,
                Releases =
                [
                    .. publicationGroup
                        .GroupBy(rv => rv.Release)
                        .Select(ReleaseSelector())
                        .Where(r => r.ReleaseVersions.Count > 0)
                        .OrderBy(r => r.Title),
                ],
                Slug = publication.Slug,
                Theme = publication.Theme.Title,
                Title = publication.Title,
            };
        };

    private static Func<IGrouping<Release, ReleaseVersion>, KeyStatisticsMigrationReportReleaseDto> ReleaseSelector() =>
        releaseGroup =>
        {
            var release = releaseGroup.Key;
            return new KeyStatisticsMigrationReportReleaseDto
            {
                ReleaseId = release.Id,
                ReleaseVersions =
                [
                    .. releaseGroup
                        .Select(ReleaseVersionSelector())
                        .Where(r => r.KeyStatistics.Count > 0)
                        .OrderBy(rv => rv.Version),
                ],
                Slug = release.Slug,
                Title = release.Title,
            };
        };

    private static Func<ReleaseVersion, KeyStatisticsMigrationReportReleaseVersionDto> ReleaseVersionSelector() =>
        releaseVersion => new KeyStatisticsMigrationReportReleaseVersionDto
        {
            ReleaseVersionId = releaseVersion.Id,
            KeyStatistics = [.. releaseVersion.KeyStatistics.Select(KeyStatisticSelector()).OrderBy(ks => ks.Order)],
            Published = releaseVersion.Published,
            Version = releaseVersion.Version,
        };

    private static Func<KeyStatistic, KeyStatisticsMigrationReportKeyStatisticDto> KeyStatisticSelector() =>
        ks => new KeyStatisticsMigrationReportKeyStatisticDto
        {
            KeyStatisticId = ks.Id,
            GuidanceTextOriginal = ks.GuidanceText,
            GuidanceTextPlain = MarkdownToPlainText(ks.GuidanceText),
            Order = ks.Order,
        };

    private static string? MarkdownToPlainText(string? input)
    {
        if (string.IsNullOrEmpty(input?.Trim()))
        {
            return input;
        }

        // The input may already be plain text, in which case this should not alter it
        return Markdig.Markdown.ToPlainText(input).TrimEnd('\n');
    }

    private async Task UpdateKeyStatisticGuidanceText(
        KeyStatisticsMigrationReportKeyStatisticUpdateDto keyStatisticToUpdate,
        CancellationToken cancellationToken
    )
    {
        var keyStatistic = await contentDbContext.KeyStatistics.SingleAsync(
            ks => ks.Id == keyStatisticToUpdate.KeyStatisticId,
            cancellationToken
        );

        if (keyStatistic.GuidanceText != keyStatisticToUpdate.GuidanceTextOriginal)
        {
            throw new InvalidOperationException(
                $"KeyStatistic {keyStatistic.Id} guidance text has changed since report generation"
            );
        }

        keyStatistic.GuidanceText = keyStatisticToUpdate.GuidanceTextPlain;
    }
}
