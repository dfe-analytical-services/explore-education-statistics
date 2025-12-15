#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;
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
    StatisticsDbContext statisticsDbContext,
    IUserReleaseRoleRepository userReleaseRoleRepository
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
                    .OnSuccessDo(amendment => CopyReleaseRoles(releaseVersionId, amendment.Id, createdDate))
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

        // Create a map of the original DataBlocks to their amended counterparts.
        var originalDataBlockVersionsToAmendments = dataBlockVersionAmendments
            .Select(dataBlockVersionAmendment => dataBlockVersionAmendment.DataBlockParent)
            .ToDictionary(
                dataBlockParent => dataBlockParent.LatestPublishedVersion!,
                dataBlockParent => dataBlockParent.LatestDraftVersion!
            );

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

            DataBlockVersions = dataBlockVersionAmendments,
            KeyStatistics = CopyKeyStatistics(
                originalReleaseVersion,
                amendmentReleaseVersionId,
                createdByUserId,
                originalDataBlockVersionsToAmendments
            ),
            Content = CopyContent(
                originalReleaseVersion,
                createdDate,
                amendmentReleaseVersionId,
                originalDataBlockVersionsToAmendments
            ),
            FeaturedTables = CopyFeaturedTables(
                originalReleaseVersion,
                amendmentReleaseVersionId,
                createdByUserId,
                originalDataBlockVersionsToAmendments
            ),
            RelatedInformation = CopyRelatedInformation(originalReleaseVersion),
            Updates = CopyUpdates(originalReleaseVersion, amendmentReleaseVersionId, createdDate, createdByUserId),
        };

        context.ReleaseVersions.Add(amendmentReleaseVersion);
        await context.SaveChangesAsync();
        return amendmentReleaseVersion;
    }

    private List<KeyStatistic> CopyKeyStatistics(
        ReleaseVersion originalReleaseVersion,
        Guid amendmentReleaseVersionId,
        Guid createdByUserId,
        Dictionary<DataBlockVersion, DataBlockVersion> originalDataBlockVersionsToAmendments
    )
    {
        var originalDataBlockIdsToAmendments = originalDataBlockVersionsToAmendments.ToDictionary(
            kvp => kvp.Key.ContentBlockId,
            kvp => kvp.Value.ContentBlock
        );

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
                        DataBlockParentId = originalKeyStatDataBlock.DataBlockParentId,

                        // Link to the new version of the DataBlock from the original.
                        DataBlockId = originalDataBlockIdsToAmendments[originalKeyStatDataBlock.DataBlockId].Id,

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

    private List<DataBlockVersion> CopyDataBlockVersions(
        ReleaseVersion originalReleaseVersion,
        Guid amendmentReleaseVersionId,
        DateTime createdDate
    )
    {
        return originalReleaseVersion
            .DataBlockVersions.Select(originalDataBlockVersion =>
            {
                // Create a new entry in the DataBlock history in the form of a new DataBlockVersion.
                var copiedDataBlockVersion = CopyDataBlockVersion(
                    originalDataBlockVersion,
                    amendmentReleaseVersionId,
                    createdDate
                );

                // Set the new DataBlockVersion to be the new Draft version.
                copiedDataBlockVersion.DataBlockParent.LatestDraftVersion = copiedDataBlockVersion;
                copiedDataBlockVersion.DataBlockParent.LatestDraftVersionId = copiedDataBlockVersion.Id;
                return copiedDataBlockVersion;
            })
            .ToList();
    }

    private DataBlockVersion CopyDataBlockVersion(
        DataBlockVersion originalDataBlockVersion,
        Guid amendmentReleaseVersionId,
        DateTime createdDate
    )
    {
        var originalContentBlock = originalDataBlockVersion.ContentBlock;

        // Not copying Comments.
        var copiedContentBlock = new DataBlock
        {
            // Assign a new Id.
            Id = Guid.NewGuid(),

            // Assign this new DataBlockVersion to the amended release version.
            ReleaseVersionId = amendmentReleaseVersionId,

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

            // Assign this new DataBlockVersion to the amended release version.
            ReleaseVersionId = amendmentReleaseVersionId,

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
        ReleaseVersion originalReleaseVersion,
        DateTime createdDate,
        Guid amendmentReleaseVersionId,
        Dictionary<DataBlockVersion, DataBlockVersion> originalDataBlockVersionsToAmendments
    )
    {
        // Copy ContentSections, using the newly-cloned ContentBlocks and DataBlocks in the new ContentSections
        // rather than the original ones.
        var amendedContent = originalReleaseVersion
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
                        originalDataBlockVersionsToAmendments: originalDataBlockVersionsToAmendments
                    ),
                };
            })
            .ToList();

        // If the original Release did not contain a RelatedDashboards section, add an empty one to its amendment.
        if (originalReleaseVersion.RelatedDashboardsSection == null)
        {
            amendedContent.Add(
                new ContentSection
                {
                    Id = Guid.NewGuid(),
                    Type = ContentSectionType.RelatedDashboards,
                    ReleaseVersionId = amendmentReleaseVersionId,
                }
            );
        }

        return amendedContent;
    }

    private List<ContentBlock> CopyContentBlocks(
        List<ContentBlock> originalSectionContent,
        Guid contentSectionAmendmentId,
        Guid amendmentReleaseVersionId,
        DateTime createdDate,
        Dictionary<DataBlockVersion, DataBlockVersion> originalDataBlockVersionsToAmendments
    )
    {
        var originalDataBlockIdsToAmendments = originalDataBlockVersionsToAmendments.ToDictionary(
            kvp => kvp.Key.ContentBlockId,
            kvp => kvp.Value.ContentBlock
        );

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
        Dictionary<DataBlockVersion, DataBlockVersion> originalDataBlockVersionsToAmendments
    )
    {
        var originalDataBlockIdsToAmendments = originalDataBlockVersionsToAmendments.ToDictionary(
            kvp => kvp.Key.ContentBlockId,
            kvp => kvp.Value.ContentBlock
        );

        return originalReleaseVersion
            .FeaturedTables.Select(originalFeaturedTable => new FeaturedTable
            {
                // Assign a new Id.
                Id = Guid.NewGuid(),

                // Assign it to the amended release version.
                ReleaseVersionId = amendmentReleaseVersionId,

                // Link it to the amended version of the original DataBlock, but to the same overarching
                // DataBlockParent.
                DataBlock = originalDataBlockIdsToAmendments[originalFeaturedTable.DataBlockId],
                DataBlockParentId = originalFeaturedTable.DataBlockParentId,

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

    private async Task<Either<ActionResult, Unit>> CopyReleaseRoles(
        Guid originalReleaseId,
        Guid amendmentReleaseVersionId,
        DateTime createdDate
    )
    {
        // Copy all current roles apart from Prerelease Users to the Release amendment.
        var newRoles = await userReleaseRoleRepository
            .Query()
            .AsNoTracking()
            .WhereForReleaseVersion(originalReleaseId)
            .WhereRolesNotIn(ReleaseRole.PrereleaseViewer)
            .Select(urr => new UserReleaseRole
            {
                // Assign a new Id.
                Id = Guid.NewGuid(),
                // Assign it to the amended release version.
                ReleaseVersionId = amendmentReleaseVersionId,
                // Copy certain fields from the original.
                Role = urr.Role,
                UserId = urr.UserId,
                // Assign the new created date.
                Created = createdDate,
            })
            .ToListAsync();

        context.AddRange(newRoles);
        await context.SaveChangesAsync();

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

    private static string FilterOutComments(string bodyText)
    {
        if (bodyText.IsNullOrEmpty())
        {
            return bodyText;
        }

        return CommentsRegex().Replace(bodyText, _ => string.Empty);
    }
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
                .ThenInclude(keyStat => (keyStat as KeyStatisticDataBlock)!.DataBlock)
            .Include(releaseVersion => releaseVersion.FeaturedTables)
            .Include(releaseVersion => releaseVersion.DataBlockVersions)
            .Include(releaseVersion => releaseVersion.DataBlockVersions)
                .ThenInclude(dataBlockVersion => dataBlockVersion.DataBlockParent)
                    .ThenInclude(dataBlockParent => dataBlockParent.LatestDraftVersion)
            .Include(releaseVersion => releaseVersion.DataBlockVersions)
                .ThenInclude(dataBlockVersion => dataBlockVersion.DataBlockParent)
                    .ThenInclude(dataBlockParent => dataBlockParent.LatestPublishedVersion)
                        .ThenInclude(dataBlockVersion =>
                            dataBlockVersion != null ? dataBlockVersion.ContentBlock : null
                        );
    }
}
