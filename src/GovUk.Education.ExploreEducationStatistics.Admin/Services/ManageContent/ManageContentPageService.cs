#nullable enable
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent;

public class ManageContentPageService(
    ContentDbContext contentDbContext,
    IPersistenceHelper<ContentDbContext> persistenceHelper,
    IMapper mapper,
    IDataBlockService dataBlockService,
    IMethodologyVersionRepository methodologyVersionRepository,
    IReleaseFileService releaseFileService,
    IReleaseRepository releaseRepository,
    IUserService userService
) : IManageContentPageService
{
    public async Task<Either<ActionResult, ManageContentPageViewModel>> GetManageContentPageViewModel(
        Guid releaseVersionId,
        bool isPrerelease = false,
        CancellationToken cancellationToken = default
    ) =>
        await persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId, HydrateReleaseQuery)
            .OnSuccess(userService.CheckCanViewReleaseVersion)
            .OnSuccessCombineWith(_ => dataBlockService.GetUnattachedDataBlocks(releaseVersionId))
            .OnSuccessCombineWith(_ => releaseFileService.ListAll(releaseVersionId, Ancillary, FileType.Data))
            .OnSuccess(async releaseVersionBlocksAndFiles =>
            {
                var (releaseVersion, unattachedDataBlocks, files) = releaseVersionBlocksAndFiles;

                var publication = releaseVersion.Release.Publication;

                var methodologyVersions = await methodologyVersionRepository.GetLatestVersionByPublication(
                    publication.Id
                );

                if (isPrerelease)
                {
                    // Get latest approved methodology version
                    methodologyVersions = await methodologyVersions
                        .ToAsyncEnumerable()
                        .Select(
                            async (methodologyVersion, ct) =>
                            {
                                if (methodologyVersion.Status == MethodologyApprovalStatus.Approved)
                                {
                                    return methodologyVersion;
                                }

                                if (methodologyVersion.PreviousVersionId == null)
                                {
                                    return null;
                                }

                                // If there is a previous version, it must be approved, because cannot
                                // create an amendment for an unpublished version
                                return await contentDbContext.MethodologyVersions.SingleAsync(
                                    mv => mv.Id == methodologyVersion.PreviousVersionId,
                                    cancellationToken: ct
                                );
                            }
                        )
                        .WhereNotNull()
                        .ToListAsync(cancellationToken);
                }

                var publishedReleases = await releaseRepository.ListPublishedReleases(
                    publication.Id,
                    cancellationToken
                );

                var publicationViewModel = new ManageContentPageViewModel.PublicationViewModel
                {
                    Id = publication.Id,
                    Summary = publication.Summary,
                    Title = publication.Title,
                    Slug = publication.Slug,
                    Contact = publication.Contact,
                    ReleaseSeries = BuildReleaseSeriesItemViewModels(publication, publishedReleases),
                    Methodologies = methodologyVersions
                        .Select(mv => new IdTitleViewModel { Id = mv.Id, Title = mv.Title })
                        .ToList(),
                    ExternalMethodology =
                        publication.ExternalMethodology != null
                            ? new ExternalMethodology
                            {
                                Title = publication.ExternalMethodology.Title,
                                Url = publication.ExternalMethodology.Url,
                            }
                            : null,
                };

                var downloadFiles = files.ToList();

                var publishedDisplayDate = await CalculatePublishedDisplayDate(releaseVersion, cancellationToken);

                var releaseViewModel = new ManageContentPageViewModel.ReleaseViewModel
                {
                    Id = releaseVersion.Id,
                    Title = releaseVersion.Release.Title,
                    YearTitle = releaseVersion.Release.YearTitle,
                    CoverageTitle = releaseVersion.Release.TimePeriodCoverage.GetEnumLabel(),
                    ReleaseName = releaseVersion.Release.Year.ToString(),
                    Slug = releaseVersion.Release.Slug,
                    Type = releaseVersion.Type,
                    Published = releaseVersion.Published,
                    PublishedDisplayDate = publishedDisplayDate,
                    PublishScheduled = releaseVersion.PublishScheduled?.ToUkDateOnly(),
                    PublicationId = publication.Id,
                    Publication = publicationViewModel,
                    ApprovalStatus = releaseVersion.ApprovalStatus,
                    LatestRelease =
                        releaseVersion.Release.Publication.LatestPublishedReleaseVersionId == releaseVersion.Id,
                    DownloadFiles = downloadFiles,
                    HasPreReleaseAccessList = !releaseVersion.PreReleaseAccessList.IsNullOrEmpty(),
                    KeyStatistics = releaseVersion
                        .KeyStatistics.OrderBy(ks => ks.Order)
                        .Select(KeyStatisticViewModel.FromKeyStatistic)
                        .ToList(),
                    PublishingOrganisations = releaseVersion
                        .PublishingOrganisations.OrderBy(o => o.Title)
                        .Select(OrganisationViewModel.FromOrganisation)
                        .ToList(),
                    NextReleaseDate = releaseVersion.NextReleaseDate,
                    RelatedInformation = releaseVersion.RelatedInformation,
                    Updates = releaseVersion
                        .Updates.OrderByDescending(u => u.On)
                        .Select(ReleaseNoteViewModel.FromUpdate)
                        .ToList(),
                    Content = mapper.Map<List<ContentSectionViewModel>>(
                        releaseVersion.GenericContent.OrderBy(cs => cs.Order)
                    ),
                    SummarySection = mapper.Map<ContentSectionViewModel>(releaseVersion.SummarySection!),
                    HeadlinesSection = mapper.Map<ContentSectionViewModel>(releaseVersion.HeadlinesSection!),
                    KeyStatisticsSecondarySection = mapper.Map<ContentSectionViewModel>(
                        releaseVersion.KeyStatisticsSecondarySection!
                    ),
                    RelatedDashboardsSection = mapper.Map<ContentSectionViewModel>(
                        releaseVersion.RelatedDashboardsSection!
                    ),
                    WarningSection = mapper.Map<ContentSectionViewModel>(releaseVersion.WarningSection!),
                };

                return new ManageContentPageViewModel
                {
                    Release = releaseViewModel,
                    UnattachedDataBlocks = unattachedDataBlocks,
                };
            });

    private static List<ReleaseSeriesItemViewModel> BuildReleaseSeriesItemViewModels(
        Publication publication,
        List<Release> publishedReleases
    )
    {
        var publishedReleasesById = publishedReleases.ToDictionary(r => r.Id);
        return publication
            .ReleaseSeries
            // Only include release series items for legacy links and published releases
            .Where(rsi => rsi.IsLegacyLink || publishedReleasesById.ContainsKey(rsi.ReleaseId!.Value))
            .Select(rsi =>
            {
                if (rsi.IsLegacyLink)
                {
                    return new ReleaseSeriesItemViewModel
                    {
                        Description = rsi.LegacyLinkDescription!,
                        LegacyLinkUrl = rsi.LegacyLinkUrl,
                    };
                }

                var release = publishedReleasesById[rsi.ReleaseId!.Value];

                return new ReleaseSeriesItemViewModel
                {
                    Description = release.Title,
                    ReleaseId = release.Id,
                    ReleaseSlug = release.Slug,
                };
            })
            .ToList();
    }

    private static IQueryable<ReleaseVersion> HydrateReleaseQuery(IQueryable<ReleaseVersion> queryable) =>
        queryable
            // Using `AsSplitQuery` as the generated SQL without it is incredibly
            // inefficient. Previously, we had dealt with this by splitting out
            // individual queries and hydrating the release manually.
            // We should keep an eye on this in case `AsSplitQuery` is not as
            // performant as running individual queries, and revert this if required.
            .AsSplitQuery()
            .Include(rv => rv.Release)
                .ThenInclude(r => r.Publication)
                    .ThenInclude(p => p.Contact)
            .Include(rv => rv.Release)
                .ThenInclude(r => r.Publication)
                    .ThenInclude(p => p.Theme)
            .Include(rv => rv.PublishingOrganisations)
            .Include(rv => rv.Content)
                .ThenInclude(cs => cs.Content)
                    .ThenInclude(cb => cb.Comments)
                        .ThenInclude(c => c.CreatedBy)
            .Include(rv => rv.Content)
                .ThenInclude(cs => cs.Content)
                    .ThenInclude(cb => cb.LockedBy)
            .Include(rv => rv.Content)
                .ThenInclude(cs => cs.Content)
                    .ThenInclude(cb => (cb as EmbedBlockLink)!.EmbedBlock)
            .Include(rv => rv.KeyStatistics)
            .Include(rv => rv.Updates);

    /// <summary>
    /// Determines the date displayed as the published date for the release version. This depends on whether the release
    /// version is published and its approval status.
    /// </summary>
    private async Task<DateTimeOffset?> CalculatePublishedDisplayDate(
        ReleaseVersion releaseVersion,
        CancellationToken cancellationToken
    )
    {
        if (releaseVersion.PublishedDisplayDate != null)
        {
            // The release version has a published display date value because it's already been published, so use it.
            return releaseVersion.PublishedDisplayDate.Value;
        }

        // If the release version is approved but not yet published, return the published display date that will be set
        // when publishing completes. This is required for the preview and pre-release views.
        if (releaseVersion.ApprovalStatus == ReleaseApprovalStatus.Approved)
        {
            if (releaseVersion.Version == 0 || releaseVersion.UpdatePublishedDisplayDate)
            {
                // The published display date will be set to the current date when publishing completes,
                // so return the scheduled published date as an approximation of this.
                return releaseVersion.PublishScheduled
                    ?? throw new ArgumentException(
                        $"Expected approved release version '{releaseVersion.Id}' to have a publish scheduled date."
                    );
            }

            // The published display date will be inherited from the previous version when publishing
            // completes, so return that value.
            return await GetPreviousReleaseVersionPublishedDisplayDate(releaseVersion, cancellationToken);
        }

        // The release version is neither published nor approved, so the published display date cannot be determined.
        return null;
    }

    private async Task<DateTimeOffset> GetPreviousReleaseVersionPublishedDisplayDate(
        ReleaseVersion releaseVersion,
        CancellationToken cancellationToken
    )
    {
        await contentDbContext.Entry(releaseVersion).Reference(rv => rv.PreviousVersion).LoadAsync(cancellationToken);

        var previousVersion =
            releaseVersion.PreviousVersion
            ?? throw new ArgumentException(
                $"Expected release version '{releaseVersion.Id}' (v{releaseVersion.Version}) to have a previous version."
            );

        return previousVersion.PublishedDisplayDate
            ?? throw new ArgumentException(
                $"Expected previous release version '{previousVersion.Id}' to be published."
            );
    }
}
