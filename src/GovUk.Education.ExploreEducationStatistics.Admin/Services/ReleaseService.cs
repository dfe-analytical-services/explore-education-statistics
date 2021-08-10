using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using IReleaseRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseRepository;
using IReleaseService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseService;
using Publication = GovUk.Education.ExploreEducationStatistics.Content.Model.Publication;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ReleaseService : IReleaseService
    {
        private readonly ContentDbContext _context;
        private readonly StatisticsDbContext _statisticsDbContext;
        private readonly IPublishingService _publishingService;
        private readonly IMapper _mapper;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IUserService _userService;
        private readonly IReleaseRepository _repository;
        private readonly IReleaseFileRepository _releaseFileRepository;
        private readonly ISubjectRepository _subjectRepository;
        private readonly IReleaseDataFileService _releaseDataFileService;
        private readonly IReleaseFileService _releaseFileService;
        private readonly IDataImportService _dataImportService;
	    private readonly IFootnoteService _footnoteService;
        private readonly IDataBlockService _dataBlockService;
        private readonly IReleaseChecklistService _releaseChecklistService;
        private readonly IContentService _contentService;
        private readonly IReleaseSubjectRepository _releaseSubjectRepository;
        private readonly IGuidGenerator _guidGenerator;

        // TODO EES-212 - ReleaseService needs breaking into smaller services as it feels like it is now doing too
        // much work and has too many dependencies
        public ReleaseService(
            ContentDbContext context,
            IMapper mapper,
            IPublishingService publishingService,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IUserService userService,
            IReleaseRepository repository,
            IReleaseFileRepository releaseFileRepository,
            ISubjectRepository subjectRepository,
            IReleaseDataFileService releaseDataFileService,
            IReleaseFileService releaseFileService,
            IDataImportService dataImportService,
            IFootnoteService footnoteService,
            StatisticsDbContext statisticsDbContext,
            IDataBlockService dataBlockService,
            IReleaseChecklistService releaseChecklistService,
            IContentService contentService,
            IReleaseSubjectRepository releaseSubjectRepository,
            IGuidGenerator guidGenerator)
        {
            _context = context;
            _publishingService = publishingService;
            _mapper = mapper;
            _persistenceHelper = persistenceHelper;
            _userService = userService;
            _repository = repository;
            _releaseFileRepository = releaseFileRepository;
            _subjectRepository = subjectRepository;
            _releaseDataFileService = releaseDataFileService;
            _releaseFileService = releaseFileService;
            _dataImportService = dataImportService;
            _footnoteService = footnoteService;
            _statisticsDbContext = statisticsDbContext;
            _dataBlockService = dataBlockService;
            _releaseChecklistService = releaseChecklistService;
            _contentService = contentService;
            _releaseSubjectRepository = releaseSubjectRepository;
            _guidGenerator = guidGenerator;
        }

        public async Task<Either<ActionResult, ReleaseViewModel>> GetRelease(Guid id)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(id, HydrateReleaseForReleaseViewModel)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(release => _mapper.Map<ReleaseViewModel>(release));
        }

        public async Task<Either<ActionResult, List<ReleaseStatusViewModel>>> GetReleaseStatuses(
            Guid releaseId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId, release =>
                    release.Include(r => r.ReleaseStatuses)
                        .ThenInclude(rs => rs.CreatedBy))
                .OnSuccess(_userService.CheckCanViewReleaseStatusHistory)
                .OnSuccess(release =>
                    release.ReleaseStatuses
                        .Select(rs =>
                            new ReleaseStatusViewModel
                            {
                                ReleaseStatusId = rs.Id,
                                InternalReleaseNote = rs.InternalReleaseNote,
                                ApprovalStatus = rs.ApprovalStatus,
                                Created = rs.Created,
                                CreatedByEmail = rs.CreatedBy?.Email
                            })
                        .OrderByDescending(vm => vm.Created)
                        .ToList()
                );
        }

        public async Task<Either<ActionResult, ReleaseViewModel>> CreateReleaseAsync(ReleaseCreateViewModel releaseCreate)
        {
            return await _persistenceHelper
                .CheckEntityExists<Publication>(releaseCreate.PublicationId)
                .OnSuccess(_userService.CheckCanCreateReleaseForPublication)
                .OnSuccess(async _ => await ValidateReleaseSlugUniqueToPublication(releaseCreate.Slug, releaseCreate.PublicationId))
                .OnSuccess(async () =>
                {
                    var release = _mapper.Map<Release>(releaseCreate);

                    release.Id = _guidGenerator.NewGuid();

                    if (releaseCreate.TemplateReleaseId.HasValue)
                    {
                        CreateGenericContentFromTemplate(releaseCreate.TemplateReleaseId.Value, release);
                    }
                    else
                    {
                        release.GenericContent = new List<ContentSection>();
                    }

                    release.SummarySection = new ContentSection
                    {
                        Type = ContentSectionType.ReleaseSummary
                    };
                    release.KeyStatisticsSection = new ContentSection{
                        Type = ContentSectionType.KeyStatistics
                    };
                    release.KeyStatisticsSecondarySection = new ContentSection{
                        Type = ContentSectionType.KeyStatisticsSecondary
                    };
                    release.HeadlinesSection = new ContentSection{
                        Type = ContentSectionType.Headlines
                    };
                    release.Created = DateTime.UtcNow;
                    release.CreatedById = _userService.GetUserId();

                    await _context.Releases.AddAsync(release);
                    await _context.SaveChangesAsync();
                    return await GetRelease(release.Id);
                });
        }

        public Task<Either<ActionResult, Unit>> DeleteRelease(Guid releaseId)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanDeleteRelease)
                .OnSuccessDo(async () => await _releaseDataFileService.DeleteAll(releaseId))
                .OnSuccessDo(async () => await _releaseFileService.DeleteAll(releaseId))
                .OnSuccessVoid(async release =>
                {
                    var roles = await _context
                        .UserReleaseRoles
                        .Where(r => r.ReleaseId == releaseId)
                        .ToListAsync();

                    var invites = await _context
                        .UserReleaseInvites
                        .Where(r => r.ReleaseId == releaseId)
                        .ToListAsync();

                    release.SoftDeleted = true;
                    roles.ForEach(r => r.SoftDeleted = true);
                    invites.ForEach(r => r.SoftDeleted = true);

                    _context.Update(release);
                    _context.UpdateRange(roles);
                    _context.UpdateRange(invites);

                    await _context.SaveChangesAsync();

                    await _releaseSubjectRepository.SoftDeleteAllReleaseSubjects(releaseId);
                });
        }

        public async Task<Either<ActionResult, ReleaseViewModel>> CreateReleaseAmendmentAsync(Guid releaseId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(HydrateReleaseForAmendment)
                .OnSuccess(_userService.CheckCanMakeAmendmentOfRelease)
                .OnSuccess(originalRelease =>
                    CreateBasicReleaseAmendment(originalRelease)
                    .OnSuccess(CreateStatisticsReleaseAmendment)
                    .OnSuccess(amendment => CopyReleaseTeam(releaseId, amendment))
                    .OnSuccessDo(amendment => _footnoteService.CopyFootnotes(releaseId, amendment.Id))
                    .OnSuccess(amendment => CopyFileLinks(originalRelease, amendment))
                    .OnSuccess(amendment => GetRelease(amendment.Id)));
        }

        private async Task<Either<ActionResult, Release>> CreateStatisticsReleaseAmendment(Release amendment)
        {
            var statsRelease =_statisticsDbContext
                .Release
                .FirstOrDefault(r => r.Id == amendment.PreviousVersionId);

            // Release does not have to have stats uploaded but if it has then
            // create a link row to link back to the original subject
            if (statsRelease != null)
            {
                var statsAmendment = statsRelease.CreateReleaseAmendment(amendment.Id, amendment.PreviousVersionId);

                var statsAmendmentSubjectLinks = _statisticsDbContext
                    .ReleaseSubject
                    .Where(rs => rs.ReleaseId == amendment.PreviousVersionId)
                    .Select(rs => rs.CopyForRelease(statsAmendment));

                await _statisticsDbContext.Release.AddAsync(statsAmendment);
                await _statisticsDbContext.ReleaseSubject.AddRangeAsync(statsAmendmentSubjectLinks);

                await _statisticsDbContext.SaveChangesAsync();
            }

            return amendment;
        }

        private async Task<Either<ActionResult, Release>> CopyReleaseTeam(Guid originalReleaseId, Release amendment)
        {
            var newRoles = _context
                .UserReleaseRoles
                .Where(r => r.ReleaseId == originalReleaseId)
                .Select(r => r.CreateReleaseAmendment(amendment));

            await _context.AddRangeAsync(newRoles);
            await _context.SaveChangesAsync();
            return amendment;
        }

        private async Task<Either<ActionResult, Release>> CreateBasicReleaseAmendment(Release release)
        {
            var amendment = release.CreateReleaseAmendment(DateTime.UtcNow, _userService.GetUserId());
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

        public Task<Either<ActionResult, ReleasePublicationStatusViewModel>> GetReleasePublicationStatusAsync(Guid releaseId)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(_mapper.Map<ReleasePublicationStatusViewModel>);
        }

        public async Task<Either<ActionResult, ReleaseViewModel>> UpdateRelease(
            Guid releaseId, ReleaseUpdateViewModel request)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId, ReleaseChecklistService.HydrateReleaseForChecklist)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccessDo(async release => await ValidateReleaseSlugUniqueToPublication(request.Slug, release.PublicationId, releaseId))
                .OnSuccess(async release =>
                {
                    release.Slug = request.Slug;
                    release.TypeId = request.TypeId;
                    release.ReleaseName = request.ReleaseName;
                    release.TimePeriodCoverage = request.TimePeriodCoverage;
                    release.PreReleaseAccessList = request.PreReleaseAccessList;
                    
                    _context.Releases.Update(release);
                    await _context.SaveChangesAsync();
                    return await GetRelease(releaseId);
                });
        }

        public async Task<Either<ActionResult, ReleaseViewModel>> CreateReleaseStatus(
            Guid releaseId, ReleaseStatusCreateViewModel request)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId, ReleaseChecklistService.HydrateReleaseForChecklist)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccessDo(release => _userService.CheckCanUpdateReleaseStatus(release, request.ApprovalStatus))
                .OnSuccess(async release =>
                {
                    if (request.ApprovalStatus != ReleaseApprovalStatus.Approved && release.Published.HasValue)
                    {
                        return ValidationActionResult(PublishedReleaseCannotBeUnapproved);
                    }

                    if (request.ApprovalStatus == ReleaseApprovalStatus.Approved
                        && request.PublishMethod == PublishMethod.Scheduled
                        && !request.PublishScheduledDate.HasValue)
                    {
                        return ValidationActionResult(ApprovedReleaseMustHavePublishScheduledDate);
                    }

                    var oldStatus = release.ApprovalStatus;

                    release.ApprovalStatus = request.ApprovalStatus;
                    release.NextReleaseDate = request.NextReleaseDate;
                    release.PublishScheduled = request.PublishMethod == PublishMethod.Immediate &&
                                               request.ApprovalStatus == ReleaseApprovalStatus.Approved
                        ? DateTime.UtcNow
                        : request.PublishScheduledDate;

                    var releaseStatus = new ReleaseStatus
                    {
                        Release = release,
                        InternalReleaseNote = request.LatestInternalReleaseNote,
                        ApprovalStatus = request.ApprovalStatus,
                        Created = DateTime.UtcNow,
                        CreatedById = _userService.GetUserId()
                    };

                    return await ValidateReleaseWithChecklist(release)
                        .OnSuccessDo(() => RemoveUnusedImages(release.Id))
                        .OnSuccess(async () =>
                        {
                            _context.Releases.Update(release);
                            await _context.AddAsync(releaseStatus);
                            await _context.SaveChangesAsync();

                            // Only need to inform Publisher if changing release approval status to or from Approved
                            if (oldStatus == ReleaseApprovalStatus.Approved || request.ApprovalStatus == ReleaseApprovalStatus.Approved)
                            {
                                await _publishingService.ReleaseChanged(
                                    releaseId,
                                    releaseStatus.Id,
                                    request.PublishMethod == PublishMethod.Immediate
                                );
                            }

                            _context.Update(release);
                            await _context.SaveChangesAsync();

                            return await GetRelease(releaseId);
                        });
                });
        }

        private async Task<Either<ActionResult, Unit>> ValidateReleaseWithChecklist(Release release)
        {
            if (release.ApprovalStatus != ReleaseApprovalStatus.Approved)
            {
                return Unit.Instance;
            }

            var errors = (await _releaseChecklistService.GetErrors(release))
                .Select(error => error.Code)
                .ToList();

            if (!errors.Any())
            {
                return Unit.Instance;
            }

            return ValidationActionResult(errors);
        }

        public async Task<Either<ActionResult, TitleAndIdViewModel>> GetLatestReleaseAsync(Guid publicationId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Publication>(publicationId, queryable =>
                    queryable.Include(r => r.Releases))
                .OnSuccess(_userService.CheckCanViewPublication)
                .OnSuccess(publication =>
                {
                    var latestRelease = publication.LatestPublishedRelease();
                    return latestRelease != null ? new TitleAndIdViewModel
                    {
                        Id = latestRelease.Id,
                        Title = latestRelease.Title
                    } : null;
                });
        }

        public async Task<Either<ActionResult, List<MyReleaseViewModel>>> GetMyReleasesForReleaseStatusesAsync(
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
                });
        }

        public async Task<Either<ActionResult, List<MyReleaseViewModel>>> GetMyScheduledReleasesAsync()
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
                .OnSuccess(approvedReleases =>
                {
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
            var releases = await _context.Releases.Where(r => r.PublicationId == publicationId).ToListAsync();

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

        private void CreateGenericContentFromTemplate(Guid releaseId, Release newRelease)
        {
            var templateRelease = _context.Releases.AsNoTracking()
                .Include(r => r.Content)
                .ThenInclude(c => c.ContentSection)
                .First(r => r.Id == releaseId);

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
                        ? await _subjectRepository.Get(file.SubjectId.Value)
                        : null;

                    var footnotes = subject == null
                        ? new List<Footnote>()
                        : _footnoteService.GetFootnotes(releaseId, subject.Id);

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
                .OnSuccess(file =>
                {
                    return CheckCanDeleteDataFiles(releaseId, file)
                        .OnSuccessDo(async _ =>
                        {
                            // Delete any replacement that might exist
                            if (file.ReplacedById.HasValue)
                            {
                                return await RemoveDataFiles(releaseId, file.ReplacedById.Value);
                            }
                            return Unit.Instance;
                        })
                        .OnSuccess(_ => GetDeleteDataFilePlan(releaseId, fileId))
                        .OnSuccess(async deletePlan =>
                        {
                            await _dataBlockService.DeleteDataBlocks(deletePlan.DeleteDataBlockPlan);
                            await _releaseSubjectRepository.SoftDeleteReleaseSubject(releaseId,
                                deletePlan.SubjectId);

                            return await _releaseDataFileService.Delete(releaseId, fileId);
                        });
                });
        }

        private async Task<Either<ActionResult, Unit>> RemoveUnusedImages(Guid releaseId)
        {
            return await _contentService.GetContentBlocks<HtmlBlock>(releaseId)
                .OnSuccess(async contentBlocks =>
                {
                    var contentImageIds = contentBlocks.SelectMany(contentBlock =>
                            HtmlImageUtil.GetReleaseImages(contentBlock.Body))
                        .Distinct();

                    var imageFiles = await _releaseFileRepository.GetByFileType(releaseId, Image);

                    var unusedImages = imageFiles
                        .Where(file => !contentImageIds.Contains(file.File.Id))
                        .Select(file => file.File.Id)
                        .ToList();

                    if (unusedImages.Any())
                    {
                        return await _releaseFileService.Delete(releaseId, unusedImages);
                    }

                    return Unit.Instance;
                });
        }

        public async Task<Either<ActionResult, DataImportViewModel>> GetDataFileImportStatus(Guid releaseId, Guid fileId)
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
                        return DataImportViewModel.NotFound();
                    }

                    return await _dataImportService.GetImport(fileId);
                });
        }

        private bool CanUpdateDataFiles(Guid releaseId)
        {
            var release = _context.Releases.First(r => r.Id == releaseId);
            return release.ApprovalStatus != ReleaseApprovalStatus.Approved;
        }

        private async Task<Either<ActionResult, Unit>> CheckCanDeleteDataFiles(Guid releaseId, File file)
        {
            var importStatus = await _dataImportService.GetStatus(file.Id);

            if (!importStatus.IsFinished())
            {
                return ValidationActionResult(CannotRemoveDataFilesUntilImportComplete);
            }

            if (!CanUpdateDataFiles(releaseId))
            {
                return ValidationActionResult(CannotRemoveDataFilesOnceReleaseApproved);
            }

            return Unit.Instance;
        }

        public static IQueryable<Release> HydrateReleaseForReleaseViewModel(IQueryable<Release> values)
        {
            // Require publication / release / contact / type graph to be able to work out:
            // If the release is the latest
            // The contact
            // The type
            return values.Include(r => r.Publication)
                .ThenInclude(publication => publication.Releases) // Back refs required to work out latest
                .Include(r => r.Publication)
                .ThenInclude(publication => publication.Contact)
                .Include(r => r.Type)
                .Include(r => r.ReleaseStatuses);
        }

        private async Task<Release> HydrateReleaseForAmendment(Release release)
        {
            // Require publication / release / contact / type graph to be able to work out:
            // If the release is the latest
            // The contact
            // The type
            await _context.Entry(release)
                .Reference(r => r.Publication)
                .LoadAsync();
            await _context.Entry(release)
                .Collection(r => r.Content)
                .LoadAsync();
            await release.Content.ForEachAsync(async cs =>
            {
                await _context.Entry(cs)
                    .Reference(rcs => rcs.ContentSection)
                    .LoadAsync();
                await _context.Entry(cs.ContentSection)
                    .Collection(s => s.Content)
                    .LoadAsync();
            });
            await _context.Entry(release)
                .Collection(r => r.Updates)
                .LoadAsync();
            await _context.Entry(release)
                .Collection(r => r.ContentBlocks)
                .LoadAsync();
            await release.ContentBlocks.ForEachAsync(async rcb =>
                await _context.Entry(rcb)
                    .Reference(cb => cb.ContentBlock)
                    .LoadAsync()
            );
            return release;
        }
    }

    public class DeleteDataFilePlan
    {
        [JsonIgnore]
        public Guid ReleaseId { get; set; }

        [JsonIgnore]
        public Guid SubjectId { get; set; }

        public DeleteDataBlockPlan DeleteDataBlockPlan { get; set; }

        public List<Guid> FootnoteIds { get; set; }
    }
}
