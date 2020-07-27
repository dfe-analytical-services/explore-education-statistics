using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using IFootnoteService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IFootnoteService;
using IReleaseService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseService;
using Publication = GovUk.Education.ExploreEducationStatistics.Content.Model.Publication;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

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
        private readonly ISubjectService _subjectService;
        private readonly ITableStorageService _coreTableStorageService;
        private readonly IFileStorageService _fileStorageService;
        private readonly IImportStatusService _importStatusService;
	    private readonly IFootnoteService _footnoteService;
        private readonly IDataBlockService _dataBlockService;
        private readonly IReleaseSubjectService _releaseSubjectService;
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
            ISubjectService subjectService,
            ITableStorageService coreTableStorageService,
            IFileStorageService fileStorageService,
            IImportStatusService importStatusService,
            IFootnoteService footnoteService,
            StatisticsDbContext statisticsDbContext,
            IDataBlockService dataBlockService,
            IReleaseSubjectService releaseSubjectService,
            IGuidGenerator guidGenerator)
        {
            _context = context;
            _publishingService = publishingService;
            _mapper = mapper;
            _persistenceHelper = persistenceHelper;
            _userService = userService;
            _repository = repository;
            _subjectService = subjectService;
            _coreTableStorageService = coreTableStorageService;
            _fileStorageService = fileStorageService;
            _importStatusService = importStatusService;
            _footnoteService = footnoteService;
            _statisticsDbContext = statisticsDbContext;
            _dataBlockService = dataBlockService;
            _releaseSubjectService = releaseSubjectService;
            _guidGenerator = guidGenerator;
        }

        public async Task<Either<ActionResult, ReleaseViewModel>> GetReleaseForIdAsync(Guid id)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(id, HydrateReleaseForReleaseViewModel)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(release => _mapper.Map<ReleaseViewModel>(release));
        }
        
        public async Task<Either<ActionResult, ReleaseViewModel>> CreateReleaseAsync(CreateReleaseViewModel createRelease)
        {
            return await _persistenceHelper
                .CheckEntityExists<Publication>(createRelease.PublicationId)
                .OnSuccess(_userService.CheckCanCreateReleaseForPublication)
                .OnSuccess(async _ => await ValidateReleaseSlugUniqueToPublication(createRelease.Slug, createRelease.PublicationId))
                .OnSuccess(async () =>
                {
                    var release = _mapper.Map<Release>(createRelease);
                    
                    release.Id = _guidGenerator.NewGuid();
                    
                    if (createRelease.TemplateReleaseId.HasValue)
                    {
                        CreateGenericContentFromTemplate(createRelease.TemplateReleaseId.Value, release);
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
                    release.PreviousVersionId = release.Id;
                    
                    _context.Releases.Add(release);
                    await _context.SaveChangesAsync();
                    return await GetReleaseForIdAsync(release.Id);
                });
        }

        public Task<Either<ActionResult, bool>> DeleteReleaseAsync(Guid releaseId)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanDeleteRelease)
                .OnSuccess(async release =>
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

                    return true;
                });
        }

        public async Task<Either<ActionResult, ReleaseViewModel>> CreateReleaseAmendmentAsync(Guid releaseId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId, HydrateReleaseForAmendment)
                .OnSuccess(_userService.CheckCanMakeAmendmentOfRelease)
                .OnSuccess(originalRelease =>
                    CreateBasicReleaseAmendment(originalRelease)
                    .OnSuccess(CreateStatisticsReleaseRecord)
                    .OnSuccess(amendment => CopyReleaseTeam(releaseId, amendment))
                    .OnSuccess(amendment => CopyFileLinks(originalRelease, amendment))
                    .OnSuccess(amendment => GetReleaseForIdAsync(amendment.Id)));
        }

        private async Task<Either<ActionResult, Release>> CreateStatisticsReleaseRecord(Release amendment)
        {
            var statsRelease =_statisticsDbContext
                .Release
                .FirstOrDefault(r => r.Id == amendment.PreviousVersionId);

            // Release does not have to have stats uploaded but if it has then
            // create a link row to link back to the original subject & footnotes
            if (statsRelease != null)
            {
                var statsAmendment = statsRelease.CreateReleaseAmendment(amendment.Id, amendment.PreviousVersionId);

                var statsAmendmentSubjectLinks =_statisticsDbContext
                    .ReleaseSubject
                    .Where(rs => rs.ReleaseId == amendment.PreviousVersionId)
                    .Select(rs => new ReleaseSubject
                    {
                        ReleaseId = statsAmendment.Id,
                        SubjectId = rs.SubjectId
                    });
  
                var statsAmendmentFootnoteLinks =_statisticsDbContext
                    .ReleaseFootnote
                    .Where(rf => rf.ReleaseId == amendment.PreviousVersionId)
                    .Select(rf => new ReleaseFootnote
                    {
                        ReleaseId = statsAmendment.Id,
                        FootnoteId = rf.FootnoteId
                    });

                _statisticsDbContext.Release.Add(statsAmendment);
                _statisticsDbContext.ReleaseSubject.AddRange(statsAmendmentSubjectLinks);
                _statisticsDbContext.ReleaseFootnote.AddRange(statsAmendmentFootnoteLinks);
                
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
            _context.Releases.Add(amendment);
            await _context.SaveChangesAsync();
            return amendment;
        }

        private async Task<Either<ActionResult, Release>> CopyFileLinks(Release originalRelease, Release newRelease)
        {
            var releaseFileCopies = _context
                .ReleaseFiles
                .Include(f => f.ReleaseFileReference)
                .Where(f => f.ReleaseId == originalRelease.Id)
                .Select(f => f.CreateReleaseAmendment(newRelease)).ToList();

            await _context.ReleaseFiles.AddRangeAsync(releaseFileCopies);
            await _context.SaveChangesAsync();
            return newRelease;
        }

        public Task<Either<ActionResult, ReleaseSummaryViewModel>> GetReleaseSummaryAsync(Guid releaseId)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId, 
                    releases => releases.Include(r => r.Type)
                )
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(_mapper.Map<ReleaseSummaryViewModel>);
        }

        public Task<Either<ActionResult, ReleasePublicationStatusViewModel>> GetReleasePublicationStatusAsync(Guid releaseId)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(_mapper.Map<ReleasePublicationStatusViewModel>);
        }

        public async Task<Either<ActionResult, ReleaseViewModel>> UpdateRelease(
            Guid releaseId, UpdateReleaseRequest request)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(release => _userService.CheckCanUpdateReleaseStatus(release, request.Status))
                .OnSuccessDo(async release => await ValidateReleaseSlugUniqueToPublication(request.Slug, release.PublicationId, releaseId))
                .OnSuccessDo(async release => await CheckAllDataFilesUploaded(release, request.Status))
                .OnSuccessDo(async release => await CheckMethodologyHasBeenApproved(release, request.Status))
                .OnSuccess(async release =>
                {
                    if (request.Status != ReleaseStatus.Approved && release.Published.HasValue)
                    {
                        return ValidationActionResult(PublishedReleaseCannotBeUnapproved);
                    }

                    if (request.Status == ReleaseStatus.Approved &&
                        request.PublishMethod == PublishMethod.Scheduled &&
                        !request.PublishScheduledDate.HasValue)
                    {
                        return ValidationActionResult(ApprovedReleaseMustHavePublishScheduledDate);
                    }

                    release.Slug = request.Slug;
                    release.TypeId = request.TypeId;
                    release.ReleaseName = request.ReleaseName;
                    release.TimePeriodCoverage = request.TimePeriodCoverage;

                    var oldStatus = release.Status;

                    release.Status = request.Status;
                    release.InternalReleaseNote = request.InternalReleaseNote;
                    release.NextReleaseDate = request.NextReleaseDate;

                    release.PublishScheduled = request.PublishMethod == PublishMethod.Immediate && 
                        request.Status == ReleaseStatus.Approved
                        ? DateTime.UtcNow
                        : request.PublishScheduledDate;

                    _context.Releases.Update(release);
                    await _context.SaveChangesAsync();

                    // Only need to inform Publisher if changing release status to or from Approved
                    if (oldStatus == ReleaseStatus.Approved || request.Status == ReleaseStatus.Approved)
                    {
                        await _publishingService.NotifyChange(releaseId, request.PublishMethod == PublishMethod.Immediate);
                    }
                    
                    _context.Update(release);
                    await _context.SaveChangesAsync();

                    return await GetReleaseForIdAsync(releaseId);
                });
        }

        public async Task<Either<ActionResult, TitleAndIdViewModel>> GetLatestReleaseAsync(Guid publicationId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Publication>(publicationId, queryable => 
                    queryable.Include(r => r.Releases))
                .OnSuccess(_userService.CheckCanViewPublication)
                .OnSuccess(publication =>
                {
                    var latestRelease = publication.LatestRelease();
                    return latestRelease != null ? new TitleAndIdViewModel
                    {
                        Id = latestRelease.Id,
                        Title = latestRelease.Title
                    } : null;
                });
        }
        
        public async Task<Either<ActionResult, List<MyReleaseViewModel>>> GetMyReleasesForReleaseStatusesAsync(
            params ReleaseStatus[] releaseStatuses)
        {
            return await _userService
                .CheckCanAccessSystem()
                .OnSuccess(_ =>
                {
                    return _userService
                        .CheckCanViewAllReleases()
                        .OnSuccess(() => _repository.GetAllReleasesForReleaseStatusesAsync(releaseStatuses))
                        .OrElse(() =>
                            _repository.GetReleasesForReleaseStatusRelatedToUserAsync(_userService.GetUserId(),
                                releaseStatuses));
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
                        .OnSuccess(() => _repository.GetAllReleasesForReleaseStatusesAsync(ReleaseStatus.Approved))
                        .OrElse(() =>
                            _repository.GetReleasesForReleaseStatusRelatedToUserAsync(_userService.GetUserId(),
                                ReleaseStatus.Approved));
                })
                .OnSuccess(approvedReleases =>
                {
                    return approvedReleases
                        .Where(release => !release.Live)
                        .ToList();
                });
        }

        public IEnumerable<Guid> GetReferencedReleaseFileVersions(Guid releaseId, params ReleaseFileTypes[] types)
        {
            return _context
                .ReleaseFiles
                .Include(rf => rf.ReleaseFileReference)
                .Where(rf => rf.ReleaseId == releaseId)
                .Select(rf => rf.ReleaseFileReference)
                .Where(rfr => types.Contains(rfr.ReleaseFileType))
                .Select(rfr => rfr.ReleaseId).Distinct();
        }

        private async Task<Either<ActionResult, bool>> ValidateReleaseSlugUniqueToPublication(string slug,
            Guid publicationId, Guid? releaseId = null)
        {
            var releases = await _context.Releases.Where(r => r.PublicationId == publicationId).ToListAsync();
             
            if (releases.Any(release => release.Slug == slug && release.Id != releaseId && IsLatestVersionOfRelease(releases, release.Id)))
            {
                return ValidationActionResult(SlugNotUnique);
            }

            return true;
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

        public async Task<Either<ActionResult, DeleteDataFilePlan>> GetDeleteDataFilePlan(Guid releaseId,
            string dataFileName, string subjectTitle)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async _ => 
                {
                    var subject = await _subjectService.GetAsync(releaseId, subjectTitle);
                    var footnotes = subject == null ? new List<Footnote>() : _footnoteService.GetFootnotes(releaseId, subject.Id);
                    
                    return new DeleteDataFilePlan
                    {
                        ReleaseId = releaseId,
                        SubjectId = subject?.Id ?? Guid.Empty,
                        TableStorageItem = new DatafileImport(releaseId.ToString(), dataFileName),
                        DeleteDataBlockPlan = _dataBlockService.GetDeleteDataBlockPlan(releaseId, subject),
                        FootnoteIds = footnotes.Select(footnote => footnote.Id).ToList(),
                    };
                });
        }

        public async Task<Either<ActionResult, bool>> RemoveDataFileReleaseLinkAsync(Guid releaseId, string fileName, string subjectTitle)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(() => CheckCanDeleteDataFiles(releaseId, fileName))
                .OnSuccess(_ => GetDeleteDataFilePlan(releaseId, fileName, subjectTitle))
                .OnSuccess(async deletePlan =>
                {
                    await _dataBlockService.DeleteDataBlocks(deletePlan.DeleteDataBlockPlan);
                    await _releaseSubjectService.SoftDeleteSubjectOrBreakReleaseLink(releaseId, deletePlan.SubjectId);

                    await _fileStorageService
                        .RemoveDataFileReleaseLinkAsync(releaseId, fileName)
                        .OnSuccess(async () => await RemoveFileImportEntryIfOrphaned(deletePlan));
                    return true;
                });
        }

        private async Task<bool> RemoveFileImportEntryIfOrphaned(DeleteDataFilePlan deletePlan)
        {
            if (await _subjectService.GetAsync(deletePlan.SubjectId) == null)
            {
                return await _coreTableStorageService.DeleteEntityAsync("imports", deletePlan.TableStorageItem);
            }

            return false;
        }

        public async Task<Either<ActionResult, ImportStatus>> GetDataFileImportStatus(Guid releaseId, string dataFileName)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(async _ =>
                {
                    var fileLink = _context
                            .ReleaseFiles
                            .Include(f => f.ReleaseFileReference)
                            .First(f => f.ReleaseId == releaseId && f.ReleaseFileReference.Filename == dataFileName);

                    if (fileLink == null)
                    {
                        return new ImportStatus
                        {
                            Status = IStatus.NOT_FOUND.GetEnumValue()
                        };
                    }
                    
                    var fileReference = fileLink.ReleaseFileReference;

                    return await _importStatusService.GetImportStatus(fileReference.ReleaseId.ToString(), dataFileName);
                });
        }

        private bool CanUpdateDataFiles(Guid releaseId)
        {
            var release = _context.Releases.First(r => r.Id == releaseId);
            return release.Status != ReleaseStatus.Approved;
        }

        private async Task<Either<ActionResult, bool>> CheckCanDeleteDataFiles(Guid releaseId, string dataFileName)
        {
            var releaseFileReference = _context
                .ReleaseFiles
                .Include(rf => rf.ReleaseFileReference)
                .Where(rf => rf.ReleaseId == releaseId && rf.ReleaseFileReference.Filename == dataFileName)
                .Select(rf => rf.ReleaseFileReference)
                .First();
                
            var importFinished = await _importStatusService.IsImportFinished(releaseFileReference.ReleaseId.ToString(), dataFileName);
            
            if (!importFinished)
            {
                return ValidationActionResult(CannotRemoveDataFilesUntilImportComplete);
            }

            if (!CanUpdateDataFiles(releaseId))
            {
                return ValidationActionResult(CannotRemoveDataFilesOnceReleaseApproved);
            }

            return true;
        }

        private async Task<Either<ActionResult, bool>> CheckMethodologyHasBeenApproved(Release release, ReleaseStatus status)
        {
            if (status == ReleaseStatus.Approved)
            {
                var publication = await _context.Publications
                    .Include(publication => publication.Methodology)
                    .FirstAsync(publication => publication.Id == release.PublicationId);

                if (publication.Methodology != null && publication.Methodology.Status != MethodologyStatus.Approved)
                {
                    return ValidationActionResult(MethodologyMustBeApprovedOrPublished);
                }
            }

            return true;
        }
        
        private async Task<Either<ActionResult, bool>> CheckAllDataFilesUploaded(Release release, ReleaseStatus status)
        {
            if (status == ReleaseStatus.Approved)
            {
                var filters = TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, release.Id.ToString()), 
                    TableOperators.And, 
                    TableQuery.GenerateFilterCondition("Status", QueryComparisons.NotEqual, IStatus.COMPLETE.ToString())
                );
                
                var query = new TableQuery<DatafileImport>().Where(filters);
                var cloudTable = await _coreTableStorageService.GetTableAsync("imports");
                var results = await cloudTable.ExecuteQuerySegmentedAsync(query, null);
                
                if (results.Results.Count != 0)
                {
                    return ValidationActionResult(AllDatafilesUploadedMustBeComplete);
                }
            }

            return true;
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
                .Include(r => r.Type);
        }
        
        private static IQueryable<Release> HydrateReleaseForAmendment(IQueryable<Release> values)
        {
            // Require publication / release / contact / type graph to be able to work out:
            // If the release is the latest
            // The contact
            // The type
            return values.Include(r => r.Publication)
                .Include(r => r.Content)
                .ThenInclude(c => c.ContentSection)
                .ThenInclude(c => c.Content)
                .Include(r => r.Updates)
                .Include(r => r.ContentBlocks)
                .ThenInclude(r => r.ContentBlock);
        }
    }

    public class DeleteDataFilePlan
    {
        [JsonIgnore]
        public Guid ReleaseId { get; set; }
        
        [JsonIgnore]
        public Guid SubjectId { get; set; }
        
        [JsonIgnore]
        public DatafileImport TableStorageItem { get; set; }
        
        public DeleteDataBlockPlan DeleteDataBlockPlan { get; set; }
        
        public List<Guid> FootnoteIds { get; set; }
    }
}