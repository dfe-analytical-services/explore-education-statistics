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
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using FileInfo = GovUk.Education.ExploreEducationStatistics.Admin.Models.FileInfo;
using IFootnoteService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IFootnoteService;
using IReleaseService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseService;
using Publication = GovUk.Education.ExploreEducationStatistics.Content.Model.Publication;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ReleaseService : IReleaseService
    {
        private readonly ContentDbContext _context;
        private readonly IPublishingService _publishingService;
        private readonly IMapper _mapper;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IUserService _userService;
        private readonly IReleaseRepository _repository;
        private readonly ISubjectService _subjectService;
        private readonly ITableStorageService _tableStorageService;
        private readonly IFileStorageService _fileStorageService;
        private readonly IImportStatusService _importStatusService;
	private readonly IFootnoteService _footnoteService;

        // TODO PP-318 - ReleaseService needs breaking into smaller services as it feels like it is now doing too
        // much work and has too many dependencies
        public ReleaseService(
            ContentDbContext context, 
            IMapper mapper, 
            IPublishingService publishingService,
            IPersistenceHelper<ContentDbContext> persistenceHelper, 
            IUserService userService, 
            IReleaseRepository repository, 
            ISubjectService subjectService,
            ITableStorageService tableStorageService, 
            IFileStorageService fileStorageService, 
            IImportStatusService importStatusService,
	    IFootnoteService footnoteService)
        {
            _context = context;
            _publishingService = publishingService;
            _mapper = mapper;
            _persistenceHelper = persistenceHelper;
            _userService = userService;
            _repository = repository;
            _subjectService = subjectService;
            _tableStorageService = tableStorageService;
            _fileStorageService = fileStorageService;
            _importStatusService = importStatusService;
            _footnoteService = footnoteService;
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
                .OnSuccess(_ => ValidateReleaseSlugUniqueToPublication(createRelease.Slug, createRelease.PublicationId))
                .OnSuccess(async () =>
                {
                    var release = _mapper.Map<Release>(createRelease);
                    
                    release.GenericContent = await TemplateFromRelease(createRelease.TemplateReleaseId);
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
                    
                    var saved =_context.Releases.Add(release);
                    await _context.SaveChangesAsync();
                    return await GetReleaseForIdAsync(saved.Entity.Id);
                });
        }

        public async Task<Either<ActionResult, ReleaseViewModel>> CreateReleaseAmendmentAsync(Guid releaseId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId, HydrateReleaseForAmendment)
                // TODO DW - correct permission check
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(originalRelease =>
                    CreateBasicReleaseAmendment(originalRelease)
                    .OnSuccess(amendment => CopyReleaseTeam(releaseId, amendment))
                    .OnSuccess(amendment => CopyReleaseFilesOfType(releaseId, amendment, ReleaseFileTypes.Ancillary))
                    .OnSuccess(amendment => CopyReleaseFilesOfType(releaseId, amendment, ReleaseFileTypes.Chart))
                    .OnSuccess(amendment => CopyDataFileLinks(originalRelease, amendment))
                    .OnSuccess(amendment => GetReleaseForIdAsync(amendment.Id)));
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

        private async Task<Either<ActionResult, Release>> CopyDataFileLinks(Release originalRelease, Release newRelease)
        {
            var releaseFileCopies = _context
                .ReleaseFiles
                .Include(f => f.ReleaseFileReference)
                .Where(f => f.ReleaseId == originalRelease.Id)
                .Select(f => f.CreateReleaseAmendment(newRelease));

            await _context.AddRangeAsync(releaseFileCopies);
            await _context.SaveChangesAsync();
            return newRelease;
        }

        private Task<Either<ActionResult, Release>> CopyReleaseFilesOfType(
            Guid originalReleaseId, Release newRelease, ReleaseFileTypes type)
        {
            return _fileStorageService
                .CopyReleaseFilesAsync(originalReleaseId, newRelease.Id, type);
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

        public async Task<Either<ActionResult, ReleaseViewModel>> EditReleaseSummaryAsync(
            Guid releaseId, UpdateReleaseSummaryRequest request)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(_ => ValidateReleaseSlugUniqueToPublication(request.Slug, releaseId, releaseId))
                .OnSuccess(async () =>
                {
                    var release = await _context.Releases
                        .Where(r => r.Id == releaseId)
                        .FirstOrDefaultAsync();

                    release.Slug = request.Slug;
                    release.TypeId = request.TypeId;
                    release.PublishScheduled = request.PublishScheduled;
                    release.ReleaseName = request.ReleaseName;
                    release.NextReleaseDate = request.NextReleaseDate;
                    release.TimePeriodCoverage = request.TimePeriodCoverage;
                    
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
        
        public async Task<Either<ActionResult, List<ReleaseViewModel>>> GetMyReleasesForReleaseStatusesAsync(
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
        
        private async Task<Either<ActionResult, bool>> ValidateReleaseSlugUniqueToPublication(string slug,
            Guid publicationId, Guid? releaseId = null)
        {
            if (await _context.Releases.AnyAsync(r => r.Slug == slug && r.PublicationId == publicationId && r.Id != releaseId))
            {
                return ValidationActionResult(SlugNotUnique);
            }

            return true;
        }

        private async Task<List<ContentSection>> GetContentAsync(Guid id)
        {
            return await _context
                .ReleaseContentSections
                .Include(join => join.ContentSection)
                .ThenInclude(section => section.Content)
                .Where(join => join.ReleaseId == id)
                .Select(join => join.ContentSection)
                .ToListAsync();
        }

        private async Task<List<ContentSection>> TemplateFromRelease(Guid? releaseId)
        {
            if (releaseId.HasValue)
            {
                var templateContent = await GetContentAsync(releaseId.Value);
                if (templateContent != null)
                {
                    return templateContent.Select(c => new ContentSection
                    {
                        Id = new Guid(),
                        Caption = c.Caption,
                        Heading = c.Heading,
                        Order = c.Order,
                        // TODO in future do we want to copy across more? Is it possible to do so?
                    }).ToList();
                }
            }

            return new List<ContentSection>();
        }

        public Task<Either<ActionResult, ReleaseSummaryViewModel>> UpdateReleaseStatusAsync(
            Guid releaseId, ReleaseStatus status, string internalReleaseNote)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(release => _userService.CheckCanUpdateReleaseStatus(release, status))
                .OnSuccess(async release => {

                    if (status == ReleaseStatus.Approved && !release.PublishScheduled.HasValue)
                    {
                        return ValidationActionResult(ApprovedReleaseMustHavePublishScheduledDate);
                    }
                    
                    release.Status = status;
                    release.InternalReleaseNote = internalReleaseNote;
                    _context.Releases.Update(release);
                    await _context.SaveChangesAsync();

                    await _publishingService.QueueReleaseStatusAsync(releaseId);

                    return await GetReleaseSummaryAsync(releaseId);
                });
        }

        public async Task<Either<ActionResult, DeleteDataFilePlan>> GetDeleteDataFilePlan(Guid releaseId,
            string dataFileName, string subjectTitle)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(_ => _footnoteService.GetFootnotesAsync(releaseId))
                .OnSuccess(async footnotes =>
                {
                    var subject = await _subjectService.GetAsync(releaseId, subjectTitle);
                    var dependentDataBlocks = subject == null ? new List<DataBlock>() : GetDependentDataBlocks(releaseId, subject.Id);
                    var orphanFootnotes = subject == null ? new List<Footnote>() : await _subjectService.GetFootnotesOnlyForSubjectAsync(subject.Id);
                    
                    return new DeleteDataFilePlan
                    {
                        ReleaseId = releaseId,
                        
                        SubjectId = subject?.Id ?? Guid.Empty,
                        
                        TableStorageItem = new DatafileImport(releaseId.ToString(), dataFileName, 0,0, null),
                        
                        DependentDataBlocks = dependentDataBlocks.
                            Select(block => new DependentDataBlock
                            {
                                Id = block.Id,
                                Name = block.Name,
                                ContentSectionHeading = GetContentSectionHeading(block),
                                InfographicFilenames = block
                                   .Charts
                                   .Where(chart => chart.Type == ChartType.infographic.ToString())
                                   .Cast<InfographicChart>()
                                   .Select(chart => chart.FileId)
                                   .ToList(),
                            })
                            .ToList(),
                        
                        FootnoteIds = orphanFootnotes
                           .Select(footnote => footnote.Id)
                           .ToList()
                     };
                });
        }

        private string GetContentSectionHeading(DataBlock block)
        {
            var section = block.ContentSection;

            if (section == null)
            {
                return null;
            }

            switch (block.ContentSection.Type)
            {
                case ContentSectionType.Generic: return section.Heading;
                case ContentSectionType.ReleaseSummary: return "Release Summary";
                case ContentSectionType.Headlines: return "Headlines";
                case ContentSectionType.KeyStatistics: return "Key Statistics";
                case ContentSectionType.KeyStatisticsSecondary: return "Key Statistics";
                default: return block.ContentSection.Type.ToString();
            }
        }

        public async Task<Either<ActionResult, IEnumerable<FileInfo>>> DeleteDataFilesAsync(Guid releaseId, string fileName, string subjectTitle)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(() => CheckCanDeleteDataFiles(releaseId, fileName))
                .OnSuccess(_ => GetDeleteDataFilePlan(releaseId, fileName, subjectTitle))
                .OnSuccess(async deletePlan =>
                {
                    await _subjectService.DeleteAsync(deletePlan.SubjectId);
                    await DeleteDependentDataBlocks(deletePlan);
                    await DeleteChartFiles(deletePlan);

                    return await _fileStorageService
                        .DeleteDataFileAsync(releaseId, fileName)
                        .OnSuccessDo(async () =>
                        {
                            await _tableStorageService.DeleteEntityAsync("imports", deletePlan.TableStorageItem);
                        });
                });
        }

        private async Task DeleteChartFiles(DeleteDataFilePlan deletePlan)
        {
            var deletes = deletePlan.DependentDataBlocks.SelectMany(block =>
                block.InfographicFilenames.Select(chartFilename =>
                    _fileStorageService.DeleteFileAsync(deletePlan.ReleaseId, ReleaseFileTypes.Chart, chartFilename)
                )
            );
            
            await Task.WhenAll(deletes);
        }

        private async Task DeleteDependentDataBlocks(DeleteDataFilePlan deletePlan)
        {
            var blockIdsToDelete = deletePlan
                .DependentDataBlocks
                .Select(block => block.Id);
            
            var dependentDataBlocks = _context
                .DataBlocks
                .Where(block => blockIdsToDelete.Contains(block.Id));
            
            _context.ContentBlocks.RemoveRange(dependentDataBlocks);
            await _context.SaveChangesAsync();
        }

        private async Task<Either<ActionResult, bool>> CheckCanDeleteDataFiles(Guid releaseId, string fileName)
        {
            var importFinished = await _importStatusService.IsImportFinished(releaseId.ToString(), fileName);
            
            if (!importFinished)
            {
                return ValidationActionResult(CannotRemoveDataFilesUntilImportComplete);
            }

            return true;
        }

        private List<DataBlock> GetDependentDataBlocks(Guid releaseId, Guid subjectId)
        {
            return _context
                .ReleaseContentBlocks
                .Include(join => join.ContentBlock)
                .ThenInclude(block => block.ContentSection)
                .Where(join => join.ReleaseId == releaseId)
                .ToList()
                .Select(join => join.ContentBlock)
                .Where(block => block.GetType() == typeof(DataBlock))
                .Cast<DataBlock>()
                .Where(block => block.DataBlockRequest.SubjectId == subjectId)
                .ToList();
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
        
        public static IQueryable<Release> HydrateReleaseForAmendment(IQueryable<Release> values)
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
        
        public List<DependentDataBlock> DependentDataBlocks { get; set; }
        
        public List<Guid> FootnoteIds { get; set; }
    }

    public class DependentDataBlock
    {
        [JsonIgnore]
        public Guid Id { get; set; }
        
        public string Name { get; set; }
        
        public string? ContentSectionHeading { get; set; }
        
        public List<string> InfographicFilenames { get; set; }
    }
}