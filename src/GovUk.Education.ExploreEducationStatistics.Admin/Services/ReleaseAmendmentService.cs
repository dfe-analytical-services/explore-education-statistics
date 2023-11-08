#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class ReleaseAmendmentService : IReleaseAmendmentService
{
    private readonly ContentDbContext _context;
    private readonly IReleaseService _releaseService;
    private readonly IFootnoteService _footnoteService;
    private readonly StatisticsDbContext _statisticsDbContext;
    private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
    private readonly IUserService _userService;

    public ReleaseAmendmentService(
        ContentDbContext context,
        IReleaseService releaseService,
        IPersistenceHelper<ContentDbContext> persistenceHelper,
        IUserService userService,
        IFootnoteService footnoteService,
        StatisticsDbContext statisticsDbContext)
    {
        _context = context;
        _persistenceHelper = persistenceHelper;
        _userService = userService;
        _footnoteService = footnoteService;
        _statisticsDbContext = statisticsDbContext;
        _releaseService = releaseService;
    }

    public async Task<Either<ActionResult, ReleaseViewModel>> CreateReleaseAmendment(Guid releaseId)
    {
        return await _persistenceHelper
            .CheckEntityExists<Release>(releaseId, HydrateReleaseForAmendment)
            .OnSuccess(_userService.CheckCanMakeAmendmentOfRelease)
            .OnSuccess(originalRelease =>
                CreateBasicReleaseAmendment(originalRelease)
                    .OnSuccess(CreateStatisticsReleaseAmendment)
                    .OnSuccess(amendment => CopyReleaseRoles(releaseId, amendment))
                    .OnSuccessDo(amendment => _footnoteService.CopyFootnotes(releaseId, amendment.Id))
                    .OnSuccess(amendment => CopyFileLinks(originalRelease, amendment))
                    .OnSuccess(amendment => _releaseService.GetRelease(amendment.Id)));
    }

    private async Task<Either<ActionResult, Release>> CreateBasicReleaseAmendment(Release originalRelease)
    {
        var createdDate = DateTime.UtcNow;
        var createdByUserId = _userService.GetUserId();

        var amendment = originalRelease.Clone();

        // Set new values for fields that should be altered in the amended
        // Release rather than copied from the original Release
        amendment.Id = Guid.NewGuid();
        amendment.Published = null;
        amendment.PublishScheduled = null;
        amendment.ApprovalStatus = ReleaseApprovalStatus.Draft;
        amendment.NotifiedOn = null;
        amendment.NotifySubscribers = false;
        amendment.UpdatePublishedDate = false;
        // EES-4637 - we need to decide on how we're being consistent with Created dates in Release Amendments.
        amendment.Created = createdDate;
        amendment.CreatedById = createdByUserId;
        amendment.Version = originalRelease.Version + 1;
        amendment.PreviousVersionId = originalRelease.Id;

        // Create new DataBlockVersions for each DataBlockParent and for each, replace the "LatestDraftVersion"
        // with the new DataBlockVersion.
        amendment.DataBlockVersions = amendment
            .DataBlockVersions
            .Select(originalDataBlockVersion =>
            {
                var clonedDataBlockVersion = originalDataBlockVersion.Clone(amendment);
                clonedDataBlockVersion.DataBlockParent.LatestPublishedVersion = originalDataBlockVersion;
                clonedDataBlockVersion.DataBlockParent.LatestPublishedVersionId = originalDataBlockVersion.Id;
                clonedDataBlockVersion.DataBlockParent.LatestDraftVersion = clonedDataBlockVersion;
                clonedDataBlockVersion.DataBlockParent.LatestDraftVersionId = clonedDataBlockVersion.Id;
                return clonedDataBlockVersion;
            })
            .ToList();

        // Clone all non-DataBlock Content Blocks found within Release Content.
        Dictionary<ContentBlock, ContentBlock> originalToClonedContentBlocks = originalRelease
            .Content
            .SelectMany(section => section.Content)
            .Where(block => block is not DataBlock)
            .ToDictionary(
                block => block,
                block => block.Clone(amendment));

        var originalToClonedDataBlocks = amendment
            .DataBlockVersions
            .Select(dataBlockVersion => dataBlockVersion.DataBlockParent)
            .ToDictionary(
                dataBlockParent => dataBlockParent.LatestPublishedVersion!.ContentBlock,
                dataBlockParent => dataBlockParent.LatestDraftVersion!.ContentBlock);

        var allClonedBlocks = originalToClonedContentBlocks
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        originalToClonedDataBlocks.ForEach(kv => allClonedBlocks.Add(kv.Key, kv.Value));

        // Copy ContentSections, using the newly-cloned ContentBlocks and DataBlocks in the new ContentSections
        // rather than the original ones.
        amendment.Content = amendment
            .Content
            .Select(section => section.Clone(amendment, allClonedBlocks))
            .ToList();

        // NOTE: This is to ensure that a RelatedDashboards ContentSection exists on all new amendments.
        // There are older releases without a RelatedDashboards ContentSection.
        if (!amendment
                .Content
                .Any(c => c is { Type: ContentSectionType.RelatedDashboards }))
        {
            amendment.Content.Add(new ContentSection
            {
                Id = Guid.NewGuid(),
                Type = ContentSectionType.RelatedDashboards,
                Content = new List<ContentBlock>(),
                Release = amendment,
                ReleaseId = amendment.Id
            });
        }

        amendment.KeyStatistics = amendment.KeyStatistics.Select(originalKeyStatistic =>
        {
            var amendmentKeyStatistic = originalKeyStatistic.Clone(amendment);

            if (originalKeyStatistic is KeyStatisticDataBlock originalKeyStatDataBlock
                && amendmentKeyStatistic is KeyStatisticDataBlock amendmentKeyStatDataBlock)
            {
                var amendmentDataBlock = originalToClonedDataBlocks[originalKeyStatDataBlock.DataBlock];
                amendmentKeyStatDataBlock.DataBlock = amendmentDataBlock;
                amendmentKeyStatDataBlock.DataBlockId = amendmentDataBlock.Id;
            }

            return amendmentKeyStatistic;
        }).ToList();

        amendment.FeaturedTables = amendment.FeaturedTables.Select(originalFeaturedTable =>
        {
            var amendmentFeaturedTable = originalFeaturedTable.Clone(amendment);

            var amendmentDataBlock = originalToClonedDataBlocks[originalFeaturedTable.DataBlock];
            amendmentFeaturedTable.DataBlock = amendmentDataBlock;
            amendmentFeaturedTable.DataBlockId = amendmentDataBlock.Id;

            return amendmentFeaturedTable;
        }).ToList();

        amendment.RelatedInformation = amendment
            .RelatedInformation
            .Select(link => link.Clone())
            .ToList();

        amendment.Updates = amendment
            .Updates
            .Select(update => update.Clone(amendment))
            .ToList();

        UpdateAmendmentContent(allClonedBlocks);

        await _context.Releases.AddAsync(amendment);
        await _context.SaveChangesAsync();
        return amendment;
    }

    private async Task<Either<ActionResult, Release>> CreateStatisticsReleaseAmendment(Release amendment)
    {
        var statsRelease = await _statisticsDbContext
            .Release
            .FirstOrDefaultAsync(r => r.Id == amendment.PreviousVersionId);

        // Release does not have to have stats uploaded but if it has then
        // create a link row to link back to the original subject
        if (statsRelease != null)
        {
            var statsAmendment = statsRelease.CreateReleaseAmendment(amendment.Id);

            var statsAmendmentSubjectLinks = _statisticsDbContext
                .ReleaseSubject
                .AsQueryable()
                .Where(rs => rs.ReleaseId == amendment.PreviousVersionId)
                .Select(rs => rs.CopyForRelease(statsAmendment));

            await _statisticsDbContext.Release.AddAsync(statsAmendment);
            await _statisticsDbContext.ReleaseSubject.AddRangeAsync(statsAmendmentSubjectLinks);

            await _statisticsDbContext.SaveChangesAsync();
        }

        return amendment;
    }

    private async Task<Either<ActionResult, Release>> CopyReleaseRoles(Guid originalReleaseId, Release amendment)
    {
        // Copy all current roles apart from Prerelease Users to the Release amendment.
        var newRoles = _context
            .UserReleaseRoles
            // For auditing purposes, we also want to migrate release roles that have Deleted set (when a role is
            // manually removed from a Release as opposed to SoftDeleted, which is only set when a Release is
            // deleted)
            .IgnoreQueryFilters()
            .Where(releaseRole => releaseRole.ReleaseId == originalReleaseId
                                  && releaseRole.Role != ReleaseRole.PrereleaseViewer)
            .Select(releaseRole => releaseRole.CopyForAmendment(amendment))
            .ToList();

        await _context.AddRangeAsync(newRoles);
        await _context.SaveChangesAsync();
        return amendment;
    }

    private async Task<Either<ActionResult, Release>> CopyFileLinks(Release originalRelease, Release newRelease)
    {
        var releaseFileCopies = _context
            .ReleaseFiles
            .Include(f => f.File)
            .Where(f => f.ReleaseId == originalRelease.Id)
            .Select(f => f.CreateReleaseAmendment(newRelease)).ToList();

        await _context.ReleaseFiles.AddRangeAsync(releaseFileCopies);
        await _context.SaveChangesAsync();
        return newRelease;
    }

    private static void UpdateAmendmentContent(Dictionary<ContentBlock, ContentBlock> allClonedBlocks)
    {
        var replacements = new Dictionary<string, MatchEvaluator>();

        var regex = new Regex(
            string.Join('|', replacements.Keys.Append(ContentFilterUtils.CommentsFilterPattern)),
            RegexOptions.Compiled
        );

        foreach (var amendmentBlock in allClonedBlocks.Values)
        {
            switch (amendmentBlock)
            {
                case HtmlBlock block:
                    block.Body = ReplaceContent(block.Body, regex, replacements);
                    break;

                case MarkDownBlock block:
                    block.Body = ReplaceContent(block.Body, regex, replacements);
                    break;

                case EmbedBlockLink block:
                    block.EmbedBlock = block.EmbedBlock.Clone();
                    break;
            }
        }
    }

    private static string ReplaceContent(
        string content,
        Regex regex,
        Dictionary<string, MatchEvaluator> replacements)
    {
        if (content.IsNullOrEmpty())
        {
            return content;
        }

        return regex.Replace(
            content,
            match =>
            {
                if (replacements.ContainsKey(match.Value))
                {
                    return replacements[match.Value](match);
                }

                // Assume that it is a filtered match and we should remove it.
                return string.Empty;
            }
        );
    }

    private static IQueryable<Release> HydrateReleaseForAmendment(IQueryable<Release> queryable)
    {
        return queryable
            .AsSplitQuery()
            .Include(release => release.Publication)
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