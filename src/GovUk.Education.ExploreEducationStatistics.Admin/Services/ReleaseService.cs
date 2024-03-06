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
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyPublishingStrategy;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyApprovalStatus;
using IReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseVersionRepository;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ReleaseService : IReleaseService
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
        private readonly IReleaseSubjectRepository _releaseSubjectRepository;
        private readonly IGuidGenerator _guidGenerator;
        private readonly IBlobCacheService _cacheService;

        // TODO EES-212 - ReleaseService needs breaking into smaller services as it feels like it is now doing too
        // much work and has too many dependencies
        public ReleaseService(
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
            IReleaseSubjectRepository releaseSubjectRepository,
            IGuidGenerator guidGenerator,
            IBlobCacheService cacheService)
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
            _releaseSubjectRepository = releaseSubjectRepository;
            _guidGenerator = guidGenerator;
            _cacheService = cacheService;
        }

        public async Task<Either<ActionResult, ReleaseViewModel>> GetRelease(Guid releaseVersionId)
        {
            return await _persistenceHelper
                .CheckEntityExists<ReleaseVersion>(releaseVersionId, HydrateReleaseVersion)
                .OnSuccess(_userService.CheckCanViewReleaseVersion)
                .OnSuccess(releaseVersion => _mapper
                    .Map<ReleaseViewModel>(releaseVersion) with
                    {
                        PreReleaseUsersOrInvitesAdded = _context
                            .UserReleaseRoles
                            .Any(role => role.ReleaseVersionId == releaseVersionId
                                         && role.Role == ReleaseRole.PrereleaseViewer) ||
                            _context
                            .UserReleaseInvites
                            .Any(role => role.ReleaseVersionId == releaseVersionId
                                         && role.Role == ReleaseRole.PrereleaseViewer)
                    });
        }

        public async Task<Either<ActionResult, ReleaseViewModel>> CreateRelease(ReleaseCreateRequest releaseCreate)
        {
            return await ReleaseCreateRequestValidator.Validate(releaseCreate)
                .OnSuccess(async () => await _persistenceHelper.CheckEntityExists<Publication>(releaseCreate.PublicationId))
                .OnSuccess(_userService.CheckCanCreateReleaseForPublication)
                .OnSuccess(async _ => await ValidateReleaseSlugUniqueToPublication(releaseCreate.Slug, releaseCreate.PublicationId))
                .OnSuccess(async () =>
                {
                    var newReleaseVersion = new ReleaseVersion
                    {
                        Id = _guidGenerator.NewGuid(),
                        ReleaseParent = new ReleaseParent(),
                        PublicationId = releaseCreate.PublicationId,
                        Slug = releaseCreate.Slug,
                        TimePeriodCoverage = releaseCreate.TimePeriodCoverage,
                        ReleaseName = releaseCreate.Year.ToString(),
                        Type = releaseCreate.Type,
                        ApprovalStatus = ReleaseApprovalStatus.Draft
                    };

                    if (releaseCreate.TemplateReleaseId.HasValue)
                    {
                        await CreateGenericContentFromTemplate(releaseCreate.TemplateReleaseId.Value,
                            newReleaseVersion);
                    }
                    else
                    {
                        newReleaseVersion.GenericContent = new List<ContentSection>();
                    }

                    newReleaseVersion.SummarySection = new ContentSection
                    {
                        Type = ContentSectionType.ReleaseSummary,
                    };
                    newReleaseVersion.KeyStatisticsSecondarySection = new ContentSection
                    {
                        Type = ContentSectionType.KeyStatisticsSecondary,
                    };
                    newReleaseVersion.HeadlinesSection = new ContentSection
                    {
                        Type = ContentSectionType.Headlines,
                    };
                    newReleaseVersion.RelatedDashboardsSection = new ContentSection
                    {
                        Type = ContentSectionType.RelatedDashboards,
                    };
                    newReleaseVersion.Created = DateTime.UtcNow;
                    newReleaseVersion.CreatedById = _userService.GetUserId();

                    _context.ReleaseVersions.Add(newReleaseVersion);
                    await _context.SaveChangesAsync();
                    return await GetRelease(newReleaseVersion.Id);
                });
        }

        public Task<Either<ActionResult, DeleteReleasePlan>> GetDeleteReleasePlan(Guid releaseVersionId)
        {
            return _persistenceHelper
                .CheckEntityExists<ReleaseVersion>(releaseVersionId)
                .OnSuccess(_userService.CheckCanDeleteReleaseVersion)
                .OnSuccess(_ =>
                {
                    var methodologiesScheduledWithRelease =
                        GetMethodologiesScheduledWithRelease(releaseVersionId)
                        .Select(m => new IdTitleViewModel(m.Id, m.Title))
                        .ToList();

                    return new DeleteReleasePlan
                    {
                        ScheduledMethodologies = methodologiesScheduledWithRelease
                    };
                });
        }

        public Task<Either<ActionResult, Unit>> DeleteRelease(Guid releaseVersionId)
        {
            return _persistenceHelper
                .CheckEntityExists<ReleaseVersion>(releaseVersionId)
                .OnSuccess(_userService.CheckCanDeleteReleaseVersion)
                .OnSuccessDo(async release => await _cacheService.DeleteCacheFolderAsync(
                    new PrivateReleaseContentFolderCacheKey(release.Id)))
                .OnSuccessDo(async () => await _releaseDataFileService.DeleteAll(releaseVersionId))
                .OnSuccessDo(async () => await _releaseFileService.DeleteAll(releaseVersionId))
                .OnSuccessVoid(async release =>
                {
                    release.SoftDeleted = true;
                    _context.Update(release);

                    var roles = await _context
                        .UserReleaseRoles
                        .AsQueryable()
                        .Where(r => r.ReleaseVersionId == releaseVersionId)
                        .ToListAsync();
                    roles.ForEach(r => r.SoftDeleted = true);
                    _context.UpdateRange(roles);

                    var invites = await _context
                        .UserReleaseInvites
                        .AsQueryable()
                        .Where(r => r.ReleaseVersionId == releaseVersionId)
                        .ToListAsync();
                    invites.ForEach(r => r.SoftDeleted = true);
                    _context.UpdateRange(invites);

                    var methodologiesScheduledWithRelease =
                        GetMethodologiesScheduledWithRelease(releaseVersionId);

                    // TODO EES-2747 - this should be looked at to see how best to reuse similar "set to draft" logic
                    // in MethodologyApprovalService.
                    methodologiesScheduledWithRelease.ForEach(m =>
                    {
                        m.PublishingStrategy = Immediately;
                        m.Status = Draft;
                        m.ScheduledWithReleaseVersion = null;
                        m.ScheduledWithReleaseVersionId = null;
                        m.Updated = DateTime.UtcNow;
                    });

                    _context.UpdateRange(methodologiesScheduledWithRelease);

                    await _context.SaveChangesAsync();

                    await _releaseSubjectRepository.DeleteAllReleaseSubjects(releaseVersionId: releaseVersionId);
                });
        }

        public Task<Either<ActionResult, ReleasePublicationStatusViewModel>> GetReleasePublicationStatus(
            Guid releaseVersionId)
        {
            return _persistenceHelper
                .CheckEntityExists<ReleaseVersion>(releaseVersionId)
                .OnSuccess(_userService.CheckCanViewReleaseVersion)
                .OnSuccess(_mapper.Map<ReleasePublicationStatusViewModel>);
        }

        public async Task<Either<ActionResult, ReleaseViewModel>> UpdateRelease(
            Guid releaseVersionId, ReleaseUpdateRequest request)
        {
            return await ReleaseUpdateRequestValidator.Validate(request)
                .OnSuccess(async () => await CheckReleaseExists(releaseVersionId))
                .OnSuccess(_userService.CheckCanUpdateReleaseVersion)
                .OnSuccessDo(async releaseVersion =>
                    await ValidateReleaseSlugUniqueToPublication(request.Slug,
                        publicationId: releaseVersion.PublicationId,
                        releaseVersionId: releaseVersionId))
                .OnSuccess(async releaseVersion =>
                {
                    releaseVersion.Slug = request.Slug;
                    releaseVersion.Type = request.Type;
                    releaseVersion.ReleaseName = request.Year.ToString();
                    releaseVersion.TimePeriodCoverage = request.TimePeriodCoverage;
                    releaseVersion.PreReleaseAccessList = request.PreReleaseAccessList;

                    _context.ReleaseVersions.Update(releaseVersion);
                    await _context.SaveChangesAsync();
                    return await GetRelease(releaseVersionId);
                });
        }

        public async Task<Either<ActionResult, Unit>> UpdateReleasePublished(Guid releaseVersionId,
            ReleasePublishedUpdateRequest request)
        {
            return await _persistenceHelper
                .CheckEntityExists<ReleaseVersion>(releaseVersionId,
                    queryable => queryable.Include(rv => rv.Publication))
                .OnSuccessDo(_userService.CheckIsBauUser)
                .OnSuccess<ActionResult, ReleaseVersion, Unit>(async release =>
                {
                    if (release.Published == null)
                    {
                        return ValidationActionResult(ReleaseNotPublished);
                    }

                    var newPublishedDate = request.Published?.ToUniversalTime() ?? DateTime.UtcNow;

                    // Prevent assigning a future date since it would have the effect of un-publishing the release
                    if (newPublishedDate > DateTime.UtcNow)
                    {
                        return ValidationActionResult(ReleasePublishedCannotBeFutureDate);
                    }

                    _context.ReleaseVersions.Update(release);
                    release.Published = newPublishedDate;
                    await _context.SaveChangesAsync();

                    // Update the cached release version
                    await _releaseCacheService.UpdateRelease(
                        releaseVersionId,
                        publicationSlug: release.Publication.Slug,
                        releaseSlug: release.Slug);

                    if (release.Publication.LatestPublishedReleaseVersionId == releaseVersionId)
                    {
                        // This is the latest published release version so also update the latest cached release version
                        // for the publication which is a separate cache entry
                        await _releaseCacheService.UpdateRelease(
                            releaseVersionId,
                            publicationSlug: release.Publication.Slug);
                    }

                    return Unit.Instance;
                });
        }

        public async Task<Either<ActionResult, IdTitleViewModel>> GetLatestPublishedRelease(Guid publicationId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Publication>(publicationId, queryable =>
                    queryable.Include(rv => rv.LatestPublishedReleaseVersion))
                .OnSuccess(_userService.CheckCanViewPublication)
                .OnSuccess(publication =>
                {
                    var latestPublishedReleaseVersion = publication.LatestPublishedReleaseVersion;
                    return latestPublishedReleaseVersion != null
                        ? new IdTitleViewModel
                        {
                            Id = latestPublishedReleaseVersion.Id,
                            Title = latestPublishedReleaseVersion.Title
                        }
                        : new Either<ActionResult, IdTitleViewModel>(new NotFoundResult());
                });
        }

        public async Task<Either<ActionResult, List<ReleaseSummaryViewModel>>> ListReleasesWithStatuses(
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
                .OnSuccess(async releases =>
                {
                    return await releases
                        .ToAsyncEnumerable()
                        .SelectAwait(async release =>
                        {
                            var releaseViewModel = _mapper.Map<ReleaseSummaryViewModel>(release);
                            releaseViewModel.Permissions =
                                await PermissionsUtils.GetReleasePermissions(_userService, release);
                            return releaseViewModel;
                        }).ToListAsync();
                });
        }

        public async Task<Either<ActionResult, List<ReleaseSummaryViewModel>>> ListUsersReleasesForApproval()
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
                .Include(releaseVersion => releaseVersion.Publication)
                .Where(releaseVersion =>
                    releaseVersion.ApprovalStatus == ReleaseApprovalStatus.HigherLevelReview
                    && releaseVersionIdsForApproval.Contains(releaseVersion.Id))
                .ToListAsync();

            return _mapper.Map<List<ReleaseSummaryViewModel>>(releaseVersionsForApproval);
        }

        public async Task<Either<ActionResult, List<ReleaseSummaryViewModel>>> ListScheduledReleases()
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
                .OnSuccess(async releases =>
                {
                    var approvedReleases = await releases
                        .ToAsyncEnumerable()
                        .SelectAwait(async release =>
                        {
                            var releaseViewModel = _mapper.Map<ReleaseSummaryViewModel>(release);
                            releaseViewModel.Permissions =
                                await PermissionsUtils.GetReleasePermissions(_userService, release);
                            return releaseViewModel;
                        }).ToListAsync();

                    return approvedReleases
                        .Where(release => !release.Live)
                        .ToList();
                });
        }

        public async Task<Either<ActionResult, DeleteDataFilePlan>> GetDeleteDataFilePlan(Guid releaseVersionId,
            Guid fileId)
        {
            return await _context.ReleaseVersions
                .FirstOrNotFoundAsync(rv => rv.Id == releaseVersionId)
                .OnSuccess(_userService.CheckCanUpdateReleaseVersion)
                .OnSuccess(() => CheckFileExists(fileId))
                .OnSuccessCombineWith(file => _statisticsDbContext.Subject
                    .FirstOrNotFoundAsync(s => s.Id == file.SubjectId))
                .OnSuccess(async tuple =>
                {
                    var (file, subject) = tuple;

                    var footnotes =
                        await _footnoteRepository.GetFootnotes(releaseVersionId: releaseVersionId,
                            subjectId: file.SubjectId);

                    return new DeleteDataFilePlan
                    {
                        ReleaseId = releaseVersionId,
                        SubjectId = subject.Id,
                        DeleteDataBlockPlan = await _dataBlockService.GetDeletePlan(releaseVersionId, subject),
                        FootnoteIds = footnotes.Select(footnote => footnote.Id).ToList()
                    };
                });
        }

        public async Task<Either<ActionResult, Unit>> RemoveDataFiles(Guid releaseVersionId, Guid fileId)
        {
            return await _persistenceHelper
                .CheckEntityExists<ReleaseVersion>(releaseVersionId)
                .OnSuccess(_userService.CheckCanUpdateReleaseVersion)
                .OnSuccess(() => CheckFileExists(fileId))
                .OnSuccessDo(file => CheckCanDeleteDataFiles(releaseVersionId, file))
                .OnSuccessDo(async file =>
                {
                    // Delete any replacement that might exist
                    if (file.ReplacedById.HasValue)
                    {
                        return await RemoveDataFiles(releaseVersionId, file.ReplacedById.Value);
                    }

                    return Unit.Instance;
                })
                .OnSuccess(_ => GetDeleteDataFilePlan(releaseVersionId, fileId))
                .OnSuccessDo(deletePlan => _dataBlockService.DeleteDataBlocks(deletePlan.DeleteDataBlockPlan))
                .OnSuccessVoid(async deletePlan =>
                {
                    await _releaseSubjectRepository.DeleteReleaseSubject(releaseVersionId: releaseVersionId,
                        subjectId: deletePlan.SubjectId);
                    await _cacheService.DeleteItemAsync(new PrivateSubjectMetaCacheKey(
                        releaseVersionId: releaseVersionId,
                        subjectId: deletePlan.SubjectId));
                })
                .OnSuccess(() => _releaseDataFileService.Delete(releaseVersionId, fileId));
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

        private async Task<Either<ActionResult, ReleaseVersion>> CheckReleaseExists(Guid releaseVersionId)
        {
            return await _context
                .ReleaseVersions
                .HydrateReleaseForChecklist()
                .SingleOrNotFoundAsync(rv => rv.Id == releaseVersionId);
        }

        private async Task<Either<ActionResult, File>> CheckFileExists(Guid id)
        {
            return await _persistenceHelper.CheckEntityExists<File>(id)
                .OnSuccess(file => file.Type != FileType.Data
                    ? new Either<ActionResult, File>(
                        ValidationActionResult(FileTypeMustBeData))
                    : file);
        }

        private async Task<Either<ActionResult, Unit>> ValidateReleaseSlugUniqueToPublication(string slug,
            Guid publicationId,
            Guid? releaseVersionId = null)
        {
            var releaseVersions = await _context.ReleaseVersions
                .Where(rv => rv.PublicationId == publicationId)
                .ToListAsync();

            if (releaseVersions.Any(release =>
                    release.Slug == slug &&
                    release.Id != releaseVersionId &&
                    IsLatestVersionOfRelease(releaseVersions, release.Id)))
            {
                return ValidationActionResult(SlugNotUnique);
            }

            return Unit.Instance;
        }

        private static bool IsLatestVersionOfRelease(IEnumerable<ReleaseVersion> releaseVersions, Guid releaseVersionId)
        {
            return !releaseVersions.Any(rv => rv.PreviousVersionId == releaseVersionId && rv.Id != releaseVersionId);
        }

        private async Task CreateGenericContentFromTemplate(Guid templateReleaseVersionId,
            ReleaseVersion newReleaseVersion)
        {
            var templateReleaseVersion = await _context
                .ReleaseVersions
                .AsNoTracking()
                .Include(releaseVersion => releaseVersion.Content)
                .FirstAsync(rv => rv.Id == templateReleaseVersionId);

            newReleaseVersion.Content = templateReleaseVersion
                .Content
                .Where(section => section.Type == ContentSectionType.Generic)
                .Select(section => CloneContentSectionFromReleaseTemplate(section, newReleaseVersion))
                .ToList();
        }

        private ContentSection CloneContentSectionFromReleaseTemplate(
            ContentSection originalSection,
            ReleaseVersion newReleaseVersion)
        {
            // Create a new ContentSection based upon the original template.
            return new ContentSection
            {
                // Assign a new Id.
                Id = Guid.NewGuid(),

                // Assign it to the new release version.
                ReleaseVersionId = newReleaseVersion.Id,

                // Copy certain fields from the original.
                Caption = originalSection.Caption,
                Heading = originalSection.Heading,
                Order = originalSection.Order,
                Type = originalSection.Type
            };
        }

        private async Task<bool> CanUpdateDataFiles(Guid releaseVersionId)
        {
            var releaseVersion = await _context.ReleaseVersions.FirstAsync(rv => rv.Id == releaseVersionId);
            return releaseVersion.ApprovalStatus != ReleaseApprovalStatus.Approved;
        }

        private async Task<Either<ActionResult, Unit>> CheckCanDeleteDataFiles(Guid releaseVersionId, File file)
        {
            var import = await _dataImportService.GetImport(file.Id);
            var importStatus = import?.Status ?? DataImportStatus.NOT_FOUND;

            if (!importStatus.IsFinished())
            {
                return ValidationActionResult(CannotRemoveDataFilesUntilImportComplete);
            }

            if (!await CanUpdateDataFiles(releaseVersionId))
            {
                return ValidationActionResult(CannotRemoveDataFilesOnceReleaseApproved);
            }

            return Unit.Instance;
        }

        public static IQueryable<ReleaseVersion> HydrateReleaseVersion(IQueryable<ReleaseVersion> values)
        {
            // Require publication / release graph to be able to work out:
            // If the release is the latest
            return values
                .Include(rv => rv.Publication)
                .Include(rv => rv.ReleaseStatuses);
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

    public class DeleteDataFilePlan
    {
        [JsonIgnore]
        public Guid ReleaseId { get; set; }

        [JsonIgnore]
        public Guid SubjectId { get; set; }

        public DeleteDataBlockPlan DeleteDataBlockPlan { get; set; } = null!;

        public List<Guid> FootnoteIds { get; set; } = null!;
    }

    public class DeleteReleasePlan
    {
        public List<IdTitleViewModel> ScheduledMethodologies { get; set; } = new();
    }
}
