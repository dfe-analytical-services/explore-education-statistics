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
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;
using IReleaseRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseRepository;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;
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
        private readonly IReleaseRepository _repository;
        private readonly IReleaseCacheService _releaseCacheService;
        private readonly IReleaseFileRepository _releaseFileRepository;
        private readonly ISubjectRepository _subjectRepository;
        private readonly IReleaseDataFileService _releaseDataFileService;
        private readonly IReleaseFileService _releaseFileService;
        private readonly IDataImportService _dataImportService;
        private readonly IFootnoteService _footnoteService;
        private readonly IFootnoteRepository _footnoteRepository;
        private readonly IDataBlockService _dataBlockService;
        private readonly IReleaseSubjectRepository _releaseSubjectRepository;
        private readonly IGuidGenerator _guidGenerator;
        private readonly IBlobCacheService _cacheService;

        // TODO EES-212 - ReleaseService needs breaking into smaller services as it feels like it is now doing too
        // much work and has too many dependencies
        public ReleaseService(
            ContentDbContext context,
            IMapper mapper,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IUserService userService,
            IReleaseRepository repository,
            IReleaseCacheService releaseCacheService,
            IReleaseFileRepository releaseFileRepository,
            ISubjectRepository subjectRepository,
            IReleaseDataFileService releaseDataFileService,
            IReleaseFileService releaseFileService,
            IDataImportService dataImportService,
            IFootnoteService footnoteService,
            IFootnoteRepository footnoteRepository,
            StatisticsDbContext statisticsDbContext,
            IDataBlockService dataBlockService,
            IReleaseSubjectRepository releaseSubjectRepository,
            IGuidGenerator guidGenerator,
            IBlobCacheService cacheService)
        {
            _context = context;
            _mapper = mapper;
            _persistenceHelper = persistenceHelper;
            _userService = userService;
            _repository = repository;
            _releaseCacheService = releaseCacheService;
            _releaseFileRepository = releaseFileRepository;
            _subjectRepository = subjectRepository;
            _releaseDataFileService = releaseDataFileService;
            _releaseFileService = releaseFileService;
            _dataImportService = dataImportService;
            _footnoteService = footnoteService;
            _footnoteRepository = footnoteRepository;
            _statisticsDbContext = statisticsDbContext;
            _dataBlockService = dataBlockService;
            _releaseSubjectRepository = releaseSubjectRepository;
            _guidGenerator = guidGenerator;
            _cacheService = cacheService;
        }

        public async Task<Either<ActionResult, ReleaseViewModel>> GetRelease(Guid id)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(id, HydrateRelease)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(release => _mapper
                    .Map<ReleaseViewModel>(release) with
                    {
                        PreReleaseUsersOrInvitesAdded = _context
                            .UserReleaseRoles
                            .Any(role => role.ReleaseId == id 
                                         && role.Role == ReleaseRole.PrereleaseViewer) ||
                            _context
                            .UserReleaseInvites
                            .Any(role => role.ReleaseId == id 
                                         && role.Role == ReleaseRole.PrereleaseViewer)
                    });
        }

        public async Task<Either<ActionResult, ReleaseViewModel>> CreateRelease(ReleaseCreateRequest releaseCreate)
        {
            return await _persistenceHelper
                .CheckEntityExists<Publication>(releaseCreate.PublicationId)
                .OnSuccess(_userService.CheckCanCreateReleaseForPublication)
                .OnSuccess(async _ => await ValidateReleaseSlugUniqueToPublication(releaseCreate.Slug, releaseCreate.PublicationId))
                .OnSuccess(async () =>
                {
                    var release = new Release
                    {
                        Id = _guidGenerator.NewGuid(),
                        PublicationId =  releaseCreate.PublicationId,
                        Slug = releaseCreate.Slug,
                        TimePeriodCoverage = releaseCreate.TimePeriodCoverage,
                        ReleaseName = releaseCreate.Year.ToString(),
                        Type = releaseCreate.Type,
                        ApprovalStatus = ReleaseApprovalStatus.Draft
                    };

                    if (releaseCreate.TemplateReleaseId.HasValue)
                    {
                        await CreateGenericContentFromTemplate(releaseCreate.TemplateReleaseId.Value, release);
                    }
                    else
                    {
                        release.GenericContent = new List<ContentSection>();
                    }

                    release.SummarySection = new ContentSection
                    {
                        Type = ContentSectionType.ReleaseSummary,
                    };
                    release.KeyStatisticsSecondarySection = new ContentSection
                    {
                        Type = ContentSectionType.KeyStatisticsSecondary,
                    };
                    release.HeadlinesSection = new ContentSection
                    {
                        Type = ContentSectionType.Headlines,
                    };
                    release.RelatedDashboardsSection = new ContentSection
                    {
                        Type = ContentSectionType.RelatedDashboards,
                    };
                    release.Created = DateTime.UtcNow;
                    release.CreatedById = _userService.GetUserId();

                    await _context.Releases.AddAsync(release);
                    await _context.SaveChangesAsync();
                    return await GetRelease(release.Id);
                });
        }

        public Task<Either<ActionResult, DeleteReleasePlan>> GetDeleteReleasePlan(Guid releaseId)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanDeleteRelease)
                .OnSuccess(_ =>
                {
                    var methodologiesScheduledWithRelease =
                        GetMethodologiesScheduledWithRelease(releaseId)
                        .Select(m => new IdTitleViewModel(m.Id, m.Title))
                        .ToList();

                    return new DeleteReleasePlan
                    {
                        ScheduledMethodologies = methodologiesScheduledWithRelease
                    };
                });
        }

        public Task<Either<ActionResult, Unit>> DeleteRelease(Guid releaseId)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanDeleteRelease)
                .OnSuccessDo(async release => await _cacheService.DeleteCacheFolder(
                    new PrivateReleaseContentFolderCacheKey(release.Id)))
                .OnSuccessDo(async () => await _releaseDataFileService.DeleteAll(releaseId))
                .OnSuccessDo(async () => await _releaseFileService.DeleteAll(releaseId))
                .OnSuccessVoid(async release =>
                {
                    release.SoftDeleted = true;
                    _context.Update(release);

                    var roles = await _context
                        .UserReleaseRoles
                        .AsQueryable()
                        .Where(r => r.ReleaseId == releaseId)
                        .ToListAsync();
                    roles.ForEach(r => r.SoftDeleted = true);
                    _context.UpdateRange(roles);

                    var invites = await _context
                        .UserReleaseInvites
                        .AsQueryable()
                        .Where(r => r.ReleaseId == releaseId)
                        .ToListAsync();
                    invites.ForEach(r => r.SoftDeleted = true);
                    _context.UpdateRange(invites);

                    var methodologiesScheduledWithRelease =
                        GetMethodologiesScheduledWithRelease(releaseId);

                    // TODO EES-2747 - this should be looked at to see how best to reuse similar "set to draft" logic
                    // in MethodologyApprovalService.
                    methodologiesScheduledWithRelease.ForEach(m =>
                    {
                        m.PublishingStrategy = Immediately;
                        m.Status = Draft;
                        m.ScheduledWithRelease = null;
                        m.ScheduledWithReleaseId = null;
                        m.InternalReleaseNote = null;
                        m.Updated = DateTime.UtcNow;
                    });

                    _context.UpdateRange(methodologiesScheduledWithRelease);

                    await _context.SaveChangesAsync();

                    await _releaseSubjectRepository.SoftDeleteAllReleaseSubjects(releaseId);
                });
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
                    .OnSuccess(amendment => GetRelease(amendment.Id)));
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

        private async Task<Either<ActionResult, Release>> CreateBasicReleaseAmendment(Release release)
        {
            var amendment = release.CreateAmendment(DateTime.UtcNow, _userService.GetUserId());
            await _context.Releases.AddAsync(amendment);
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

        public Task<Either<ActionResult, ReleasePublicationStatusViewModel>> GetReleasePublicationStatus(Guid releaseId)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(_mapper.Map<ReleasePublicationStatusViewModel>);
        }

        public async Task<Either<ActionResult, ReleaseViewModel>> UpdateRelease(
            Guid releaseId, ReleaseUpdateRequest request)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId, ReleaseChecklistService.HydrateReleaseForChecklist)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccessDo(async release => await ValidateReleaseSlugUniqueToPublication(request.Slug, release.PublicationId, releaseId))
                .OnSuccess(async release =>
                {
                    release.Slug = request.Slug;
                    release.Type = request.Type;
                    release.ReleaseName = request.Year.ToString();
                    release.TimePeriodCoverage = request.TimePeriodCoverage;
                    release.PreReleaseAccessList = request.PreReleaseAccessList;

                    _context.Releases.Update(release);
                    await _context.SaveChangesAsync();
                    return await GetRelease(releaseId);
                });
        }

        public async Task<Either<ActionResult, Unit>> UpdateReleasePublished(Guid releaseId,
            ReleasePublishedUpdateRequest request)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId,
                    queryable => queryable.Include(r => r.Publication))
                .OnSuccessDo(_userService.CheckIsBauUser)
                .OnSuccess<ActionResult, Release, Unit>(async release =>
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

                    _context.Releases.Update(release);
                    release.Published = newPublishedDate;
                    await _context.SaveChangesAsync();

                    // Update the cached release
                    await _releaseCacheService.UpdateRelease(
                        releaseId,
                        publicationSlug: release.Publication.Slug,
                        releaseSlug: release.Slug);

                    if (release.Publication.LatestPublishedReleaseId == releaseId)
                    {
                        // This is the latest published release so also update the latest cached release
                        // for the publication which is a separate cache entry
                        await _releaseCacheService.UpdateRelease(
                            releaseId,
                            publicationSlug: release.Publication.Slug);
                    }

                    return Unit.Instance;
                });
        }

        public async Task<Either<ActionResult, IdTitleViewModel>> GetLatestPublishedRelease(Guid publicationId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Publication>(publicationId, queryable =>
                    queryable.Include(r => r.LatestPublishedRelease))
                .OnSuccess(_userService.CheckCanViewPublication)
                .OnSuccess(publication =>
                {
                    var latestPublishedRelease = publication.LatestPublishedRelease;
                    return latestPublishedRelease != null ? new IdTitleViewModel
                    {
                        Id = latestPublishedRelease.Id,
                        Title = latestPublishedRelease.Title
                    } : new Either<ActionResult, IdTitleViewModel>(new NotFoundResult());
                });
        }

        public async Task<Either<ActionResult, List<ReleaseViewModel>>> ListReleasesWithStatuses(
            params ReleaseApprovalStatus[] releaseApprovalStatuses)
        {
            return await _userService
                .CheckCanAccessSystem()
                .OnSuccess(_ =>
                {
                    return _userService
                        .CheckCanViewAllReleases()
                        .OnSuccess(() => _repository.ListReleases(releaseApprovalStatuses))
                        .OrElse(() =>
                            _repository.ListReleasesForUser(_userService.GetUserId(),
                                releaseApprovalStatuses));
                })
                .OnSuccess(async releases =>
                {
                    return await releases
                        .ToAsyncEnumerable()
                        .SelectAwait(async release =>
                        {
                            var releaseViewModel = _mapper.Map<ReleaseViewModel>(release);
                            releaseViewModel.Permissions =
                                await PermissionsUtils.GetReleasePermissions(_userService, release);
                            return releaseViewModel;
                        }).ToListAsync();
                });
        }

        public async Task<Either<ActionResult, List<ReleaseViewModel>>> ListScheduledReleases()
        {
            return await _userService
                .CheckCanAccessSystem()
                .OnSuccess(_ =>
                {
                    return _userService
                        .CheckCanViewAllReleases()
                        .OnSuccess(() => _repository.ListReleases(ReleaseApprovalStatus.Approved))
                        .OrElse(() =>
                            _repository.ListReleasesForUser(_userService.GetUserId(),
                                ReleaseApprovalStatus.Approved));
                })
                .OnSuccess(async releases =>
                {
                    var approvedReleases = await releases
                        .ToAsyncEnumerable()
                        .SelectAwait(async release =>
                        {
                            var releaseViewModel = _mapper.Map<ReleaseViewModel>(release);
                            releaseViewModel.Permissions =
                                await PermissionsUtils.GetReleasePermissions(_userService, release);
                            return releaseViewModel;
                        }).ToListAsync();

                    return approvedReleases
                        .Where(release => !release.Live)
                        .ToList();
                });
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
            Guid publicationId, Guid? releaseId = null)
        {
            var releases = await _context.Releases
                .AsQueryable()
                .Where(r => r.PublicationId == publicationId).ToListAsync();

            if (releases.Any(release => release.Slug == slug && release.Id != releaseId && IsLatestVersionOfRelease(releases, release.Id)))
            {
                return ValidationActionResult(SlugNotUnique);
            }

            return Unit.Instance;
        }

        private static bool IsLatestVersionOfRelease(IEnumerable<Release> releases, Guid releaseId)
        {
            return !releases.Any(r => r.PreviousVersionId == releaseId && r.Id != releaseId);
        }

        private async Task CreateGenericContentFromTemplate(Guid releaseId, Release newRelease)
        {
            var templateRelease = await _context.Releases.AsNoTracking()
                .Include(r => r.Content)
                .ThenInclude(c => c.ContentSection)
                .FirstAsync(r => r.Id == releaseId);

            templateRelease.CreateGenericContentFromTemplate(newRelease);
        }

        public async Task<Either<ActionResult, DeleteDataFilePlan>> GetDeleteDataFilePlan(Guid releaseId, Guid fileId)
        {
            return await _persistenceHelper.CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(() => CheckFileExists(fileId))
                .OnSuccess(async file =>
                {
                    var subject = file.SubjectId.HasValue
                        ? await _subjectRepository.Find(file.SubjectId.Value)
                        : null;

                    var footnotes = subject == null
                        ? new List<Footnote>()
                        : await _footnoteRepository.GetFootnotes(releaseId, subject.Id);

                    return new DeleteDataFilePlan
                    {
                        ReleaseId = releaseId,
                        SubjectId = subject?.Id ?? Guid.Empty,
                        DeleteDataBlockPlan = await _dataBlockService.GetDeletePlan(releaseId, subject),
                        FootnoteIds = footnotes.Select(footnote => footnote.Id).ToList()
                    };
                });
        }

        public async Task<Either<ActionResult, Unit>> RemoveDataFiles(Guid releaseId, Guid fileId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(() => CheckFileExists(fileId))
                .OnSuccessDo(file => CheckCanDeleteDataFiles(releaseId, file))
                .OnSuccessDo(async file =>
                {
                    // Delete any replacement that might exist
                    if (file.ReplacedById.HasValue)
                    {
                        return await RemoveDataFiles(releaseId, file.ReplacedById.Value);
                    }
                    return Unit.Instance;
                })
                .OnSuccess(_ => GetDeleteDataFilePlan(releaseId, fileId))
                .OnSuccessDo(deletePlan => _dataBlockService.DeleteDataBlocks(deletePlan.DeleteDataBlockPlan))
                .OnSuccessVoid(async deletePlan =>
                {
                    await _releaseSubjectRepository.SoftDeleteReleaseSubject(releaseId, deletePlan.SubjectId);
                    await _cacheService.DeleteItem(new PrivateSubjectMetaCacheKey(releaseId, deletePlan.SubjectId));
                })
                .OnSuccess(() => _releaseDataFileService.Delete(releaseId, fileId));
        }

        public async Task<Either<ActionResult, DataImportStatusViewModel>> GetDataFileImportStatus(Guid releaseId, Guid fileId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(async _ =>
                {
                    // Ensure file is linked to the Release by getting the ReleaseFile first
                    var releaseFile = await _releaseFileRepository.Find(releaseId, fileId);
                    if (releaseFile == null || releaseFile.File.Type != FileType.Data)
                    {
                        return DataImportStatusViewModel.NotFound();
                    }

                    return await _dataImportService.GetImportStatus(fileId);
                });
        }

        private async Task<bool> CanUpdateDataFiles(Guid releaseId)
        {
            var release = await _context.Releases.FirstAsync(r => r.Id == releaseId);
            return release.ApprovalStatus != ReleaseApprovalStatus.Approved;
        }

        private async Task<Either<ActionResult, Unit>> CheckCanDeleteDataFiles(Guid releaseId, File file)
        {
            var import = await _dataImportService.GetImport(file.Id);
            var importStatus = import?.Status ?? DataImportStatus.NOT_FOUND;

            if (!importStatus.IsFinished())
            {
                return ValidationActionResult(CannotRemoveDataFilesUntilImportComplete);
            }

            if (!await CanUpdateDataFiles(releaseId))
            {
                return ValidationActionResult(CannotRemoveDataFilesOnceReleaseApproved);
            }

            return Unit.Instance;
        }

        public static IQueryable<Release> HydrateRelease(IQueryable<Release> values)
        {
            // Require publication / release / contact graph to be able to work out:
            // If the release is the latest
            // The contact
            return values.Include(r => r.Publication)
                .ThenInclude(publication => publication.Contact)
                .Include(r => r.ReleaseStatuses);
        }

        private static IQueryable<Release> HydrateReleaseForAmendment(IQueryable<Release> queryable)
        {
            return queryable
                .AsSplitQuery()
                .Include(r => r.Publication)
                .Include(r => r.Content)
                .ThenInclude(c => c.ContentSection)
                .ThenInclude(c => c.Content)
                .ThenInclude(cb => (cb as EmbedBlockLink)!.EmbedBlock)
                .Include(r => r.Updates)
                .Include(r => r.ContentBlocks)
                .ThenInclude(r => r.ContentBlock)
                .Include(r => r.KeyStatistics)
                .ThenInclude(ks => (ks as KeyStatisticDataBlock)!.DataBlock);
        }

        private IList<MethodologyVersion> GetMethodologiesScheduledWithRelease(Guid releaseId)
        {
            return _context
                .MethodologyVersions
                .Include(m => m.Methodology)
                .Where(m => releaseId == m.ScheduledWithReleaseId)
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
