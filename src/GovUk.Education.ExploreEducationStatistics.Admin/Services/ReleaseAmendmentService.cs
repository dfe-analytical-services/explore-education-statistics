#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Util;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Cache;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyPublishingStrategy;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyApprovalStatus;
using IReleaseRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseRepository;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

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

    private async Task<Either<ActionResult, Release>> CreateBasicReleaseAmendment(Release release)
    {
        var amendment = release.CreateAmendment(DateTime.UtcNow, _userService.GetUserId());
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