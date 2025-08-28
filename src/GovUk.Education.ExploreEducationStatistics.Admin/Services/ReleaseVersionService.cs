#nullable enable
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
using GovUk.Education.ExploreEducationStatistics.Common.Options;
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
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyApprovalStatus;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyPublishingStrategy;
using IReleaseVersionRepository =
    GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseVersionRepository;
using ValidationUtils = GovUk.Education.ExploreEducationStatistics.Common.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class ReleaseVersionService(
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
    IPrivateBlobCacheService privateCacheService,
    IOrganisationsValidator organisationsValidator,
    IUserReleaseInviteRepository userReleaseInviteRepository,
    IUserReleaseRoleRepository userReleaseRoleRepository,
    IReleaseSlugValidator releaseSlugValidator,
    IOptions<FeatureFlagsOptions> featureFlags,
    ILogger<ReleaseVersionService> logger) : IReleaseVersionService
{
    public async Task<Either<ActionResult, ReleaseVersionViewModel>> GetRelease(Guid releaseVersionId)
    {
        return await context
            .ReleaseVersions
            .Include(rv => rv.Release)
            .ThenInclude(r => r.Publication)
            .Include(rv => rv.PublishingOrganisations)
            .Include(rv => rv.ReleaseStatuses)
            .SingleOrNotFoundAsync(rv => rv.Id == releaseVersionId)
            .OnSuccess(userService.CheckCanViewReleaseVersion)
            .OnSuccess(releaseVersion =>
            {
                var prereleaseRolesOrInvitesAdded =
                    context
                        .UserReleaseRoles
                        .Any(role => role.ReleaseVersionId == releaseVersionId
                                     && role.Role == ReleaseRole.PrereleaseViewer) ||
                    context
                        .UserReleaseInvites
                        .Any(role => role.ReleaseVersionId == releaseVersionId
                                     && role.Role == ReleaseRole.PrereleaseViewer);

                return mapper.Map<ReleaseVersionViewModel>(releaseVersion) with
                {
                    PreReleaseUsersOrInvitesAdded = prereleaseRolesOrInvitesAdded,
                };
            });
    }

    public async Task<Either<ActionResult, DeleteReleasePlanViewModel>> GetDeleteReleaseVersionPlan(
        Guid releaseVersionId,
        CancellationToken cancellationToken = default)
    {
        return await context
            .ReleaseVersions
            .SingleOrNotFoundAsync(rv => rv.Id == releaseVersionId, cancellationToken)
            .OnSuccess(userService.CheckCanDeleteReleaseVersion)
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
        return context
            .ReleaseVersions
            .SingleOrNotFoundAsync(releaseVersion => releaseVersion.Id == releaseVersionId, cancellationToken)
            .OnSuccess(userService.CheckCanDeleteReleaseVersion)
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
        return context
            .ReleaseVersions
            .IgnoreQueryFilters()
            .SingleOrNotFoundAsync(releaseVersion => releaseVersion.Id == releaseVersionId, cancellationToken)
            .OnSuccessDo(userService.CheckCanDeleteTestReleaseVersion)
            .OnSuccessDo(async releaseVersion =>
            {
                // Unset any Soft Deleted flags on Test ReleaseVersions / Subjects
                // prior to hard-deleting them, as a number of Services / Repositories that
                // will be called during the hard delete are not configured to look for
                // soft-deleted data.
                var subjects = await statisticsDbContext
                    .ReleaseSubject
                    .IgnoreQueryFilters()
                    .Where(releaseSubject => releaseSubject.ReleaseVersionId == releaseVersion.Id)
                    .Select(releaseSubject => releaseSubject.Subject)
                    .ToListAsync(cancellationToken);

                releaseVersion.SoftDeleted = false;
                subjects.ForEach(subject => subject.SoftDeleted = false);

                context.ReleaseVersions.Update(releaseVersion);
                statisticsDbContext.Subject.UpdateRange(subjects);

                await context.SaveChangesAsync(cancellationToken);
                await statisticsDbContext.SaveChangesAsync(cancellationToken);
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
        return await processorClient
            .BulkDeleteDataSetVersions(
                releaseVersionId: releaseVersion.Id,
                forceDeleteAll: forceDeleteRelatedData,
                cancellationToken: cancellationToken)
            .OnSuccessDo(async _ =>
                await privateCacheService.DeleteCacheFolderAsync(
                    new PrivateReleaseContentFolderCacheKey(releaseVersion.Id)))
            .OnSuccessDo(() => releaseDataFileService.DeleteAll(
                releaseVersionId: releaseVersion.Id,
                forceDelete: forceDeleteRelatedData))
            .OnSuccessDo(() => releaseFileService.DeleteAll(
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

                await context.SaveChangesAsync(cancellationToken);

                if (releaseVersion.ApprovalStatus == ReleaseApprovalStatus.Approved)
                {
                    // Delete release entries in the Azure Storage ReleaseStatus table - if not it will attempt to publish
                    // deleted releases that were left scheduled
                    await releasePublishingStatusRepository.RemovePublisherReleaseStatuses(releaseVersionIds:
                        [releaseVersion.Id]);
                }

                // TODO: This may be redundant (investigate as part of EES-1295)
                await releaseSubjectRepository.DeleteAllReleaseSubjects(
                    releaseVersionId: releaseVersion.Id,
                    softDeleteOrphanedSubjects: !forceDeleteRelatedData);

                if (forceDeleteRelatedData)
                {
                    var statsReleaseVersion = await statisticsDbContext
                        .ReleaseVersion
                        .SingleOrDefaultAsync(
                            statsReleaseVersion => statsReleaseVersion.Id == releaseVersion.Id,
                            cancellationToken);

                    if (statsReleaseVersion != null)
                    {
                        statisticsDbContext.ReleaseVersion.Remove(statsReleaseVersion);
                        await statisticsDbContext.SaveChangesAsync(cancellationToken);
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

        context.ReleaseVersions.Remove(releaseVersion);
        await context.SaveChangesAsync(cancellationToken);

        var release = await context
            .Releases
            .Include(release => release.Versions)
            .SingleAsync(
                release => release.Id == releaseVersion.ReleaseId,
                cancellationToken);

        if (release.Versions.Count == 0)
        {
            context.Releases.Remove(release);
            await context.SaveChangesAsync(cancellationToken);
        }

        await RemoveRolesAndInvites(releaseVersion, cancellationToken);
    }

    private async Task SoftDeleteReleaseVersion(
        ReleaseVersion releaseVersion,
        CancellationToken cancellationToken)
    {
        releaseVersion.SoftDeleted = true;
        context.ReleaseVersions.Update(releaseVersion);

        await RemoveRolesAndInvites(releaseVersion, cancellationToken);
    }

    private async Task RemoveRolesAndInvites(ReleaseVersion releaseVersion, CancellationToken cancellationToken)
    {
        // TODO: UserReleaseRoles deletion should probably be handled by cascade deletion of the associated ReleaseVersion (investigate as part of EES-1295)
        await userReleaseRoleRepository.RemoveForReleaseVersion(
            releaseVersionId: releaseVersion.Id,
            cancellationToken: cancellationToken);

        await userReleaseInviteRepository.RemoveByReleaseVersion(
            releaseVersionId: releaseVersion.Id,
            cancellationToken: cancellationToken);
    }

    private async Task DeleteReleaseSeriesItem(
        ReleaseVersion releaseVersion,
        CancellationToken cancellationToken)
    {
        var publication = await context.Publications.FindAsync(releaseVersion.PublicationId, cancellationToken);
        var releaseSeriesItem = publication!.ReleaseSeries.Find(rs => rs.ReleaseId == releaseVersion.ReleaseId);

        publication.ReleaseSeries.Remove(releaseSeriesItem!);
        context.Publications.Update(publication);
    }

    private async Task DeleteDataBlocks(Guid releaseVersionId, CancellationToken cancellationToken = default)
    {
        var dataBlockVersions = await context
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

        await context.SaveChangesAsync(cancellationToken);

        // Then remove the now-unreferenced DataBlockVersions.
        context.DataBlockVersions.RemoveRange(dataBlockVersions);
        await context.SaveChangesAsync(cancellationToken);

        // And finally, delete the DataBlockParents if they are now orphaned.
        var orphanedDataBlockParents = dataBlockParents
            .Where(dataBlockParent =>
                !context
                    .DataBlockVersions
                    .Any(dataBlockVersion => dataBlockVersion.DataBlockParentId == dataBlockParent.Id))
            .ToList();

        context.DataBlockParents.RemoveRange(orphanedDataBlockParents);
        await context.SaveChangesAsync(cancellationToken);
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

        context.UpdateRange(methodologiesScheduledWithRelease);
    }

    public Task<Either<ActionResult, ReleasePublicationStatusViewModel>> GetReleasePublicationStatus(
        Guid releaseVersionId)
    {
        return persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(userService.CheckCanViewReleaseVersion)
            .OnSuccess(mapper.Map<ReleasePublicationStatusViewModel>);
    }

    public async Task<Either<ActionResult, ReleaseVersionViewModel>> UpdateReleaseVersion(
        Guid releaseVersionId, ReleaseVersionUpdateRequest request)
    {
        return await ReleaseVersionUpdateRequestValidator.Validate(request)
            .OnSuccess(async () => await context.ReleaseVersions
                .Include(rv => rv.Release)
                .Include(rv => rv.PublishingOrganisations)
                .SingleOrNotFoundAsync(rv => rv.Id == releaseVersionId))
            .OnSuccess(userService.CheckCanUpdateReleaseVersion)
            .OnSuccessDo(releaseVersion => ValidateUpdateRequest(releaseVersion, request))
            .OnSuccessDo(async releaseVersion =>
                await releaseSlugValidator.ValidateNewSlug(
                    newReleaseSlug: request.Slug,
                    publicationId: releaseVersion.Release.PublicationId,
                    releaseId: releaseVersion.ReleaseId))
            .OnSuccessCombineWith(async _ =>
                await organisationsValidator.ValidateOrganisations(
                    organisationIds: request.PublishingOrganisations,
                    path: nameof(ReleaseVersionUpdateRequest.PublishingOrganisations).ToLowerFirst()))
            .OnSuccessDo(async releaseVersionAndPublishingOrganisations =>
            {
                var (releaseVersion, publishingOrganisations) = releaseVersionAndPublishingOrganisations;
                return await context.RequireTransaction(() =>
                    UpdateReleaseAndVersion(request, releaseVersion, publishingOrganisations)
                        .OnSuccessDo(async () => await UpdateApiDataSetVersions(releaseVersion)));
            })
            .OnSuccess(async () => await GetRelease(releaseVersionId));
    }

    public async Task<Either<ActionResult, Unit>> UpdateReleasePublished(
        Guid releaseVersionId,
        ReleasePublishedUpdateRequest request)
    {
        return await context
            .ReleaseVersions
            .Include(rv => rv.Release)
            .ThenInclude(r => r.Publication)
            .SingleOrNotFoundAsync(rv => rv.Id == releaseVersionId)
            .OnSuccessDo(userService.CheckIsBauUser)
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

                context.ReleaseVersions.Update(releaseVersion);
                releaseVersion.Published = newPublishedDate;
                await context.SaveChangesAsync();

                // Update the cached release version
                await releaseCacheService.UpdateRelease(
                    releaseVersionId,
                    publicationSlug: releaseVersion.Release.Publication.Slug,
                    releaseSlug: releaseVersion.Release.Slug);

                if (releaseVersion.Release.Publication.LatestPublishedReleaseVersionId == releaseVersionId)
                {
                    // This is the latest published release version so also update the latest cached release version
                    // for the publication which is a separate cache entry
                    await releaseCacheService.UpdateRelease(
                        releaseVersionId,
                        publicationSlug: releaseVersion.Release.Publication.Slug);
                }

                return Unit.Instance;
            });
    }

    public async Task<Either<ActionResult, IdTitleViewModel>> GetLatestPublishedRelease(Guid publicationId)
    {
        return await context.Publications
            .Include(p => p.LatestPublishedReleaseVersion)
            .ThenInclude(rv => rv.Release)
            .SingleOrNotFoundAsync(p => p.Id == publicationId)
            .OnSuccess(userService.CheckCanViewPublication)
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
        return await userService
            .CheckCanAccessSystem()
            .OnSuccess(_ =>
            {
                return userService
                    .CheckCanViewAllReleases()
                    .OnSuccess(() => releaseVersionRepository.ListReleases(releaseApprovalStatuses))
                    .OrElse(() =>
                        releaseVersionRepository.ListReleasesForUser(userService.GetUserId(),
                            releaseApprovalStatuses));
            })
            .OnSuccess(async releaseVersions =>
            {
                return await releaseVersions
                    .ToAsyncEnumerable()
                    .SelectAwait(async releaseVersion => mapper.Map<ReleaseVersionSummaryViewModel>(releaseVersion) with
                    {
                        Permissions = await PermissionsUtils.GetReleasePermissions(userService, releaseVersion)
                    }).ToListAsync();
            });
    }

    public async Task<Either<ActionResult, List<ReleaseVersionSummaryViewModel>>> ListUsersReleasesForApproval()
    {
        var userId = userService.GetUserId();

        var directReleasesWithApprovalRole = await context
            .UserReleaseRoles
            .Where(role => role.UserId == userId && role.Role == ReleaseRole.Approver)
            .Select(role => role.ReleaseVersionId)
            .ToListAsync();

        var indirectReleasesWithApprovalRole = await context
            .UserPublicationRoles
            .Where(role => role.UserId == userId && role.Role == PublicationRole.Allower)
            .SelectMany(role => role.Publication.ReleaseVersions.Select(releaseVersion => releaseVersion.Id))
            .ToListAsync();

        var releaseVersionIdsForApproval = directReleasesWithApprovalRole
            .Concat(indirectReleasesWithApprovalRole)
            .Distinct();

        var releaseVersionsForApproval = await context
            .ReleaseVersions
            .Include(releaseVersion => releaseVersion.Release)
            .ThenInclude(release => release.Publication)
            .Where(releaseVersion =>
                releaseVersion.ApprovalStatus == ReleaseApprovalStatus.HigherLevelReview
                && releaseVersionIdsForApproval.Contains(releaseVersion.Id))
            .ToListAsync();

        return mapper.Map<List<ReleaseVersionSummaryViewModel>>(releaseVersionsForApproval);
    }

    public async Task<Either<ActionResult, List<ReleaseVersionSummaryViewModel>>> ListScheduledReleases()
    {
        return await userService
            .CheckCanAccessSystem()
            .OnSuccess(_ =>
            {
                return userService
                    .CheckCanViewAllReleases()
                    .OnSuccess(() => releaseVersionRepository.ListReleases(ReleaseApprovalStatus.Approved))
                    .OrElse(() =>
                        releaseVersionRepository.ListReleasesForUser(userService.GetUserId(),
                            ReleaseApprovalStatus.Approved));
            })
            .OnSuccess(async releaseVersions =>
            {
                var approvedReleases = await releaseVersions
                    .ToAsyncEnumerable()
                    .SelectAwait(async releaseVersion =>
                    {
                        var releaseViewModel = mapper.Map<ReleaseVersionSummaryViewModel>(releaseVersion);
                        releaseViewModel.Permissions =
                            await PermissionsUtils.GetReleasePermissions(userService, releaseVersion);
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
        return await context.ReleaseVersions
            .FirstOrNotFoundAsync(rv => rv.Id == releaseVersionId)
            .OnSuccess(userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(() => CheckReleaseDataFileExists(releaseVersionId: releaseVersionId, fileId: fileId))
            .OnSuccessCombineWith(releaseFile => statisticsDbContext.Subject
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
                    await footnoteRepository.GetFootnotes(releaseVersionId: releaseVersionId,
                        subjectId: tuple.releaseFile.File.SubjectId);

                var linkedApiDataSetVersionDeletionPlan = tuple.apiDataSetVersion is null
                    ? null
                    : new DeleteApiDataSetVersionPlanViewModel
                    {
                        DataSetId = tuple.apiDataSetVersion.DataSetId,
                        DataSetTitle = tuple.apiDataSetVersion.DataSet.Title,
                        Id = tuple.apiDataSetVersion.Id,
                        Version = tuple.apiDataSetVersion.PublicVersion,
                        Status = tuple.apiDataSetVersion.Status,
                        Valid = ShouldAllowApiDataSetDeletion(tuple.apiDataSetVersion.Status)
                    };

                return new DeleteDataFilePlanViewModel
                {
                    ReleaseId = releaseVersionId,
                    SubjectId = tuple.subject.Id,
                    DeleteDataBlockPlan = await dataBlockService.GetDeletePlan(releaseVersionId, tuple.subject),
                    FootnoteIds = footnotes.Select(footnote => footnote.Id).ToList(),
                    ApiDataSetVersionPlan = linkedApiDataSetVersionDeletionPlan
                };
            });
    }

    private async Task<Either<ActionResult, Unit>> ValidateDataFilesStatusForDeletion(ReleaseFile releaseFile)
    {
        var dataSetVersionStatus = await GetDataSetVersionStatus(releaseFile);
        var fileExistsInPublishedReleaseVersion = await context.ReleaseFiles
            .Include(rf => rf.ReleaseVersion)
            .AnyAsync(rf => rf.FileId == releaseFile.File.Id &&
                            rf.ReleaseVersion.Published != null);
        var datasetVersionStatusIsPublished = DataSetVersionAuthExtensions.PublicStatuses.Any(status => status == dataSetVersionStatus);
        var draftReleaseFile = !fileExistsInPublishedReleaseVersion;

        if (releaseFile.File.ReplacedById is not null && releaseFile.PublicApiDataSetId is not null)
        {
            return ValidationUtils.ValidationResult(new ErrorViewModel
            {
                Code = ValidationMessages.ReleaseFileMustBeOriginal.Code,
                Message = ValidationMessages.ReleaseFileMustBeOriginal.Message,
            });
        }
        if (releaseFile.File.ReplacingId is not null && !draftReleaseFile)
        {
            throw new InvalidOperationException(
                "A replacement file for a DRAFT release version cannot also be a PUBLISHED file.");
        }
        if (datasetVersionStatusIsPublished && draftReleaseFile)
        {
            throw new InvalidOperationException(
                "A DRAFT release version's file cannot be linked to a PUBLISHED API.");
        }

        return Unit.Instance;
    }

    public async Task<Either<ActionResult, Unit>> RemoveDataFiles(Guid releaseVersionId, Guid fileId)
    {
        return await context.ReleaseVersions
            .SingleOrNotFoundAsync(rv => rv.Id == releaseVersionId)
            .OnSuccess(userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(() => CheckReleaseDataFileExists(releaseVersionId: releaseVersionId, fileId: fileId))
            .OnSuccessDo(releaseFile => CheckCanDeleteDataFiles(releaseVersionId, releaseFile))
            .OnSuccessDo(ValidateDataFilesStatusForDeletion)
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
            .OnSuccessDo(deletePlan => dataBlockService.DeleteDataBlocks(deletePlan.DeleteDataBlockPlan))
            .OnSuccessDo(async deletePlan =>
            {
                await releaseSubjectRepository.DeleteReleaseSubject(releaseVersionId: releaseVersionId,
                    subjectId: deletePlan.SubjectId);
                await privateCacheService.DeleteItemAsync(new PrivateSubjectMetaCacheKey(
                    releaseVersionId: releaseVersionId,
                    subjectId: deletePlan.SubjectId));
            })
            .OnSuccessDo(DeleteDraftApiDataSetVersion)
            .OnSuccessVoid(() => releaseDataFileService.Delete(releaseVersionId, fileId));
    }

    private async Task<Either<ActionResult, Unit>> DeleteDraftApiDataSetVersion(DeleteDataFilePlanViewModel deletePlan)
    {
        // Skip when Status == DataSetVersionStatus.Published;  
        if (!featureFlags.Value.EnableReplacementOfPublicApiDataSets
            || deletePlan.ApiDataSetVersionPlan is null or { Valid: false })
        {
            return Unit.Instance;
        }

        return await dataSetVersionService.DeleteVersion(deletePlan.ApiDataSetVersionPlan!.Id);
    }

    public async Task<Either<ActionResult, DataImportStatusViewModel>> GetDataFileImportStatus(
        Guid releaseVersionId,
        Guid fileId)
    {
        return await persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(userService.CheckCanViewReleaseVersion)
            .OnSuccess(async _ =>
            {
                // Ensure file is linked to the Release by getting the ReleaseFile first
                var releaseFile =
                    await releaseFileRepository.Find(releaseVersionId: releaseVersionId, fileId: fileId);
                if (releaseFile == null || releaseFile.File.Type != FileType.Data)
                {
                    return DataImportStatusViewModel.NotFound();
                }

                return await dataImportService.GetImportStatus(fileId);
            });
    }

    private async Task<Either<ActionResult, ReleaseFile>> CheckReleaseDataFileExists(
        Guid releaseVersionId, Guid fileId)
    {
        return await context.ReleaseFiles
            .Include(rf => rf.File)
            .Where(rf => rf.ReleaseVersionId == releaseVersionId)
            .Where(rf => rf.FileId == fileId)
            .SingleOrNotFoundAsync()
            .OnSuccess(rf => rf.File.Type != FileType.Data
                ? new Either<ActionResult, ReleaseFile>(ValidationActionResult(FileTypeMustBeData))
                : rf);
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

    private async Task<Either<ActionResult, Unit>> UpdateReleaseAndVersion(
        ReleaseVersionUpdateRequest request,
        ReleaseVersion releaseVersion,
        Organisation[] publishingOrganisations)
    {
        releaseVersion.Release.Year = request.Year;
        releaseVersion.Release.TimePeriodCoverage = request.TimePeriodCoverage;
        releaseVersion.Release.Slug = request.Slug;
        releaseVersion.Release.Label = string.IsNullOrWhiteSpace(request.Label) ? null : request.Label.Trim();

        releaseVersion.PublishingOrganisations = [.. publishingOrganisations];
        releaseVersion.Type = request.Type!.Value;
        releaseVersion.PreReleaseAccessList = request.PreReleaseAccessList;

        await context.SaveChangesAsync();

        return Unit.Instance;
    }

    private async Task<Either<ActionResult, Unit>> UpdateApiDataSetVersions(ReleaseVersion releaseVersion)
    {
        await dataSetVersionService.UpdateVersionsForReleaseVersion(
            releaseVersion.Id,
            releaseSlug: releaseVersion.Release.Slug,
            releaseTitle: releaseVersion.Release.Title);

        return Unit.Instance;
    }

    private async Task<bool> CanUpdateDataFiles(Guid releaseVersionId)
    {
        var releaseVersion = await context.ReleaseVersions.FirstAsync(rv => rv.Id == releaseVersionId);
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

        return await dataSetVersionService.GetDataSetVersion(
                releaseFile.PublicApiDataSetId.Value,
                releaseFile.PublicApiDataSetVersion!,
                cancellationToken)
            .OnSuccess(dsv => (DataSetVersion?)dsv)
            .OnFailureDo(_ =>
            {
                logger.LogError(
                    $"API data set version associated with release file could not be found. Data set ID: '{releaseFile.PublicApiDataSetId}', version: '{releaseFile.PublicApiDataSetVersion}', release file ID: '{releaseFile.Id}'.");
                throw new InvalidOperationException(
                    "Failed to find the associated API data set version for the release file.");
            });
    }

    private async Task<Either<ActionResult, Unit>> CheckCanDeleteDataFiles(Guid releaseVersionId, ReleaseFile releaseFile)
    {
        var import = await dataImportService.GetImport(releaseFile.FileId);
        var importStatus = import?.Status ?? DataImportStatus.NOT_FOUND;

        if (!importStatus.IsFinished())
        {
            return ValidationActionResult(CannotRemoveDataFilesUntilImportComplete);
        }

        if (!await CanUpdateDataFiles(releaseVersionId))
        {
            return ValidationActionResult(CannotRemoveDataFilesOnceReleaseApproved);
        }

        if (!featureFlags.Value.EnableReplacementOfPublicApiDataSets && releaseFile.PublicApiDataSetId is not null)
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

    private async Task<DataSetVersionStatus?> GetDataSetVersionStatus(ReleaseFile releaseFile)
    {
        if (releaseFile.PublicApiDataSetId == null)
        {
            return null;
        }

        DataSetVersionStatus? versionStatus = null;
        await dataSetVersionService
                .GetDataSetVersion(
                    releaseFile.PublicApiDataSetId.Value,
                    releaseFile.PublicApiDataSetVersion!)
                .OnFailureDo(_ =>
                {
                    var errorMessage =
                        "Failed to find the data set version expected to be linked to the release file that is being deleted.";
                    var notFoundException = new InvalidOperationException(errorMessage);
                    logger.LogError(notFoundException,
                        errorMessage +
                        $" Details: Data set id: {releaseFile.PublicApiDataSetId.Value} and the data set version number: {releaseFile.PublicApiDataSetVersionString}.");
                    throw notFoundException;
                }).OnSuccess(dsv => versionStatus = dsv.Status);

        return versionStatus;
    }

    private bool ShouldAllowApiDataSetDeletion(DataSetVersionStatus? dataSetVersionStatus)
    {
        return featureFlags.Value.EnableReplacementOfPublicApiDataSets
               && DataSetVersionAuthExtensions.PublicStatuses.All(status => status != dataSetVersionStatus);
    }

    private IList<MethodologyVersion> GetMethodologiesScheduledWithRelease(Guid releaseVersionId)
    {
        return context
            .MethodologyVersions
            .Include(m => m.Methodology)
            .Where(m => releaseVersionId == m.ScheduledWithReleaseVersionId)
            .ToList();
    }
}
