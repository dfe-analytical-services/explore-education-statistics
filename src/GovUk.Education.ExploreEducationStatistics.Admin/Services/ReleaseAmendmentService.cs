#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Utils.ContentFilterUtils;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseVersion;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class ReleaseAmendmentService(
    ContentDbContext context,
    IUserService userService,
    IFootnoteRepository footnoteRepository,
    StatisticsDbContext statisticsDbContext
) : IReleaseAmendmentService
{
    public async Task<Either<ActionResult, IdViewModel>> CreateReleaseAmendment(Guid releaseVersionId)
    {
        var createdDate = DateTime.UtcNow;

        return await context
            .ReleaseVersions.HydrateReleaseVersionForAmendment()
            .SingleOrDefault(releaseVersion => releaseVersion.Id == releaseVersionId)
            .OrNotFound()
            .OnSuccess(userService.CheckCanMakeAmendmentOfReleaseVersion)
            .OnSuccess(originalReleaseVersion =>
                CreateBasicReleaseAmendment(originalReleaseVersion, createdDate)
                    .OnSuccessDo(CreateStatisticsReleaseAmendment)
                    .OnSuccessDo(amendment => CopyFootnotes(releaseVersionId, amendment.Id))
                    .OnSuccess(amendment => CopyFileLinks(originalReleaseVersion, amendment))
                    .OnSuccess(amendment => new IdViewModel(amendment.Id))
            );
    }

    private async Task<Either<ActionResult, ReleaseVersion>> CreateBasicReleaseAmendment(
        ReleaseVersion originalReleaseVersion,
        DateTime createdDate
    )
    {
        var createdByUserId = userService.GetUserId();

        var amendmentReleaseVersionId = Guid.NewGuid();

        var dataBlockVersionAmendments = CopyDataBlockVersions(
            originalReleaseVersion,
            amendmentReleaseVersionId,
            createdDate
        );

        // Create maps of the original data blocks (keyed by their original DataBlockVersion Id, which is what
        // FeaturedTable.DataBlockVersionId, KeyStatisticDataBlock.DataBlockVersionId and
        // DataBlockVersionLink.DataBlockVersionId all reference) to their amended DataBlockVersion and amended
        // DataBlockVersionLink counterparts.
        var originalVersionIdToAmendedVersion = dataBlockVersionAmendments.ToDictionary(
            amendment => amendment.OriginalVersionId,
            amendment => amendment.AmendedVersion
        );

        // Only data blocks that were placed in a content section have an amended link, as a DataBlockVersionLink
        // exists only for the duration of a DataBlockVersion's placement in a ContentSection.
        var originalVersionIdToAmendedLink = dataBlockVersionAmendments
            .Where(amendment => amendment.AmendedLink is not null)
            .ToDictionary(amendment => amendment.OriginalVersionId, amendment => amendment.AmendedLink!);

        var amendmentReleaseVersion = new ReleaseVersion
        {
            // Assign this Release amendment a new Id.
            Id = amendmentReleaseVersionId,

            // Copy various fields directly from the original release version.
            Release = originalReleaseVersion.Release,
            Publication = originalReleaseVersion.Publication,
            Type = originalReleaseVersion.Type,
            ApprovalStatus = ReleaseApprovalStatus.Draft,
            PublishingOrganisations = originalReleaseVersion.PublishingOrganisations,
            DataGuidance = originalReleaseVersion.DataGuidance,
            PreReleaseAccessList = originalReleaseVersion.PreReleaseAccessList,
            NextReleaseDate = originalReleaseVersion.NextReleaseDate,

            // Assign new amendment-specific values to various fields.

            // TODO EES-4637 - we need to decide on how we're being consistent with Created dates in Release Amendments.
            Created = createdDate,
            CreatedById = createdByUserId,
            Version = originalReleaseVersion.Version + 1,
            PreviousVersionId = originalReleaseVersion.Id,

            DataBlockVersions = dataBlockVersionAmendments.Select(amendment => amendment.AmendedVersion).ToList(),
            KeyStatistics = CopyKeyStatistics(
                originalReleaseVersion,
                amendmentReleaseVersionId,
                createdByUserId,
                originalVersionIdToAmendedVersion
            ),
            Content = CopyContent(
                originalReleaseVersion,
                createdDate,
                amendmentReleaseVersionId,
                originalVersionIdToAmendedLink
            ),
            FeaturedTables = CopyFeaturedTables(
                originalReleaseVersion,
                amendmentReleaseVersionId,
                createdByUserId,
                originalVersionIdToAmendedVersion
            ),
            RelatedInformation = CopyRelatedInformation(originalReleaseVersion),
            Updates = CopyUpdates(originalReleaseVersion, amendmentReleaseVersionId, createdDate, createdByUserId),
        };

        context.ReleaseVersions.Add(amendmentReleaseVersion);

        // Every amended DataBlockVersionLink is placed in a content section, and so is persisted via that section's
        // Content collection (in CopyContentBlocks). Unattached data blocks have no link to add.

        await context.SaveChangesAsync();
        return amendmentReleaseVersion;
    }

    private List<KeyStatistic> CopyKeyStatistics(
        ReleaseVersion originalReleaseVersion,
        Guid amendmentReleaseVersionId,
        Guid createdByUserId,
        Dictionary<Guid, DataBlockVersion> originalVersionIdToAmendedVersion
    )
    {
        return originalReleaseVersion
            .KeyStatistics.Select<KeyStatistic, KeyStatistic>(originalKeyStat =>
            {
                if (originalKeyStat is KeyStatisticText originalKeyStatText)
                {
                    return new KeyStatisticText
                    {
                        // Assign a new Id.
                        Id = Guid.NewGuid(),

                        // Assign it to the amended release version.
                        ReleaseVersionId = amendmentReleaseVersionId,

                        // Copy certain fields from the original.
                        Order = originalKeyStatText.Order,
                        Statistic = originalKeyStatText.Statistic,
                        Trend = originalKeyStatText.Trend,
                        Title = originalKeyStatText.Title,
                        GuidanceText = originalKeyStatText.GuidanceText,
                        GuidanceTitle = originalKeyStatText.GuidanceTitle,

                        // Mark this as being created by the current user.
                        CreatedById = createdByUserId,
                    };
                }

                if (originalKeyStat is KeyStatisticDataBlock originalKeyStatDataBlock)
                {
                    return new KeyStatisticDataBlock
                    {
                        // Assign a new Id.
                        Id = Guid.NewGuid(),

                        // Assign it to the amended release version.
                        ReleaseVersionId = amendmentReleaseVersionId,

                        // Copy certain fields from the original.
                        Order = originalKeyStatDataBlock.Order,
                        Trend = originalKeyStatDataBlock.Trend,
                        GuidanceText = originalKeyStatDataBlock.GuidanceText,
                        GuidanceTitle = originalKeyStatDataBlock.GuidanceTitle,
                        DataBlockId = originalKeyStatDataBlock.DataBlockId,

                        // Link to the new version of the DataBlock from the original.
                        DataBlockVersionId = originalVersionIdToAmendedVersion[
                            originalKeyStatDataBlock.DataBlockVersionId
                        ].Id,

                        // Mark this as being created by the current user.
                        CreatedById = createdByUserId,
                    };
                }

                throw new ArgumentException(
                    $"Unknown {nameof(KeyStatistic)} subclass {originalKeyStat.GetType()} during amendment"
                );
            })
            .ToList();
    }

    private List<DataBlockVersionAmendment> CopyDataBlockVersions(
        ReleaseVersion originalReleaseVersion,
        Guid amendmentReleaseVersionId,
        DateTime createdDate
    )
    {
        // The positional/locking state of a data block now lives on its DataBlockVersionLink (a ContentBlock) rather
        // than on its DataBlockVersion, and there is no navigation from a version back to its link, so load the
        // original release's DataBlockVersionLinks up front. Only data blocks that are placed in a content section
        // have a link, so versions without one are unattached.
        var originalDataBlockVersionLinksByVersionId = context
            .DataBlockVersionLinks.Where(link => link.ReleaseVersionId == originalReleaseVersion.Id)
            .ToList()
            .ToDictionary(link => link.DataBlockVersionId);

        return originalReleaseVersion
            .DataBlockVersions.Select(originalDataBlockVersion =>
            {
                // Create a new entry in the DataBlock history in the form of a new DataBlockVersion, accompanied by a
                // new DataBlockVersionLink if the original data block was placed in a content section.
                var amendment = CopyDataBlockVersion(
                    originalDataBlockVersion,
                    originalDataBlockVersionLinksByVersionId.GetValueOrDefault(originalDataBlockVersion.Id),
                    amendmentReleaseVersionId,
                    createdDate
                );

                // Set the new DataBlockVersion to be the new Draft version.
                amendment.AmendedVersion.DataBlock.LatestDraftVersion = amendment.AmendedVersion;
                amendment.AmendedVersion.DataBlock.LatestDraftVersionId = amendment.AmendedVersion.Id;
                return amendment;
            })
            .ToList();
    }

    private static DataBlockVersionAmendment CopyDataBlockVersion(
        DataBlockVersion originalDataBlockVersion,
        DataBlockVersionLink? originalDataBlockVersionLink,
        Guid amendmentReleaseVersionId,
        DateTime createdDate
    )
    {
        // The amended DataBlockVersionLink (a ContentBlock) shares its Id with the amended DataBlockVersion it points
        // to. This shared-Id invariant is relied upon by FeaturedTable.DataBlockVersionId and
        // KeyStatisticDataBlock.DataBlockVersionId, which are matched against the DataBlockVersion's Id.
        var amendedId = Guid.NewGuid();

        var amendedDataBlockVersion = new DataBlockVersion
        {
            Id = amendedId,

            // Copy over fields that we want to carry over into the amended version.
            DataBlock = originalDataBlockVersion.DataBlock,
            DataBlockId = originalDataBlockVersion.DataBlockId,

            // Assign this new DataBlockVersion to the amended release version.
            ReleaseVersionId = amendmentReleaseVersionId,

            // Assign new field values to this new DataBlockVersion where we deliberately want separate
            // values from the original.
            Version = originalDataBlockVersion.Version + 1,

            // Copy over the content fields that we want to carry over into the amended version.
            Name = originalDataBlockVersion.Name,
            Charts = originalDataBlockVersion.Charts,
            Heading = originalDataBlockVersion.Heading,
            Query = originalDataBlockVersion.Query,
            Source = originalDataBlockVersion.Source,
            Table = originalDataBlockVersion.Table,

            // Explicitly list out fields that we're deliberately not carrying over, for clarity.
            // EES-4637 - we need to decide on how we're being consistent with Created dates in Release Amendments.
            // Created = createdDate,
            Updated = null,
            Published = null,
        };

        // An unattached data block has no link to copy. Its amended version will only gain one if it is placed in a
        // content section of the amendment.
        var amendedDataBlockVersionLink = originalDataBlockVersionLink is null
            ? null
            : new DataBlockVersionLink
            {
                Id = amendedId,
                DataBlockVersionId = amendedId,
                DataBlockVersion = amendedDataBlockVersion,

                // Assign this new DataBlockVersionLink to the amended release version.
                ReleaseVersionId = amendmentReleaseVersionId,

                // Copy over positional fields that we want to carry over into the amended version.
                Order = originalDataBlockVersionLink.Order,
                Created = createdDate,

                // Explicitly list out fields that we're deliberately not carrying over, for clarity.
                Updated = null,
                Comments = [],

                // The amended content section is assigned in CopyContentBlocks.
                Locked = null,
                LockedBy = null,
                LockedById = null,
            };

        return new DataBlockVersionAmendment(
            OriginalVersionId: originalDataBlockVersion.Id,
            AmendedVersion: amendedDataBlockVersion,
            AmendedLink: amendedDataBlockVersionLink
        );
    }

    /// <summary>
    /// Holds the amended <see cref="DataBlockVersion"/> produced when copying a data block during a release amendment,
    /// keyed by the original <see cref="DataBlockVersion"/>'s Id. <see cref="AmendedLink"/> is null if the original
    /// data block was unattached, as a <see cref="DataBlockVersionLink"/> exists only while a
    /// <see cref="DataBlockVersion"/> is placed in a <see cref="ContentSection"/>.
    /// </summary>
    private record DataBlockVersionAmendment(
        Guid OriginalVersionId,
        DataBlockVersion AmendedVersion,
        DataBlockVersionLink? AmendedLink
    );

    /// <summary>
    /// Copies ContentSections, using newly-cloned ContentBlocks in new ContentSections rather than the original ones.
    /// </summary>
    private static List<ContentSection> CopyContent(
        ReleaseVersion originalReleaseVersion,
        DateTime createdDate,
        Guid amendmentReleaseVersionId,
        Dictionary<Guid, DataBlockVersionLink> originalVersionIdToAmendedLink
    ) =>
        originalReleaseVersion
            .Content.Select(originalSection =>
            {
                var contentSectionAmendmentId = Guid.NewGuid();

                return new ContentSection
                {
                    // Assign a new Id.
                    Id = contentSectionAmendmentId,

                    Heading = originalSection.Heading,
                    Order = originalSection.Order,
                    Type = originalSection.Type,

                    // Assign this ContentSection to the amended release version.
                    ReleaseVersionId = amendmentReleaseVersionId,

                    // Copy the ContentBlocks themselves and assign them to this new ContentSection amendment.
                    Content = CopyContentBlocks(
                        originalSectionContent: originalSection.Content,
                        contentSectionAmendmentId: contentSectionAmendmentId,
                        amendmentReleaseVersionId: amendmentReleaseVersionId,
                        createdDate: createdDate,
                        originalVersionIdToAmendedLink: originalVersionIdToAmendedLink
                    ),
                };
            })
            .ToList();

    private static List<ContentBlock> CopyContentBlocks(
        List<ContentBlock> originalSectionContent,
        Guid contentSectionAmendmentId,
        Guid amendmentReleaseVersionId,
        DateTime createdDate,
        Dictionary<Guid, DataBlockVersionLink> originalVersionIdToAmendedLink
    )
    {
        return originalSectionContent
            .Select<ContentBlock, ContentBlock>(originalContentBlock =>
            {
                if (originalContentBlock is DataBlockVersionLink originalDataBlockVersionLink)
                {
                    // Place the already-created amended DataBlockVersionLink into this amended content section.
                    var amendedDataBlockVersionLink = originalVersionIdToAmendedLink[
                        originalDataBlockVersionLink.DataBlockVersionId
                    ];
                    amendedDataBlockVersionLink.ContentSectionId = contentSectionAmendmentId;
                    return amendedDataBlockVersionLink;
                }

                if (originalContentBlock is HtmlBlock originalHtmlBlock)
                {
                    return new HtmlBlock
                    {
                        // Assign a new Id.
                        Id = Guid.NewGuid(),

                        // Assign the HtmlBlock to the new Release amendment and the new ContentSection amendment.
                        ReleaseVersionId = amendmentReleaseVersionId,
                        ContentSectionId = contentSectionAmendmentId,

                        // Copy certain fields from the original HtmlBlock.
                        Body = FilterOutComments(originalHtmlBlock.Body),
                        Order = originalHtmlBlock.Order,
                    };
                }

                if (originalContentBlock is EmbedBlockLink originalEmbedBlockLink)
                {
                    return new EmbedBlockLink
                    {
                        // Assign a new Id.
                        Id = Guid.NewGuid(),

                        // Assign the EmbedBlockLink to the new Release amendment and the new ContentSection amendment.
                        ReleaseVersionId = amendmentReleaseVersionId,
                        ContentSectionId = contentSectionAmendmentId,

                        // Copy certain fields from the original EmbedBlockLink.
                        Order = originalEmbedBlockLink.Order,

                        // Create a new EmbedBlock for this new EmbedBlockLink, based upon the original.
                        EmbedBlock = new EmbedBlock
                        {
                            Id = Guid.NewGuid(),
                            Created = createdDate,
                            Title = originalEmbedBlockLink.EmbedBlock.Title,
                            Url = originalEmbedBlockLink.EmbedBlock.Url,
                        },
                    };
                }

                throw new ArgumentException(
                    $"Unknown {nameof(ContentBlockType)} value {originalContentBlock.GetType()} during amendment"
                );
            })
            .ToList();
    }

    private List<FeaturedTable> CopyFeaturedTables(
        ReleaseVersion originalReleaseVersion,
        Guid amendmentReleaseVersionId,
        Guid createdByUserId,
        Dictionary<Guid, DataBlockVersion> originalVersionIdToAmendedVersion
    )
    {
        return originalReleaseVersion
            .FeaturedTables.Select(originalFeaturedTable => new FeaturedTable
            {
                // Assign a new Id.
                Id = Guid.NewGuid(),

                // Assign it to the amended release version.
                ReleaseVersionId = amendmentReleaseVersionId,

                // Link it to the amended version of the original DataBlock, but to the same overarching DataBlock.
                DataBlockVersion = originalVersionIdToAmendedVersion[originalFeaturedTable.DataBlockVersionId],
                DataBlockId = originalFeaturedTable.DataBlockId,

                // Copy over certain fields from the original.
                Description = originalFeaturedTable.Description,
                Name = originalFeaturedTable.Name,
                Order = originalFeaturedTable.Order,

                CreatedById = createdByUserId,
            })
            .ToList();
    }

    private List<Link> CopyRelatedInformation(ReleaseVersion originalReleaseVersion)
    {
        return originalReleaseVersion
            .RelatedInformation.Select(originalRelatedInformation => new Link
            {
                // Assign a new Id.
                Id = Guid.NewGuid(),

                // Copy certain fields from the original.
                Description = originalRelatedInformation.Description,
                Url = originalRelatedInformation.Url,
            })
            .ToList();
    }

    private List<Update> CopyUpdates(
        ReleaseVersion originalReleaseVersion,
        Guid amendmentReleaseVersionId,
        DateTime createdDate,
        Guid createdByUserId
    )
    {
        return originalReleaseVersion
            .Updates.Select(originalUpdate => new Update
            {
                // Assign a new Id.
                Id = Guid.NewGuid(),

                // Assign it to the amended release version.
                ReleaseVersionId = amendmentReleaseVersionId,

                // Copy certain fields from the original.
                On = originalUpdate.On,
                Reason = originalUpdate.Reason,

                // Assign the new created date.
                Created = createdDate,
                CreatedById = createdByUserId,
            })
            .ToList();
    }

    private async Task<Either<ActionResult, Unit>> CreateStatisticsReleaseAmendment(
        ReleaseVersion amendmentReleaseVersion
    )
    {
        var statsReleaseVersion = await statisticsDbContext.ReleaseVersion.FirstOrDefaultAsync(rv =>
            rv.Id == amendmentReleaseVersion.PreviousVersionId
        );

        // Release does not have to have stats uploaded but if it has then
        // create a link row to link back to the original subject
        if (statsReleaseVersion != null)
        {
            var statsAmendmentVersion = new Data.Model.ReleaseVersion
            {
                Id = amendmentReleaseVersion.Id,
                PublicationId = amendmentReleaseVersion.PublicationId,
            };

            var statsAmendmentSubjectLinks = statisticsDbContext
                .ReleaseSubject.Where(rs => rs.ReleaseVersionId == amendmentReleaseVersion.PreviousVersionId)
                .Select(originalReleaseSubject => new ReleaseSubject
                {
                    // Assign it to the new release version.
                    ReleaseVersionId = amendmentReleaseVersion.Id,

                    // Copy certain fields from the original.
                    SubjectId = originalReleaseSubject.SubjectId,
                });
            statisticsDbContext.ReleaseVersion.Add(statsAmendmentVersion);
            statisticsDbContext.ReleaseSubject.AddRange(statsAmendmentSubjectLinks);

            await statisticsDbContext.SaveChangesAsync();
        }

        return Unit.Instance;
    }

    private async Task<Either<ActionResult, List<Footnote>>> CopyFootnotes(
        Guid originalReleaseVersionId,
        Guid amendmentReleaseVersionId
    )
    {
        var originalFootnotes = await footnoteRepository.GetFootnotes(originalReleaseVersionId);

        return await originalFootnotes
            .ToAsyncEnumerable()
            .SelectAwait(async originalFootnote =>
            {
                var filterIds = originalFootnote.Filters.Select(filterFootnote => filterFootnote.FilterId).ToHashSet();
                var filterGroupIds = originalFootnote
                    .FilterGroups.Select(filterGroupFootnote => filterGroupFootnote.FilterGroupId)
                    .ToHashSet();
                var filterItemIds = originalFootnote
                    .FilterItems.Select(filterItemFootnote => filterItemFootnote.FilterItemId)
                    .ToHashSet();
                var indicatorIds = originalFootnote
                    .Indicators.Select(indicatorFootnote => indicatorFootnote.IndicatorId)
                    .ToHashSet();
                var subjectIds = originalFootnote
                    .Subjects.Select(subjectFootnote => subjectFootnote.SubjectId)
                    .ToHashSet();

                return await footnoteRepository.CreateFootnote(
                    amendmentReleaseVersionId,
                    originalFootnote.Content,
                    filterIds: filterIds,
                    filterGroupIds: filterGroupIds,
                    filterItemIds: filterItemIds,
                    indicatorIds: indicatorIds,
                    subjectIds: subjectIds,
                    originalFootnote.Order
                );
            })
            .ToListAsync();
    }

    private async Task<Either<ActionResult, ReleaseVersion>> CopyFileLinks(
        ReleaseVersion originalReleaseVersion,
        ReleaseVersion amendmentReleaseVersion
    )
    {
        var releaseFileCopies = context
            .ReleaseFiles.Include(f => f.File)
            .Where(f => f.ReleaseVersionId == originalReleaseVersion.Id)
            .Select(originalFile => new ReleaseFile
            {
                // Assign a new Id.
                Id = Guid.NewGuid(),

                // Assign it to the amended release version.
                ReleaseVersionId = amendmentReleaseVersion.Id,

                // Copy certain fields from the original.
                FileId = originalFile.FileId,
                Order = originalFile.Order,
                Name = originalFile.Name,
                Summary = originalFile.Summary,
                FilterSequence = originalFile.FilterSequence,
                IndicatorSequence = originalFile.IndicatorSequence,
                Published = originalFile.Published,
                PublicApiDataSetId = originalFile.PublicApiDataSetId,
                PublicApiDataSetVersion = originalFile.PublicApiDataSetVersion,
            })
            .ToList();

        await context.ReleaseFiles.AddRangeAsync(releaseFileCopies);
        await context.SaveChangesAsync();
        return amendmentReleaseVersion;
    }

    private static string? FilterOutComments(string? bodyText) =>
        string.IsNullOrEmpty(bodyText) ? bodyText : CommentsRegex().Replace(bodyText, _ => string.Empty);
}

internal static class ReleaseAmendmentQueryableExtensions
{
    internal static IQueryable<ReleaseVersion> HydrateReleaseVersionForAmendment(
        this IQueryable<ReleaseVersion> queryable
    )
    {
        return queryable
            .AsSplitQuery()
            .Include(releaseVersion => releaseVersion.Publication)
            .Include(releaseVersion => releaseVersion.Release)
            .Include(releaseVersion => releaseVersion.PublishingOrganisations)
            .Include(releaseVersion => releaseVersion.Content)
                .ThenInclude(section => section.Content)
                    .ThenInclude(block => (block as EmbedBlockLink)!.EmbedBlock)
            .Include(releaseVersion => releaseVersion.Updates)
            .Include(releaseVersion => releaseVersion.Content)
                .ThenInclude(contentSection => contentSection.Content)
            .Include(releaseVersion => releaseVersion.KeyStatistics)
                .ThenInclude(keyStat => (keyStat as KeyStatisticDataBlock)!.DataBlockVersion)
            .Include(releaseVersion => releaseVersion.FeaturedTables)
            .Include(releaseVersion => releaseVersion.DataBlockVersions)
            .Include(releaseVersion => releaseVersion.DataBlockVersions)
                .ThenInclude(dataBlockVersion => dataBlockVersion.DataBlock)
                    .ThenInclude(dataBlock => dataBlock.LatestDraftVersion)
            .Include(releaseVersion => releaseVersion.DataBlockVersions)
                .ThenInclude(dataBlockVersion => dataBlockVersion.DataBlock)
                    .ThenInclude(dataBlock => dataBlock.LatestPublishedVersion);
    }
}
