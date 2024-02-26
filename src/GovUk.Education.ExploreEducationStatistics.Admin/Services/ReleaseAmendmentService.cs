#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class ReleaseAmendmentService : IReleaseAmendmentService
{
    private static readonly Regex CommentsRegex =
        new(ContentFilterUtils.CommentsFilterPattern, RegexOptions.Compiled);

    private readonly ContentDbContext _context;
    private readonly IFootnoteRepository _footnoteRepository;
    private readonly StatisticsDbContext _statisticsDbContext;
    private readonly IUserService _userService;
    private readonly IPublicationReleaseSeriesViewService _publicationReleaseSeriesViewService;

    public ReleaseAmendmentService(
        ContentDbContext context,
        IUserService userService,
        IFootnoteRepository footnoteRepository,
        StatisticsDbContext statisticsDbContext,
        IPublicationReleaseSeriesViewService publicationReleaseSeriesViewService)
    {
        _context = context;
        _userService = userService;
        _footnoteRepository = footnoteRepository;
        _statisticsDbContext = statisticsDbContext;
        _publicationReleaseSeriesViewService = publicationReleaseSeriesViewService;
    }

    public async Task<Either<ActionResult, IdViewModel>> CreateReleaseAmendment(Guid releaseId)
    {
        var createdDate = DateTime.UtcNow;

        return await _context
            .Releases
            .HydrateReleaseForAmendment()
            .SingleOrDefault(release => release.Id == releaseId)
            .OrNotFound()
            .OnSuccess(_userService.CheckCanMakeAmendmentOfRelease)
            .OnSuccess(originalRelease =>
                CreateBasicReleaseAmendment(originalRelease, createdDate)
                    .OnSuccessDo(CreateStatisticsReleaseAmendment)
                    .OnSuccessDo(amendment => CopyReleaseRoles(releaseId, amendment.Id, createdDate))
                    .OnSuccessDo(amendment => CopyFootnotes(releaseId, amendment.Id))
                    .OnSuccess(amendment => CopyFileLinks(originalRelease, amendment))
                    .OnSuccess(amendment => new IdViewModel(amendment.Id)));
    }

    private async Task<Either<ActionResult, Release>> CreateBasicReleaseAmendment(Release originalRelease, DateTime createdDate)
    {
        var createdByUserId = _userService.GetUserId();

        var releaseAmendmentId = Guid.NewGuid();

        var dataBlockVersionAmendments = CopyDataBlockVersions(originalRelease, releaseAmendmentId, createdDate);

        // Create a map of the original DataBlocks to their amended counterparts.
        var originalDataBlockVersionsToAmendments = dataBlockVersionAmendments
            .Select(dataBlockVersionAmendment => dataBlockVersionAmendment.DataBlockParent)
            .ToDictionary(
                dataBlockParent => dataBlockParent.LatestPublishedVersion!,
                dataBlockParent => dataBlockParent.LatestDraftVersion!);

        var amendment = new Release
        {
            // Assign this Release amendment a new Id.
            Id = releaseAmendmentId,

            // Copy various fields directly from the originalRelease.
            ReleaseParent = originalRelease.ReleaseParent,
            Publication = originalRelease.Publication,
            Slug = originalRelease.Slug,
            Type = originalRelease.Type,
            ApprovalStatus = ReleaseApprovalStatus.Draft,
            DataGuidance = originalRelease.DataGuidance,
            ReleaseName = originalRelease.ReleaseName,
            TimePeriodCoverage = originalRelease.TimePeriodCoverage,
            PreReleaseAccessList = originalRelease.PreReleaseAccessList,
            NextReleaseDate = originalRelease.NextReleaseDate,

            // Assign new amendment-specific values to various fields.

            // TODO EES-4637 - we need to decide on how we're being consistent with Created dates in Release Amendments.
            Created = createdDate,
            CreatedById = createdByUserId,
            Version = originalRelease.Version + 1,
            PreviousVersionId = originalRelease.Id,

            DataBlockVersions = dataBlockVersionAmendments,
            KeyStatistics = CopyKeyStatistics(originalRelease, releaseAmendmentId, createdByUserId, originalDataBlockVersionsToAmendments),
            Content = CopyContent(originalRelease, createdDate, releaseAmendmentId, originalDataBlockVersionsToAmendments),
            FeaturedTables = CopyFeaturedTables(originalRelease, releaseAmendmentId, createdByUserId, originalDataBlockVersionsToAmendments),
            RelatedInformation = CopyRelatedInformation(originalRelease),
            Updates = CopyUpdates(originalRelease, releaseAmendmentId, createdDate, createdByUserId)
        };

        await _context.Releases.AddAsync(amendment);

        await _publicationReleaseSeriesViewService.CreateForAmendRelease(
            originalRelease.PublicationId,
            releaseAmendmentId);

        // What to do about ReleaseStatuses?

        await _context.SaveChangesAsync();
        return amendment;
    }

    private List<KeyStatistic> CopyKeyStatistics(
        Release originalRelease,
        Guid releaseAmendmentId,
        Guid createdByUserId,
        Dictionary<DataBlockVersion, DataBlockVersion> originalDataBlockVersionsToAmendments)
    {
        var originalDataBlockIdsToAmendments = originalDataBlockVersionsToAmendments
            .ToDictionary(
                kvp => kvp.Key.ContentBlockId,
                kvp => kvp.Value.ContentBlock);

        return originalRelease
            .KeyStatistics
            .Select<KeyStatistic, KeyStatistic>(originalKeyStat =>
            {
                if (originalKeyStat is KeyStatisticText originalKeyStatText)
                {
                    return new KeyStatisticText
                    {
                        // Assign a new Id.
                        Id = Guid.NewGuid(),

                        // Assign it to the new Release amendment.
                        ReleaseId = releaseAmendmentId,

                        // Copy certain fields from the original.
                        Order = originalKeyStatText.Order,
                        Statistic = originalKeyStatText.Statistic,
                        Trend = originalKeyStatText.Trend,
                        Title = originalKeyStatText.Title,
                        GuidanceText = originalKeyStatText.GuidanceText,
                        GuidanceTitle = originalKeyStatText.GuidanceTitle,

                        // Mark this as being created by the current user.
                        CreatedById = createdByUserId
                    };
                }

                if (originalKeyStat is KeyStatisticDataBlock originalKeyStatDataBlock)
                {
                    return new KeyStatisticDataBlock
                    {
                        // Assign a new Id.
                        Id = Guid.NewGuid(),

                        // Assign it to the new Release amendment.
                        ReleaseId = releaseAmendmentId,

                        // Copy certain fields from the original.
                        Order = originalKeyStatDataBlock.Order,
                        Trend = originalKeyStatDataBlock.Trend,
                        GuidanceText = originalKeyStatDataBlock.GuidanceText,
                        GuidanceTitle = originalKeyStatDataBlock.GuidanceTitle,
                        DataBlockParentId = originalKeyStatDataBlock.DataBlockParentId,

                        // Link to the new version of the DataBlock from the original.
                        DataBlockId = originalDataBlockIdsToAmendments[originalKeyStatDataBlock.DataBlockId].Id,

                        // Mark this as being created by the current user.
                        CreatedById = createdByUserId
                    };
                }

                throw new ArgumentException(
                    $"Unknown {nameof(KeyStatistic)} subclass {originalKeyStat.GetType()} during amendment");
            })
            .ToList();
    }

    private List<DataBlockVersion> CopyDataBlockVersions(
        Release originalRelease,
        Guid amendmentId,
        DateTime createdDate)
    {
        return originalRelease
            .DataBlockVersions
            .Select(originalDataBlockVersion =>
            {
                // Create a new entry in the DataBlock history in the form of a new DataBlockVersion.
                var copiedDataBlockVersion = CopyDataBlockVersion(originalDataBlockVersion, amendmentId, createdDate);

                // Set the new DataBlockVersion to be the new Draft version.
                copiedDataBlockVersion.DataBlockParent.LatestDraftVersion = copiedDataBlockVersion;
                copiedDataBlockVersion.DataBlockParent.LatestDraftVersionId = copiedDataBlockVersion.Id;
                return copiedDataBlockVersion;
            })
            .ToList();
    }

    private DataBlockVersion CopyDataBlockVersion(
        DataBlockVersion originalDataBlockVersion,
        Guid amendmentId,
        DateTime createdDate)
    {
        var originalContentBlock = originalDataBlockVersion.ContentBlock;

        // Not copying Comments.
        var copiedContentBlock = new DataBlock
        {
            // Assign a new Id.
            Id = Guid.NewGuid(),

            // Assign this new DataBlockVersion to the new Release amendment.
            ReleaseId = amendmentId,

            // Copy over fields that we want to carry over into the amended version.
            Name = originalContentBlock.Name,
            Charts = originalContentBlock.Charts,
            Heading = originalContentBlock.Heading,
            Order = originalContentBlock.Order,
            Query = originalContentBlock.Query,
            Source = originalContentBlock.Source,
            Table = originalContentBlock.Table,
            // EES-4637 - we need to decide on how we're being consistent with Created
            // dates in Release Amendments.
            Created = createdDate,
            Updated = null,

            // Explicitly list out fields that we're deliberately not carrying over
            // into the amended ContentBlock, for clarity.
            Comments = new List<Comment>(),
            ContentSection = null,
            ContentSectionId = null,
            Locked = null,
            LockedBy = null,
            LockedById = null,
        };

        return new DataBlockVersion
        {
            // Keep a one-to-one relationship between DataBlockVersions and their backing ContentBlocks.
            // This will make it easier to migrate DataBlocks out of the ContentBlock model in the future.
            Id = copiedContentBlock.Id,

            // Copy over fields that we want to carry over into the amended version.
            DataBlockParent = originalDataBlockVersion.DataBlockParent,
            DataBlockParentId = originalDataBlockVersion.DataBlockParentId,

            // Assign this new DataBlockVersion to the new Release amendment.
            ReleaseId = amendmentId,

            // Assign new field values to this new DataBlockVersion where we deliberately want separate
            // values from the original.
            // Created = createdDate,
            Version = originalDataBlockVersion.Version + 1,

            // Link the newly created ContentBlock to this DataBlockVersion.
            ContentBlock = copiedContentBlock,

            // Explicitly list out fields that we're deliberately not carrying over
            // into the amended ContentBlock, for clarity.
            Updated = null,
            Published = null,
        };
    }

    private List<ContentSection> CopyContent(
        Release originalRelease,
        DateTime createdDate,
        Guid releaseAmendmentId,
        Dictionary<DataBlockVersion, DataBlockVersion> originalDataBlockVersionsToAmendments)
    {
        // Copy ContentSections, using the newly-cloned ContentBlocks and DataBlocks in the new ContentSections
        // rather than the original ones.
        var amendedContent = originalRelease
            .Content
            .Select(originalSection =>
            {
                var contentSectionAmendmentId = Guid.NewGuid();

                return new ContentSection
                {
                    // Assign a new Id.
                    Id = contentSectionAmendmentId,

                    Caption = originalSection.Caption,
                    Heading = originalSection.Heading,
                    Order = originalSection.Order,
                    Type = originalSection.Type,

                    // Assign this ContentSection to the new amendment.
                    ReleaseId = releaseAmendmentId,

                    // Copy the ContentBlocks themselves and assign them to this new ContentSection amendment.
                    Content = CopyContentBlocks(
                        originalSectionContent: originalSection.Content,
                        contentSectionAmendmentId: contentSectionAmendmentId,
                        releaseAmendmentId: releaseAmendmentId,
                        createdDate: createdDate,
                        originalDataBlockVersionsToAmendments: originalDataBlockVersionsToAmendments)
                };
            })
            .ToList();

        // If the original Release did not contain a RelatedDashboards section, add an empty one to its amendment.
        if (originalRelease.RelatedDashboardsSection == null)
        {
            amendedContent.Add(new ContentSection
            {
                Id = Guid.NewGuid(),
                Type = ContentSectionType.RelatedDashboards,
                ReleaseId = releaseAmendmentId
            });
        }

        return amendedContent;
    }

    private List<ContentBlock> CopyContentBlocks(
        List<ContentBlock> originalSectionContent,
        Guid contentSectionAmendmentId,
        Guid releaseAmendmentId,
        DateTime createdDate,
        Dictionary<DataBlockVersion, DataBlockVersion> originalDataBlockVersionsToAmendments)
    {
        var originalDataBlockIdsToAmendments = originalDataBlockVersionsToAmendments
            .ToDictionary(
                kvp => kvp.Key.ContentBlockId,
                kvp => kvp.Value.ContentBlock);

        return originalSectionContent
            .Select<ContentBlock, ContentBlock>(originalContentBlock =>
            {
                if (originalContentBlock is DataBlock)
                {
                    return originalDataBlockIdsToAmendments[originalContentBlock.Id];
                }

                if (originalContentBlock is HtmlBlock originalHtmlBlock)
                {
                    return new HtmlBlock
                    {
                        // Assign a new Id.
                        Id = Guid.NewGuid(),

                        // Assign the HtmlBlock to the new Release amendment and the new ContentSection amendment.
                        ReleaseId = releaseAmendmentId,
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
                        ReleaseId = releaseAmendmentId,
                        ContentSectionId = contentSectionAmendmentId,

                        // Copy certain fields from the original EmbedBlockLink.
                        Order = originalEmbedBlockLink.Order,

                        // Create a new EmbedBlock for this new EmbedBlockLink, based upon the original.
                        EmbedBlock = new EmbedBlock
                        {
                            Id = Guid.NewGuid(),
                            Created = createdDate,
                            Title = originalEmbedBlockLink.EmbedBlock.Title,
                            Url = originalEmbedBlockLink.EmbedBlock.Url
                        },
                    };
                }

                if (originalContentBlock is MarkDownBlock originalMarkDownBlock)
                {
                    return new MarkDownBlock
                    {
                        // Assign a new Id.
                        Id = Guid.NewGuid(),

                        // Assign the MarkDownBlock to the new Release amendment and the new ContentSection amendment.
                        ReleaseId = releaseAmendmentId,
                        ContentSectionId = contentSectionAmendmentId,

                        // Copy certain fields from the original MarkDownBlock.
                        Body = originalMarkDownBlock.Body,
                        Order = originalMarkDownBlock.Order,
                    };
                }

                throw new ArgumentException(
                    $"Unknown {nameof(ContentBlockType)} value {originalContentBlock.GetType()} during amendment");
            })
            .ToList();
    }

    private List<FeaturedTable> CopyFeaturedTables(
        Release originalRelease,
        Guid releaseAmendmentId,
        Guid createdByUserId,
        Dictionary<DataBlockVersion, DataBlockVersion> originalDataBlockVersionsToAmendments)
    {
        var originalDataBlockIdsToAmendments = originalDataBlockVersionsToAmendments
            .ToDictionary(
                kvp => kvp.Key.ContentBlockId,
                kvp => kvp.Value.ContentBlock);

        return originalRelease
            .FeaturedTables
            .Select(originalFeaturedTable => new FeaturedTable
            {
                // Assign a new Id.
                Id = Guid.NewGuid(),

                // Assign it to the new Release amendment.
                ReleaseId = releaseAmendmentId,

                // Link it to the amended version of the original DataBlock, but to the same overarching
                // DataBlockParent.
                DataBlock = originalDataBlockIdsToAmendments[originalFeaturedTable.DataBlockId],
                DataBlockParentId = originalFeaturedTable.DataBlockParentId,

                // Copy over certain fields from the original.
                Description = originalFeaturedTable.Description,
                Name = originalFeaturedTable.Name,
                Order = originalFeaturedTable.Order,

                CreatedById = createdByUserId
            })
            .ToList();
    }

    private List<Link> CopyRelatedInformation(Release originalRelease)
    {
        return originalRelease
            .RelatedInformation
            .Select(originalRelatedInformation => new Link
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
        Release originalRelease,
        Guid releaseAmendmentId,
        DateTime createdDate,
        Guid createdByUserId)
    {
        return originalRelease
            .Updates
            .Select(originalUpdate => new Update
            {
                // Assign a new Id.
                Id = Guid.NewGuid(),

                // Assign it to the new Release amendment.
                ReleaseId = releaseAmendmentId,

                // Copy certain fields from the original.
                On = originalUpdate.On,
                Reason = originalUpdate.Reason,

                // Assign the new created date.
                Created = createdDate,
                CreatedById = createdByUserId
            })
            .ToList();
    }

    private async Task<Either<ActionResult, Unit>> CreateStatisticsReleaseAmendment(Release amendment)
    {
        var statsRelease = await _statisticsDbContext
            .Release
            .FirstOrDefaultAsync(r => r.Id == amendment.PreviousVersionId);

        // Release does not have to have stats uploaded but if it has then
        // create a link row to link back to the original subject
        if (statsRelease != null)
        {
            var statsAmendment = new Data.Model.Release
            {
                Id = amendment.Id,
                PublicationId = amendment.PublicationId
            };

            var statsAmendmentSubjectLinks = _statisticsDbContext
                .ReleaseSubject
                .Where(rs => rs.ReleaseId == amendment.PreviousVersionId)
                .Select(originalReleaseSubject => new ReleaseSubject
                {
                    // Assign it to the new Release amendment.
                    ReleaseId = amendment.Id,

                    // Copy certain fields from the original.
                    SubjectId = originalReleaseSubject.SubjectId,
                    FilterSequence = originalReleaseSubject.FilterSequence,
                    IndicatorSequence = originalReleaseSubject.IndicatorSequence
                });

            await _statisticsDbContext.Release.AddAsync(statsAmendment);
            await _statisticsDbContext.ReleaseSubject.AddRangeAsync(statsAmendmentSubjectLinks);

            await _statisticsDbContext.SaveChangesAsync();
        }

        return Unit.Instance;
    }

    private async Task<Either<ActionResult, Unit>> CopyReleaseRoles(
        Guid originalReleaseId,
        Guid releaseAmendmentId,
        DateTime createdDate)
    {
        // Copy all current roles apart from Prerelease Users to the Release amendment.
        var newRoles = _context
            .UserReleaseRoles
            // For auditing purposes, we also want to migrate release roles that have Deleted set (when a role is
            // manually removed from a Release as opposed to SoftDeleted, which is only set when a Release itself
            // is deleted)
            .IgnoreQueryFilters()
            .Where(releaseRole => releaseRole.ReleaseId == originalReleaseId
                                  && releaseRole.Role != ReleaseRole.PrereleaseViewer)
            .Select(originalReleaseRole => new UserReleaseRole
            {
                // Assign a new Id.
                Id = Guid.NewGuid(),

                // Assign it to the new Release amendment.
                ReleaseId = releaseAmendmentId,

                // Copy certain fields from the original.
                Role = originalReleaseRole.Role,
                UserId = originalReleaseRole.UserId,
                Deleted = originalReleaseRole.Deleted,
                DeletedById = originalReleaseRole.DeletedById,

                // Assign the new created date.
                Created = createdDate,
            })
            .ToList();

        await _context.AddRangeAsync(newRoles);
        await _context.SaveChangesAsync();
        return Unit.Instance;
    }

    private async Task<Either<ActionResult, List<Footnote>>> CopyFootnotes(
        Guid originalReleaseId,
        Guid releaseAmendmentId)
    {
        var originalFootnotes = await _footnoteRepository.GetFootnotes(originalReleaseId);

        return await originalFootnotes
            .ToAsyncEnumerable()
            .SelectAwait(async originalFootnote =>
            {
                var filterIds = originalFootnote.Filters
                    .Select(filterFootnote => filterFootnote.FilterId).ToHashSet();
                var filterGroupIds = originalFootnote.FilterGroups
                    .Select(filterGroupFootnote => filterGroupFootnote.FilterGroupId).ToHashSet();
                var filterItemIds = originalFootnote.FilterItems
                    .Select(filterItemFootnote => filterItemFootnote.FilterItemId).ToHashSet();
                var indicatorIds = originalFootnote.Indicators
                    .Select(indicatorFootnote => indicatorFootnote.IndicatorId).ToHashSet();
                var subjectIds = originalFootnote.Subjects
                    .Select(subjectFootnote => subjectFootnote.SubjectId).ToHashSet();

                return await _footnoteRepository.CreateFootnote(releaseAmendmentId,
                    originalFootnote.Content,
                    filterIds: filterIds,
                    filterGroupIds: filterGroupIds,
                    filterItemIds: filterItemIds,
                    indicatorIds: indicatorIds,
                    subjectIds: subjectIds,
                    originalFootnote.Order);
            })
            .ToListAsync();
    }

    private async Task<Either<ActionResult, Release>> CopyFileLinks(Release originalRelease, Release releaseAmendment)
    {
        var releaseFileCopies = _context
            .ReleaseFiles
            .Include(f => f.File)
            .Where(f => f.ReleaseId == originalRelease.Id)
            .Select(originalFile => new ReleaseFile
            {
                // Assign a new Id.
                Id = Guid.NewGuid(),

                // Assign it to the new Release amendment.
                ReleaseId = releaseAmendment.Id,

                // Copy certain fields from the original.
                FileId = originalFile.FileId,
                Order = originalFile.Order,
                Name = originalFile.Name,
                Summary = originalFile.Summary
            })
            .ToList();

        await _context.ReleaseFiles.AddRangeAsync(releaseFileCopies);
        await _context.SaveChangesAsync();
        return releaseAmendment;
    }

    private static string FilterOutComments(string bodyText)
    {
        if (bodyText.IsNullOrEmpty())
        {
            return bodyText;
        }

        return CommentsRegex.Replace(bodyText, _ => string.Empty);
    }
}

internal static class ReleaseAmendmentQueryableExtensions
{
    internal static IQueryable<Release> HydrateReleaseForAmendment(this IQueryable<Release> queryable)
    {
        return queryable
            .AsSplitQuery()
            .Include(release => release.Publication)
            .Include(release => release.ReleaseParent)
            .Include(release => release.Content)
            .ThenInclude(section => section.Content)
            .ThenInclude(block => (block as EmbedBlockLink)!.EmbedBlock)
            .Include(release => release.Updates)
            .Include(release => release.Content)
            .ThenInclude(release => release.Content)
            .Include(release => release.KeyStatistics)
            .ThenInclude(keyStat => (keyStat as KeyStatisticDataBlock)!.DataBlock)
            .Include(release => release.FeaturedTables)
            .Include(release => release.DataBlockVersions)
            .Include(release => release.DataBlockVersions)
            .ThenInclude(dataBlockVersion => dataBlockVersion.DataBlockParent)
            .ThenInclude(dataBlockParent => dataBlockParent.LatestDraftVersion)
            .Include(release => release.DataBlockVersions)
            .ThenInclude(dataBlockVersion => dataBlockVersion.DataBlockParent)
            .ThenInclude(dataBlockParent => dataBlockParent.LatestPublishedVersion)
            .ThenInclude(dataBlockVersion => dataBlockVersion != null ? dataBlockVersion.ContentBlock : null);
    }
}
