#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Util;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyApprovalStatus;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyPublishingStrategy;
using IReleaseVersionRepository =
    GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseVersionRepository;
using ValidationUtils = GovUk.Education.ExploreEducationStatistics.Common.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ReleaseVersionService : IReleaseVersionService
    {
        private readonly ContentDbContext _context;
        private readonly StatisticsDbContext _statisticsDbContext;
        private readonly IMapper _mapper;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IUserService _userService;
        private readonly IReleaseVersionRepository _releaseVersionRepository;
        private readonly IReleaseCacheService _releaseCacheService;
        private readonly IReleaseFileRepository _releaseFileRepository;
        private readonly IReleaseDataFileService _releaseDataFileService;
        private readonly IReleaseFileService _releaseFileService;
        private readonly IDataImportService _dataImportService;
        private readonly IFootnoteRepository _footnoteRepository;
        private readonly IDataBlockService _dataBlockService;
        private readonly IReleasePublishingStatusRepository _releasePublishingStatusRepository;
        private readonly IReleaseSubjectRepository _releaseSubjectRepository;
        private readonly IDataSetVersionService _dataSetVersionService;
        private readonly IProcessorClient _processorClient;
        private readonly IPrivateBlobCacheService _privateCacheService;

        // TODO EES-212 - ReleaseService needs breaking into smaller services as it feels like it is now doing too
        // much work and has too many dependencies
        public ReleaseVersionService(
            ContentDbContext context,
            StatisticsDbContext statisticsDbContext,
            IMapper mapper,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IUserService userService,
            IReleaseVersionRepository releaseVersionRepository,
            IReleaseCacheService releaseCacheService,
            IReleaseFileRepository releaseFileRepository,
            IReleaseDataFileService releaseDataFileService,
            IReleaseFileService releaseFileService,
            IDataImportService dataImportService,
            IFootnoteRepository footnoteRepository,
            IDataBlockService dataBlockService,
            IReleasePublishingStatusRepository releasePublishingStatusRepository,
            IReleaseSubjectRepository releaseSubjectRepository,
            IDataSetVersionService dataSetVersionService,
            IProcessorClient processorClient,
            IPrivateBlobCacheService privateCacheService)
        {
            _context = context;
            _statisticsDbContext = statisticsDbContext;
            _mapper = mapper;
            _persistenceHelper = persistenceHelper;
            _userService = userService;
            _releaseVersionRepository = releaseVersionRepository;
            _releaseCacheService = releaseCacheService;
            _releaseFileRepository = releaseFileRepository;
            _releaseDataFileService = releaseDataFileService;
            _releaseFileService = releaseFileService;
            _dataImportService = dataImportService;
            _footnoteRepository = footnoteRepository;
            _dataBlockService = dataBlockService;
            _releasePublishingStatusRepository = releasePublishingStatusRepository;
            _releaseSubjectRepository = releaseSubjectRepository;
            _dataSetVersionService = dataSetVersionService;
            _processorClient = processorClient;
            _privateCacheService = privateCacheService;
        }

        public async Task<Either<ActionResult, ReleaseVersionViewModel>> GetRelease(Guid releaseVersionId)
        {
            return await _context
                .ReleaseVersions
                .Include(rv => rv.Release)
                .ThenInclude(r => r.Publication)
                .Include(rv => rv.ReleaseStatuses)
                .SingleOrNotFoundAsync(rv => rv.Id == releaseVersionId)
                .OnSuccess(_userService.CheckCanViewReleaseVersion)
                .OnSuccess(releaseVersion =>
                {
                    var prereleaseRolesOrInvitesAdded =
                        _context
                            .UserReleaseRoles
                            .Any(role => role.ReleaseVersionId == releaseVersionId
                                         && role.Role == ReleaseRole.PrereleaseViewer) ||
                        _context
                            .UserReleaseInvites
                            .Any(role => role.ReleaseVersionId == releaseVersionId
                                         && role.Role == ReleaseRole.PrereleaseViewer);

                    return _mapper.Map<ReleaseVersionViewModel>(releaseVersion) with
                    {
                        PreReleaseUsersOrInvitesAdded = prereleaseRolesOrInvitesAdded,
                    };
                });
        }

        public async Task<Either<ActionResult, DeleteReleasePlanViewModel>> GetDeleteReleaseVersionPlan(
            Guid releaseVersionId,
            CancellationToken cancellationToken = default)
        {
            return await _context
                .ReleaseVersions
                .SingleOrNotFoundAsync(rv => rv.Id == releaseVersionId, cancellationToken)
                .OnSuccess(_userService.CheckCanDeleteReleaseVersion)
                .OnSuccess(_ =>
                {
                    var methodologiesScheduledWithRelease =
                        GetMethodologiesScheduledWithRelease(releaseVersionId)
                            .Select(m => new IdTitleViewModel(m.Id, m.Title))
                            .ToList();

                    return new DeleteReleasePlanViewModel
                    {
                        ScheduledMethodologies = methodologiesScheduledWithRelease
                    };
                });
        }

        public Task<Either<ActionResult, Unit>> DeleteReleaseVersion(
            Guid releaseVersionId,
            CancellationToken cancellationToken = default)
        {
            return _context
                .ReleaseVersions
                .SingleOrNotFoundAsync(releaseVersion => releaseVersion.Id == releaseVersionId, cancellationToken)
                .OnSuccess(_userService.CheckCanDeleteReleaseVersion)
                .OnSuccess(releaseVersion => DoDeleteReleaseVersion(
                    releaseVersion: releaseVersion,
                    forceDeleteRelatedData: false,
                    hardDeleteContentReleaseVersion: !releaseVersion.Amendment,
                    cancellationToken));
        }

        public Task<Either<ActionResult, Unit>> DeleteTestReleaseVersion(
            Guid releaseVersionId,
            CancellationToken cancellationToken = default)
        {
            return _context
                .ReleaseVersions
                .IgnoreQueryFilters()
                .SingleOrNotFoundAsync(releaseVersion => releaseVersion.Id == releaseVersionId, cancellationToken)
                .OnSuccessDo(_userService.CheckCanDeleteTestReleaseVersion)
                .OnSuccessDo(async releaseVersion =>
                {
                    // Unset any Soft Deleted flags on Test ReleaseVersions / Subjects
                    // prior to hard-deleting them, as a number of Services / Repositories that
                    // will be called during the hard delete are not configured to look for
                    // soft-deleted data.
                    var subjects = await _statisticsDbContext
                        .ReleaseSubject
                        .IgnoreQueryFilters()
                        .Where(releaseSubject => releaseSubject.ReleaseVersionId == releaseVersion.Id)
                        .Select(releaseSubject => releaseSubject.Subject)
                        .ToListAsync(cancellationToken);

                    releaseVersion.SoftDeleted = false;
                    subjects.ForEach(subject => subject.SoftDeleted = false);

                    _context.ReleaseVersions.Update(releaseVersion);
                    _statisticsDbContext.Subject.UpdateRange(subjects);

                    await _context.SaveChangesAsync(cancellationToken);
                    await _statisticsDbContext.SaveChangesAsync(cancellationToken);
                })
                .OnSuccess(releaseVersion => DoDeleteReleaseVersion(
                    releaseVersion: releaseVersion,
                    forceDeleteRelatedData: true,
                    hardDeleteContentReleaseVersion: true,
                    cancellationToken));
        }

        private async Task<Either<ActionResult, Unit>> DoDeleteReleaseVersion(
            ReleaseVersion releaseVersion,
            bool forceDeleteRelatedData = false,
            bool hardDeleteContentReleaseVersion = false,
            CancellationToken cancellationToken = default)
        {
            return await _processorClient
                .BulkDeleteDataSetVersions(
                    releaseVersionId: releaseVersion.Id,
                    forceDeleteAll: forceDeleteRelatedData,
                    cancellationToken: cancellationToken)
                .OnSuccessDo(async _ =>
                    await _privateCacheService.DeleteCacheFolderAsync(
                        new PrivateReleaseContentFolderCacheKey(releaseVersion.Id)))
                .OnSuccessDo(() => _releaseDataFileService.DeleteAll(
                    releaseVersionId: releaseVersion.Id,
                    forceDelete: forceDeleteRelatedData))
                .OnSuccessDo(() => _releaseFileService.DeleteAll(
                    releaseVersionId: releaseVersion.Id,
                    forceDelete: forceDeleteRelatedData))
                .OnSuccessDo(async _ =>
                {
                    if (hardDeleteContentReleaseVersion)
                    {
                        await HardDeleteReleaseVersion(releaseVersion, cancellationToken);
                    }
                    else
                    {
                        await SoftDeleteReleaseVersion(releaseVersion, cancellationToken);
                    }

                    UpdateMethodologies(releaseVersion.Id);

                    await _context.SaveChangesAsync(cancellationToken);

                    if (releaseVersion.ApprovalStatus == ReleaseApprovalStatus.Approved)
                    {
                        // Delete release entries in the Azure Storage ReleaseStatus table - if not it will attempt to publish
                        // deleted releases that were left scheduled
                        await _releasePublishingStatusRepository.RemovePublisherReleaseStatuses(releaseVersionIds:
                            [releaseVersion.Id]);
                    }

                    // TODO: This may be redundant (investigate as part of EES-1295)
                    await _releaseSubjectRepository.DeleteAllReleaseSubjects(
                        releaseVersionId: releaseVersion.Id,
                        softDeleteOrphanedSubjects: !forceDeleteRelatedData);

                    if (forceDeleteRelatedData)
                    {
                        var statsReleaseVersion = await _statisticsDbContext
                            .ReleaseVersion
                            .SingleOrDefaultAsync(
                                statsReleaseVersion => statsReleaseVersion.Id == releaseVersion.Id,
                                cancellationToken);

                        if (statsReleaseVersion != null)
                        {
                            _statisticsDbContext.ReleaseVersion.Remove(statsReleaseVersion);
                            await _statisticsDbContext.SaveChangesAsync(cancellationToken);
                        }
                    }
                });
        }

        private async Task HardDeleteReleaseVersion(
            ReleaseVersion releaseVersion,
            CancellationToken cancellationToken)
        {
            await DeleteReleaseSeriesItem(releaseVersion, cancellationToken);
            await DeleteDataBlocks(releaseVersion.Id, cancellationToken);

            _context.ReleaseVersions.Remove(releaseVersion);
            await _context.SaveChangesAsync(cancellationToken);

            var release = await _context
                .Releases
                .Include(release => release.Versions)
                .SingleAsync(
                    release => release.Id == releaseVersion.ReleaseId,
                    cancellationToken);

            if (release.Versions.Count == 0)
            {
                _context.Releases.Remove(release);
                await _context.SaveChangesAsync(cancellationToken);
            }

            // We suspect this is only necessary for the unit tests, as the in-memory database doesn't perform a cascade delete
            await DeleteRoles(releaseVersion.Id, hardDelete: true, cancellationToken);
            await DeleteInvites(releaseVersion.Id, hardDelete: true, cancellationToken);
        }

        private async Task SoftDeleteReleaseVersion(
            ReleaseVersion releaseVersion,
            CancellationToken cancellationToken)
        {
            releaseVersion.SoftDeleted = true;
            _context.ReleaseVersions.Update(releaseVersion);

            await DeleteRoles(releaseVersion.Id, hardDelete: false, cancellationToken);
            await DeleteInvites(releaseVersion.Id, hardDelete: false, cancellationToken);
        }

        // TODO: UserReleaseRoles deletion should probably be handled by cascade deletion of the associated ReleaseVersion (investigate as part of EES-1295)
        private async Task DeleteRoles(
            Guid releaseVersionId,
            bool hardDelete,
            CancellationToken cancellationToken)
        {
            var roles = await _context
                .UserReleaseRoles
                .AsQueryable()
                .Where(r => r.ReleaseVersionId == releaseVersionId)
                .ToListAsync(cancellationToken);

            if (hardDelete)
            {
                _context.UserReleaseRoles.RemoveRange(roles);
            }
            else
            {
                roles.ForEach(r => r.SoftDeleted = true);
                _context.UpdateRange(roles);
            }
        }

        // TODO: UserReleaseInvites deletion should probably be handled by cascade deletion of the associated ReleaseVersion (investigate as part of EES-1295)
        private async Task DeleteInvites(
            Guid releaseVersionId,
            bool hardDelete,
            CancellationToken cancellationToken)
        {
            var invites = await _context
                .UserReleaseInvites
                .AsQueryable()
                .Where(r => r.ReleaseVersionId == releaseVersionId)
                .ToListAsync(cancellationToken);

            if (hardDelete)
            {
                _context.UserReleaseInvites.RemoveRange(invites);
            }
            else
            {
                invites.ForEach(r => r.SoftDeleted = true);
                _context.UpdateRange(invites);
            }
        }

        private async Task DeleteReleaseSeriesItem(
            ReleaseVersion releaseVersion,
            CancellationToken cancellationToken)
        {
            var publication = await _context.Publications.FindAsync(releaseVersion.PublicationId, cancellationToken);
            var releaseSeriesItem = publication!.ReleaseSeries.Find(rs => rs.ReleaseId == releaseVersion.ReleaseId);

            publication.ReleaseSeries.Remove(releaseSeriesItem!);
            _context.Publications.Update(publication);
        }

        private async Task DeleteDataBlocks(Guid releaseVersionId, CancellationToken cancellationToken = default)
        {
            var dataBlockVersions = await _context
                .DataBlockVersions
                .Include(dataBlockVersion => dataBlockVersion.DataBlockParent)
                .Where(dataBlockVersion => dataBlockVersion.ReleaseVersionId == releaseVersionId)
                .ToListAsync(cancellationToken);

            var dataBlockParents = dataBlockVersions
                .Select(dataBlockVersion => dataBlockVersion.DataBlockParent)
                .Distinct()
                .ToList();

            // Unset the DataBlockVersion references from the DataBlockParent.
            dataBlockParents.ForEach(dataBlockParent =>
            {
                dataBlockParent.LatestDraftVersionId = null;
                dataBlockParent.LatestPublishedVersionId = null;
            });

            await _context.SaveChangesAsync(cancellationToken);

            // Then remove the now-unreferenced DataBlockVersions.
            _context.DataBlockVersions.RemoveRange(dataBlockVersions);
            await _context.SaveChangesAsync(cancellationToken);

            // And finally, delete the DataBlockParents if they are now orphaned.
            var orphanedDataBlockParents = dataBlockParents
                .Where(dataBlockParent =>
                    !_context
                        .DataBlockVersions
                        .Any(dataBlockVersion => dataBlockVersion.DataBlockParentId == dataBlockParent.Id))
                .ToList();

            _context.DataBlockParents.RemoveRange(orphanedDataBlockParents);
            await _context.SaveChangesAsync(cancellationToken);
        }

        private void UpdateMethodologies(Guid releaseVersionId)
        {
            var methodologiesScheduledWithRelease = GetMethodologiesScheduledWithRelease(releaseVersionId);

            // TODO EES-2747 - this should be looked at to see how best to reuse similar "set to draft" logic in MethodologyApprovalService.
            methodologiesScheduledWithRelease.ForEach(m =>
            {
                m.PublishingStrategy = Immediately;
                m.Status = Draft;
                m.ScheduledWithReleaseVersion = null;
                m.ScheduledWithReleaseVersionId = null;
                m.Updated = DateTime.UtcNow;
            });

            _context.UpdateRange(methodologiesScheduledWithRelease);
        }

        public Task<Either<ActionResult, ReleasePublicationStatusViewModel>> GetReleasePublicationStatus(
            Guid releaseVersionId)
        {
            return _persistenceHelper
                .CheckEntityExists<ReleaseVersion>(releaseVersionId)
                .OnSuccess(_userService.CheckCanViewReleaseVersion)
                .OnSuccess(_mapper.Map<ReleasePublicationStatusViewModel>);
        }

        public async Task<Either<ActionResult, ReleaseVersionViewModel>> UpdateReleaseVersion(
            Guid releaseVersionId, ReleaseVersionUpdateRequest request)
        {
            return await ReleaseVersionUpdateRequestValidator.Validate(request)
                .OnSuccess(async () => await _context.ReleaseVersions
                    .Include(rv => rv.Release)
                    .SingleOrNotFoundAsync(rv => rv.Id == releaseVersionId))
                .OnSuccess(_userService.CheckCanUpdateReleaseVersion)
                .OnSuccessDo(releaseVersion => ValidateUpdateRequest(releaseVersion, request))
                .OnSuccessDo(async releaseVersion =>
                    await ValidateReleaseSlugUniqueToPublication(
                        slug: request.Slug,
                        publicationId: releaseVersion.PublicationId,
                        releaseId: releaseVersion.ReleaseId))
                .OnSuccessDo(async releaseVersion =>
                    await _context.RequireTransaction(() =>
                        UpdateReleaseAndVersion(request, releaseVersion)
                            .OnSuccessDo(async () => await UpdateApiDataSetVersions(releaseVersion)))
                )
                .OnSuccess(async () => await GetRelease(releaseVersionId));
        }

        public async Task<Either<ActionResult, Unit>> UpdateReleasePublished(
            Guid releaseVersionId,
            ReleasePublishedUpdateRequest request)
        {
            return await _context
                .ReleaseVersions
                .Include(rv => rv.Release)
                .ThenInclude(r => r.Publication)
                .SingleOrNotFoundAsync(rv => rv.Id == releaseVersionId)
                .OnSuccessDo(_userService.CheckIsBauUser)
                .OnSuccess<ActionResult, ReleaseVersion, Unit>(async releaseVersion =>
                {
                    if (releaseVersion.Published == null)
                    {
                        return ValidationActionResult(ReleaseNotPublished);
                    }

                    var newPublishedDate = request.Published?.ToUniversalTime() ?? DateTime.UtcNow;

                    // Prevent assigning a future date since it would have the effect of un-publishing the release
                    if (newPublishedDate > DateTime.UtcNow)
                    {
                        return ValidationActionResult(ReleasePublishedCannotBeFutureDate);
                    }

                    _context.ReleaseVersions.Update(releaseVersion);
                    releaseVersion.Published = newPublishedDate;
                    await _context.SaveChangesAsync();

                    // Update the cached release version
                    await _releaseCacheService.UpdateRelease(
                        releaseVersionId,
                        publicationSlug: releaseVersion.Release.Publication.Slug,
                        releaseSlug: releaseVersion.Release.Slug);

                    if (releaseVersion.Release.Publication.LatestPublishedReleaseVersionId == releaseVersionId)
                    {
                        // This is the latest published release version so also update the latest cached release version
                        // for the publication which is a separate cache entry
                        await _releaseCacheService.UpdateRelease(
                            releaseVersionId,
                            publicationSlug: releaseVersion.Release.Publication.Slug);
                    }

                    return Unit.Instance;
                });
        }

        public async Task<Either<ActionResult, IdTitleViewModel>> GetLatestPublishedRelease(Guid publicationId)
        {
            return await _context.Publications
                .Include(p => p.LatestPublishedReleaseVersion)
                .ThenInclude(rv => rv.Release)
                .SingleOrNotFoundAsync(p => p.Id == publicationId)
                .OnSuccess(_userService.CheckCanViewPublication)
                .OnSuccess(p => p.LatestPublishedReleaseVersion != null
                    ? new IdTitleViewModel
                    {
                        Id = p.LatestPublishedReleaseVersion.Id,
                        Title = p.LatestPublishedReleaseVersion.Release.Title
                    }
                    : new Either<ActionResult, IdTitleViewModel>(new NotFoundResult()));
        }

        public async Task<Either<ActionResult, List<ReleaseVersionSummaryViewModel>>> ListReleasesWithStatuses(
            params ReleaseApprovalStatus[] releaseApprovalStatuses)
        {
            return await _userService
                .CheckCanAccessSystem()
                .OnSuccess(_ =>
                {
                    return _userService
                        .CheckCanViewAllReleases()
                        .OnSuccess(() => _releaseVersionRepository.ListReleases(releaseApprovalStatuses))
                        .OrElse(() =>
                            _releaseVersionRepository.ListReleasesForUser(_userService.GetUserId(),
                                releaseApprovalStatuses));
                })
                .OnSuccess(async releaseVersions =>
                {
                    return await releaseVersions
                        .ToAsyncEnumerable()
                        .SelectAwait(async releaseVersion => _mapper.Map<ReleaseVersionSummaryViewModel>(releaseVersion) with
                        {
                            Permissions = await PermissionsUtils.GetReleasePermissions(_userService, releaseVersion) 
                        }).ToListAsync();
                });
        }

        public async Task<Either<ActionResult, List<ReleaseVersionSummaryViewModel>>> ListUsersReleasesForApproval()
        {
            var userId = _userService.GetUserId();

            var directReleasesWithApprovalRole = await _context
                .UserReleaseRoles
                .Where(role => role.UserId == userId && role.Role == ReleaseRole.Approver)
                .Select(role => role.ReleaseVersionId)
                .ToListAsync();

            var indirectReleasesWithApprovalRole = await _context
                .UserPublicationRoles
                .Where(role => role.UserId == userId && role.Role == PublicationRole.Approver)
                .SelectMany(role => role.Publication.ReleaseVersions.Select(releaseVersion => releaseVersion.Id))
                .ToListAsync();

            var releaseVersionIdsForApproval = directReleasesWithApprovalRole
                .Concat(indirectReleasesWithApprovalRole)
                .Distinct();

            var releaseVersionsForApproval = await _context
                .ReleaseVersions
                .Include(releaseVersion => releaseVersion.Release)
                .ThenInclude(release => release.Publication)
                .Where(releaseVersion =>
                    releaseVersion.ApprovalStatus == ReleaseApprovalStatus.HigherLevelReview
                    && releaseVersionIdsForApproval.Contains(releaseVersion.Id))
                .ToListAsync();

            return _mapper.Map<List<ReleaseVersionSummaryViewModel>>(releaseVersionsForApproval);
        }

        public async Task<Either<ActionResult, List<ReleaseVersionSummaryViewModel>>> ListScheduledReleases()
        {
            return await _userService
                .CheckCanAccessSystem()
                .OnSuccess(_ =>
                {
                    return _userService
                        .CheckCanViewAllReleases()
                        .OnSuccess(() => _releaseVersionRepository.ListReleases(ReleaseApprovalStatus.Approved))
                        .OrElse(() =>
                            _releaseVersionRepository.ListReleasesForUser(_userService.GetUserId(),
                                ReleaseApprovalStatus.Approved));
                })
                .OnSuccess(async releaseVersions =>
                {
                    var approvedReleases = await releaseVersions
                        .ToAsyncEnumerable()
                        .SelectAwait(async releaseVersion =>
                        {
                            var releaseViewModel = _mapper.Map<ReleaseVersionSummaryViewModel>(releaseVersion);
                            releaseViewModel.Permissions =
                                await PermissionsUtils.GetReleasePermissions(_userService, releaseVersion);
                            return releaseViewModel;
                        }).ToListAsync();

                    return approvedReleases
                        .Where(release => !release.Live)
                        .ToList();
                });
        }

        public async Task<Either<ActionResult, DeleteDataFilePlanViewModel>> GetDeleteDataFilePlan(
            Guid releaseVersionId,
            Guid fileId,
            CancellationToken cancellationToken = default)
        {
            return await _context.ReleaseVersions
                .FirstOrNotFoundAsync(rv => rv.Id == releaseVersionId)
                .OnSuccess(_userService.CheckCanUpdateReleaseVersion)
                .OnSuccess(() => CheckReleaseDataFileExists(releaseVersionId: releaseVersionId, fileId: fileId))
                .OnSuccessCombineWith(releaseFile => _statisticsDbContext.Subject
                    .FirstOrNotFoundAsync(s => s.Id == releaseFile.File.SubjectId))
                .OnSuccess(async tuple =>
                {
                    var (releaseFile, subject) = tuple;

                    return await GetLinkedDataSetVersion(releaseFile, cancellationToken)
                        .OnSuccess(apiDataSetVersion => (releaseFile, subject, apiDataSetVersion));
                })
                .OnSuccess(async tuple =>
                {
                    var footnotes =
                        await _footnoteRepository.GetFootnotes(releaseVersionId: releaseVersionId,
                            subjectId: tuple.releaseFile.File.SubjectId);

                    var linkedApiDataSetVersionDeletionPlan = tuple.apiDataSetVersion is null
                        ? null
                        : new ApiDataSetVersionPlanViewModel
                        {
                            DataSetId = tuple.apiDataSetVersion.DataSetId,
                            DataSetTitle = tuple.apiDataSetVersion.DataSet.Title,
                            Id = tuple.apiDataSetVersion.Id,
                            Version = tuple.apiDataSetVersion.PublicVersion,
                            Status = tuple.apiDataSetVersion.Status,
                            //Valid = false TODO: test whatever uses this...
                        };

                    return new DeleteDataFilePlanViewModel
                    {
                        ReleaseId = releaseVersionId,
                        SubjectId = tuple.subject.Id,
                        DeleteDataBlockPlan = await _dataBlockService.GetDeletePlan(releaseVersionId, tuple.subject),
                        FootnoteIds = footnotes.Select(footnote => footnote.Id).ToList(),
                        DeleteApiDataSetVersionPlan = linkedApiDataSetVersionDeletionPlan
                    };
                });
        }

        public async Task<Either<ActionResult, Unit>> RemoveDataFiles(Guid releaseVersionId, Guid fileId)
        {
            return await _context.ReleaseVersions
                .SingleOrNotFoundAsync(rv => rv.Id == releaseVersionId)
                .OnSuccess(_userService.CheckCanUpdateReleaseVersion)
                .OnSuccess(() => CheckReleaseDataFileExists(releaseVersionId: releaseVersionId, fileId: fileId))
                .OnSuccessDo(releaseFile => CheckCanDeleteDataFiles(releaseVersionId, releaseFile))
                .OnSuccessDo(async releaseFile =>
                {
                    // Delete any replacement that might exist
                    if (releaseFile.File.ReplacedById.HasValue)
                    {
                        return await RemoveDataFiles(
                            releaseVersionId: releaseVersionId,
                            fileId: releaseFile.File.ReplacedById.Value);
                    }

                    return Unit.Instance;
                })
                .OnSuccess(_ => GetDeleteDataFilePlan(releaseVersionId, fileId))
                .OnSuccessDo(deletePlan => _dataBlockService.DeleteDataBlocks(deletePlan.DeleteDataBlockPlan))
                .OnSuccessVoid(async deletePlan =>
                {
                    await _releaseSubjectRepository.DeleteReleaseSubject(releaseVersionId: releaseVersionId,
                        subjectId: deletePlan.SubjectId);
                    await _privateCacheService.DeleteItemAsync(new PrivateSubjectMetaCacheKey(
                        releaseVersionId: releaseVersionId,
                        subjectId: deletePlan.SubjectId));
                })
                .OnSuccessVoid(() => _releaseDataFileService.Delete(releaseVersionId, fileId));
        }

        public async Task<Either<ActionResult, DataImportStatusViewModel>> GetDataFileImportStatus(
            Guid releaseVersionId,
            Guid fileId)
        {
            return await _persistenceHelper
                .CheckEntityExists<ReleaseVersion>(releaseVersionId)
                .OnSuccess(_userService.CheckCanViewReleaseVersion)
                .OnSuccess(async _ =>
                {
                    // Ensure file is linked to the Release by getting the ReleaseFile first
                    var releaseFile =
                        await _releaseFileRepository.Find(releaseVersionId: releaseVersionId, fileId: fileId);
                    if (releaseFile == null || releaseFile.File.Type != FileType.Data)
                    {
                        return DataImportStatusViewModel.NotFound();
                    }

                    return await _dataImportService.GetImportStatus(fileId);
                });
        }

        private async Task<Either<ActionResult, ReleaseFile>> CheckReleaseDataFileExists(
            Guid releaseVersionId, Guid fileId)
        {
            return await _context.ReleaseFiles
                .Include(rf => rf.File)
                .Where(rf => rf.ReleaseVersionId == releaseVersionId)
                .Where(rf => rf.FileId == fileId)
                .SingleOrNotFoundAsync()
                .OnSuccess(rf => rf.File.Type != FileType.Data
                    ? new Either<ActionResult, ReleaseFile>(ValidationActionResult(FileTypeMustBeData))
                    : rf);
        }

        private async Task<Either<ActionResult, Unit>> ValidateReleaseSlugUniqueToPublication(
            string slug,
            Guid publicationId,
            Guid? releaseId = null)
        {
            var slugAlreadyExists = await _context.Releases
                .Where(r => r.PublicationId == publicationId)
                .AnyAsync(r => r.Slug == slug && r.Id != releaseId);

            return slugAlreadyExists 
                ? ValidationActionResult(SlugNotUnique) 
                : Unit.Instance;
        }

        private static Either<ActionResult, Unit> ValidateUpdateRequest(ReleaseVersion releaseVersion, ReleaseVersionUpdateRequest request)
        {
            if (releaseVersion.Version == 0)
            {
                return Unit.Instance;
            }

            var yearChanged = releaseVersion.Release.Year != request.Year;
            var timePeriodCoverageChanged =
                releaseVersion.Release.TimePeriodCoverage != request.TimePeriodCoverage;
            var labelChanged = releaseVersion.Release.Label != request.Label;

            return yearChanged || timePeriodCoverageChanged || labelChanged
                ? ValidationActionResult(UpdateRequestForPublishedReleaseVersionInvalid)
                : Unit.Instance;
        }

        private async Task<Either<ActionResult, Unit>> UpdateReleaseAndVersion(ReleaseVersionUpdateRequest request, ReleaseVersion releaseVersion)
        {
            releaseVersion.Release.Year = request.Year;
            releaseVersion.Release.TimePeriodCoverage = request.TimePeriodCoverage;
            releaseVersion.Release.Slug = request.Slug;
            releaseVersion.Release.Label = string.IsNullOrWhiteSpace(request.Label) ? null : request.Label.Trim();

            releaseVersion.Type = request.Type!.Value;
            releaseVersion.PreReleaseAccessList = request.PreReleaseAccessList;

            await _context.SaveChangesAsync();

            return Unit.Instance;
        }

        private async Task<Either<ActionResult, Unit>> UpdateApiDataSetVersions(ReleaseVersion releaseVersion)
        {
            await _dataSetVersionService.UpdateVersionsForReleaseVersion(
                releaseVersion.Id,
                releaseSlug: releaseVersion.Release.Slug,
                releaseTitle: releaseVersion.Release.Title);

            return Unit.Instance;
        }

        private async Task<bool> CanUpdateDataFiles(Guid releaseVersionId)
        {
            var releaseVersion = await _context.ReleaseVersions.FirstAsync(rv => rv.Id == releaseVersionId);
            return releaseVersion.ApprovalStatus != ReleaseApprovalStatus.Approved;
        }

        private async Task<Either<ActionResult, DataSetVersion?>> GetLinkedDataSetVersion(
            ReleaseFile releaseFile,
            CancellationToken cancellationToken = default)
        {
            if (releaseFile.PublicApiDataSetId is null)
            {
                return (DataSetVersion)null!;
            }

            return await _dataSetVersionService.GetDataSetVersion(
                    releaseFile.PublicApiDataSetId.Value,
                    releaseFile.PublicApiDataSetVersion!,
                    cancellationToken)
                .OnSuccess(dsv => (DataSetVersion?)dsv)
                .OnFailureDo(_ => throw new ApplicationException(
                    $"API data set version could not be found. Data set ID: '{releaseFile.PublicApiDataSetId}', version: '{releaseFile.PublicApiDataSetVersion}'"));
        }

        private async Task<Either<ActionResult, Unit>> CheckCanDeleteDataFiles(
            Guid releaseVersionId, ReleaseFile releaseFile)
        {
            var import = await _dataImportService.GetImport(releaseFile.FileId);
            var importStatus = import?.Status ?? DataImportStatus.NOT_FOUND;

            if (!importStatus.IsFinished())
            {
                return ValidationActionResult(CannotRemoveDataFilesUntilImportComplete);
            }

            if (!await CanUpdateDataFiles(releaseVersionId))
            {
                return ValidationActionResult(CannotRemoveDataFilesOnceReleaseApproved);
            }

            if (releaseFile.PublicApiDataSetId is not null)
            {
                return ValidationUtils.ValidationResult(new ErrorViewModel
                {
                    Code = ValidationMessages.CannotDeleteApiDataSetReleaseFile.Code,
                    Message = ValidationMessages.CannotDeleteApiDataSetReleaseFile.Message,
                    Detail = new ApiDataSetErrorDetail(releaseFile.PublicApiDataSetId.Value)
                });
            }

            return Unit.Instance;
        }

        private IList<MethodologyVersion> GetMethodologiesScheduledWithRelease(Guid releaseVersionId)
        {
            return _context
                .MethodologyVersions
                .Include(m => m.Methodology)
                .Where(m => releaseVersionId == m.ScheduledWithReleaseVersionId)
                .ToList();
        }
    }
}
