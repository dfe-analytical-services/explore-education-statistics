#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ReleaseChecklistService : IReleaseChecklistService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IDataImportService _dataImportService;
        private readonly IUserService _userService;
        private readonly IDataGuidanceService _dataGuidanceService;
        private readonly IMethodologyVersionRepository _methodologyVersionRepository;
        private readonly IReleaseDataFileRepository _fileRepository;
        private readonly IFootnoteRepository _footnoteRepository;
        private readonly IDataBlockService _dataBlockService;

        public ReleaseChecklistService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IDataImportService dataImportService,
            IUserService userService,
            IDataGuidanceService dataGuidanceService,
            IReleaseDataFileRepository fileRepository,
            IMethodologyVersionRepository methodologyVersionRepository,
            IFootnoteRepository footnoteRepository,
            IDataBlockService dataBlockService)
        {
            _contentDbContext = contentDbContext;
            _persistenceHelper = persistenceHelper;
            _dataImportService = dataImportService;
            _userService = userService;
            _dataGuidanceService = dataGuidanceService;
            _fileRepository = fileRepository;
            _methodologyVersionRepository = methodologyVersionRepository;
            _footnoteRepository = footnoteRepository;
            _dataBlockService = dataBlockService;
        }

        public async Task<Either<ActionResult, ReleaseChecklistViewModel>> GetChecklist(Guid releaseId)
        {
            return await _persistenceHelper.CheckEntityExists<Release>(releaseId, HydrateReleaseForChecklist)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(
                    async release => new ReleaseChecklistViewModel(
                        await GetErrors(release),
                        await GetWarnings(release)
                    )
                );
        }

        public static IQueryable<Release> HydrateReleaseForChecklist(IQueryable<Release> query)
        {
            return query.Include(r => r.Publication)
                .Include(r => r.Updates);
        }

        public async Task<List<ReleaseChecklistIssue>> GetErrors(Release release)
        {
            var errors = new List<ReleaseChecklistIssue>();

            if (await _dataImportService.HasIncompleteImports(release.Id))
            {
                errors.Add(new ReleaseChecklistIssue(ValidationErrorMessages.DataFileImportsMustBeCompleted));
            }

            var replacementDataFiles = await _fileRepository.ListReplacementDataFiles(release.Id);

            if (replacementDataFiles.Any())
            {
                errors.Add(new ReleaseChecklistIssue(ValidationErrorMessages.DataFileReplacementsMustBeCompleted));
            }

            var isDataGuidanceValid = await _dataGuidanceService.Validate(release.Id);

            if (isDataGuidanceValid.IsLeft)
            {
                errors.Add(new ReleaseChecklistIssue(ValidationErrorMessages.PublicDataGuidanceRequired));
            }

            if (release.Amendment
                && !release.Updates.Any(update => update.Created > release.Created))
            {
                errors.Add(new ReleaseChecklistIssue(ValidationErrorMessages.ReleaseNoteRequired));
            }

            if (await ReleaseHasEmptyGenericContentSection(release.Id))
            {
                errors.Add(new ReleaseChecklistIssue(ValidationErrorMessages.EmptyContentSectionExists));
            }

            if (await ReleaseGenericContentSectionsContainEmptyContentBlock(release.Id))
            {
                errors.Add(new ReleaseChecklistIssue(ValidationErrorMessages.GenericSectionsContainEmptyHtmlBlock));
            }

            return errors;
        }

        private async Task<bool> ReleaseHasEmptyGenericContentSection(Guid releaseId)
        {
            return await _contentDbContext.ReleaseContentSections
                .Include(rcs => rcs.ContentSection)
                .ThenInclude(cs => cs.Content)
                .Where(rcs =>
                    rcs.ReleaseId == releaseId
                    && rcs.ContentSection.Type == ContentSectionType.Generic)
                .AnyAsync(rcs => rcs.ContentSection.Content.Count == 0);
        }

        private async Task<bool> ReleaseGenericContentSectionsContainEmptyContentBlock(Guid releaseId)
        {
            var releaseGenericContentBlocks = await _contentDbContext.ReleaseContentSections
                .Include(rcs => rcs.ContentSection)
                .ThenInclude(cs => cs.Content)
                .Where(rcs =>
                    rcs.ReleaseId == releaseId
                    && rcs.ContentSection.Type == ContentSectionType.Generic)
                .SelectMany(rcs => rcs.ContentSection.Content)
                .ToListAsync();

            return releaseGenericContentBlocks
                .Any(block =>
                {
                    if (block is HtmlBlock htmlBlock)
                    {
                        return htmlBlock.Body.IsNullOrEmpty();
                    }
                    return false;
                });
        }

        public async Task<List<ReleaseChecklistIssue>> GetWarnings(Release release)
        {
            var warnings = new List<ReleaseChecklistIssue>();

            var methodologies = await _methodologyVersionRepository.GetLatestVersionByPublication(release.PublicationId);
            if (!methodologies.Any())
            {
                warnings.Add(new ReleaseChecklistIssue(ValidationErrorMessages.NoMethodology));
            }

            var methodologiesNotApproved = methodologies
                .Where(m => m.Status != MethodologyStatus.Approved)
                .ToList();

            if (methodologiesNotApproved.Any())
            {
                warnings.AddRange(methodologiesNotApproved.Select(m =>
                    new MethodologyNotApprovedWarning(m.Id)));
            }

            if (release.NextReleaseDate == null)
            {
                warnings.Add(new ReleaseChecklistIssue(ValidationErrorMessages.NoNextReleaseDate));
            }

            var dataFiles = await _fileRepository.ListDataFiles(release.Id);

            if (!dataFiles.Any())
            {
                warnings.Add(new ReleaseChecklistIssue(ValidationErrorMessages.NoDataFiles));
            }
            else
            {
                var subjectsWithNoFootnotes = await GetSubjectsWithNoFootnotes(release, dataFiles);

                if (subjectsWithNoFootnotes.Any())
                {
                    warnings.Add(new NoFootnotesOnSubjectsWarning(subjectsWithNoFootnotes.Count));
                }

                var tableHighlights = await GetDataBlocksWithHighlights(release);

                if (!tableHighlights.Any())
                {
                    warnings.Add(new ReleaseChecklistIssue(ValidationErrorMessages.NoTableHighlights));
                }
            }

            if (release.PreReleaseAccessList.IsNullOrEmpty())
            {
                warnings.Add(new ReleaseChecklistIssue(ValidationErrorMessages.NoPublicPreReleaseAccessList));
            }

            return warnings;
        }

        private async Task<List<Subject>> GetSubjectsWithNoFootnotes(
            Release release,
            IEnumerable<File> dataFiles)
        {
            var allowedSubjectIds = dataFiles
                .Where(dataFile => dataFile.SubjectId.HasValue)
                .Select(dataFile => dataFile.SubjectId!.Value);

            return (await _footnoteRepository.GetSubjectsWithNoFootnotes(release.Id))
                .Where(subject => allowedSubjectIds.Contains(subject.Id))
                .ToList();
        }

        private async Task<List<DataBlockSummaryViewModel>> GetDataBlocksWithHighlights(Release release)
        {
            return (await _dataBlockService.List(release.Id))
                .FoldRight(
                    dataBlocks => dataBlocks
                        .Where(dataBlock => !dataBlock.HighlightName.IsNullOrEmpty())
                        .ToList(),
                    new List<DataBlockSummaryViewModel>()
                );
        }
    }
}
