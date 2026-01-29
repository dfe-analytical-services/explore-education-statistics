#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseVersion;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class ReleaseChecklistService : IReleaseChecklistService
{
    private readonly ContentDbContext _contentDbContext;
    private readonly IDataImportService _dataImportService;
    private readonly IUserService _userService;
    private readonly IDataGuidanceService _dataGuidanceService;
    private readonly IMethodologyVersionRepository _methodologyVersionRepository;
    private readonly IReleaseDataFileRepository _fileRepository;
    private readonly IFootnoteRepository _footnoteRepository;
    private readonly IDataBlockService _dataBlockService;
    private readonly IDataSetService _dataSetService;
    private readonly IDataSetVersionService _dataSetVersionService;

    public ReleaseChecklistService(
        ContentDbContext contentDbContext,
        IDataImportService dataImportService,
        IUserService userService,
        IDataGuidanceService dataGuidanceService,
        IReleaseDataFileRepository fileRepository,
        IMethodologyVersionRepository methodologyVersionRepository,
        IFootnoteRepository footnoteRepository,
        IDataBlockService dataBlockService,
        IDataSetService dataSetService,
        IDataSetVersionService dataSetVersionService
    )
    {
        _contentDbContext = contentDbContext;
        _dataImportService = dataImportService;
        _userService = userService;
        _dataGuidanceService = dataGuidanceService;
        _fileRepository = fileRepository;
        _methodologyVersionRepository = methodologyVersionRepository;
        _footnoteRepository = footnoteRepository;
        _dataBlockService = dataBlockService;
        _dataSetService = dataSetService;
        _dataSetVersionService = dataSetVersionService;
    }

    public async Task<Either<ActionResult, ReleaseChecklistViewModel>> GetChecklist(Guid releaseVersionId)
    {
        return await _contentDbContext
            .ReleaseVersions.HydrateReleaseForChecklist()
            .SingleOrNotFoundAsync(rv => rv.Id == releaseVersionId)
            .OnSuccess(_userService.CheckCanViewReleaseVersion)
            .OnSuccess(async release => new ReleaseChecklistViewModel(
                await GetErrors(release),
                await GetWarnings(release)
            ));
    }

    public async Task<List<ReleaseChecklistIssue>> GetErrors(ReleaseVersion releaseVersion)
    {
        var errors = new List<ReleaseChecklistIssue>();

        if (await _dataImportService.HasIncompleteImports(releaseVersion.Id))
        {
            errors.Add(new ReleaseChecklistIssue(ValidationErrorMessages.DataFileImportsMustBeCompleted));
        }

        var replacementDataFiles = await _fileRepository.ListReplacementDataFiles(releaseVersion.Id);

        if (replacementDataFiles.Any())
        {
            errors.Add(new ReleaseChecklistIssue(ValidationErrorMessages.DataFileReplacementsMustBeCompleted));
        }

        var isDataGuidanceValid = await _dataGuidanceService.ValidateForReleaseChecklist(releaseVersion.Id);

        if (isDataGuidanceValid.IsLeft)
        {
            errors.Add(new ReleaseChecklistIssue(ValidationErrorMessages.PublicDataGuidanceRequired));
        }

        if (releaseVersion.Amendment && !releaseVersion.Updates.Any(update => update.Created > releaseVersion.Created))
        {
            errors.Add(new ReleaseChecklistIssue(ValidationErrorMessages.ReleaseNoteRequired));
        }

        if (await ReleaseSectionHasEmptyHtmlBlock(releaseVersion.Id, ContentSectionType.ReleaseSummary))
        {
            errors.Add(new ReleaseChecklistIssue(ValidationErrorMessages.SummarySectionContainsEmptyHtmlBlock));
        }

        if (await ReleaseHasEmptySection(releaseVersion.Id, ContentSectionType.Generic))
        {
            errors.Add(new ReleaseChecklistIssue(ValidationErrorMessages.EmptyContentSectionExists));
        }

        if (await ReleaseSectionHasEmptyHtmlBlock(releaseVersion.Id, ContentSectionType.Generic))
        {
            errors.Add(new ReleaseChecklistIssue(ValidationErrorMessages.GenericSectionsContainEmptyHtmlBlock));
        }

        if (
            !(
                await ReleaseHasKeyStatistic(releaseVersion.Id)
                || await ReleaseSectionHasNonEmptyHtmlBlock(releaseVersion.Id, ContentSectionType.Headlines)
            )
        )
        {
            errors.Add(
                new ReleaseChecklistIssue(ValidationErrorMessages.ReleaseMustContainKeyStatOrNonEmptyHeadlineBlock)
            );
        }

        if (await ReleaseSectionHasEmptyHtmlBlock(releaseVersion.Id, ContentSectionType.RelatedDashboards))
        {
            errors.Add(
                new ReleaseChecklistIssue(ValidationErrorMessages.RelatedDashboardsSectionContainsEmptyHtmlBlock)
            );
        }

        var dataSetVersionStatuses = await _dataSetVersionService.GetStatusesForReleaseVersion(releaseVersion.Id);

        if (dataSetVersionStatuses.Any(status => status.Status == DataSetVersionStatus.Processing))
        {
            errors.Add(new ReleaseChecklistIssue(ValidationErrorMessages.PublicApiDataSetImportsMustBeCompleted));
        }

        if (dataSetVersionStatuses.Any(status => status.Status == DataSetVersionStatus.Cancelled))
        {
            errors.Add(new ReleaseChecklistIssue(ValidationErrorMessages.PublicApiDataSetCancellationsMustBeResolved));
        }

        if (dataSetVersionStatuses.Any(status => status.Status == DataSetVersionStatus.Failed))
        {
            errors.Add(new ReleaseChecklistIssue(ValidationErrorMessages.PublicApiDataSetFailuresMustBeResolved));
        }

        if (
            dataSetVersionStatuses.Any(status =>
                status.Status is DataSetVersionStatus.Mapping or DataSetVersionStatus.Finalising
            )
        )
        {
            errors.Add(new ReleaseChecklistIssue(ValidationErrorMessages.PublicApiDataSetMappingsMustBeCompleted));
        }

        return errors;
    }

    private async Task<bool> ReleaseHasKeyStatistic(Guid releaseVersionId)
    {
        return await _contentDbContext.KeyStatistics.AnyAsync(ks => ks.ReleaseVersionId == releaseVersionId);
    }

    private async Task<bool> ReleaseHasEmptySection(Guid releaseVersionId, ContentSectionType sectionType)
    {
        return await _contentDbContext
            .ContentSections.Where(cs => cs.ReleaseVersionId == releaseVersionId && cs.Type == sectionType)
            .AnyAsync(cs => cs.Content.Count == 0);
    }

    private async Task<bool> ReleaseSectionHasEmptyHtmlBlock(Guid releaseVersionId, ContentSectionType sectionType)
    {
        return await _contentDbContext
            .ContentBlocks.Where(cb =>
                cb.ContentSection!.ReleaseVersionId == releaseVersionId && cb.ContentSection.Type == sectionType
            )
            .OfType<HtmlBlock>()
            .AnyAsync(htmlBlock => string.IsNullOrEmpty(htmlBlock.Body));
    }

    private async Task<bool> ReleaseSectionHasNonEmptyHtmlBlock(Guid releaseVersionId, ContentSectionType sectionType)
    {
        return await _contentDbContext
            .ContentBlocks.Where(cb =>
                cb.ContentSection!.ReleaseVersionId == releaseVersionId && cb.ContentSection.Type == sectionType
            )
            .OfType<HtmlBlock>()
            .AnyAsync(htmlBlock => !string.IsNullOrEmpty(htmlBlock.Body));
    }

    public async Task<List<ReleaseChecklistIssue>> GetWarnings(ReleaseVersion releaseVersion)
    {
        var warnings = new List<ReleaseChecklistIssue>();

        var methodologies = await _methodologyVersionRepository.GetLatestVersionByPublication(
            releaseVersion.Release.PublicationId
        );

        if (!methodologies.Any())
        {
            warnings.Add(new ReleaseChecklistIssue(ValidationErrorMessages.NoMethodology));
        }

        var methodologiesNotApproved = methodologies
            .Where(m => m.Status != MethodologyApprovalStatus.Approved)
            .ToList();

        if (methodologiesNotApproved.Any())
        {
            warnings.AddRange(methodologiesNotApproved.Select(m => new MethodologyNotApprovedWarning(m.Id)));
        }

        if (releaseVersion.NextReleaseDate == null)
        {
            warnings.Add(new ReleaseChecklistIssue(ValidationErrorMessages.NoNextReleaseDate));
        }

        var dataFiles = await _fileRepository.ListDataFiles(releaseVersion.Id);

        if (!dataFiles.Any())
        {
            warnings.Add(new ReleaseChecklistIssue(ValidationErrorMessages.NoDataFiles));
        }
        else
        {
            var subjectsWithNoFootnotes = await GetSubjectsWithNoFootnotes(releaseVersion, dataFiles);

            if (subjectsWithNoFootnotes.Any())
            {
                warnings.Add(new NoFootnotesOnSubjectsWarning(subjectsWithNoFootnotes.Count));
            }

            if (!await HasFeaturedTable(releaseVersion.Id))
            {
                warnings.Add(new ReleaseChecklistIssue(ValidationErrorMessages.NoFeaturedTables));
            }
        }

        if (string.IsNullOrEmpty(releaseVersion.PreReleaseAccessList))
        {
            warnings.Add(new ReleaseChecklistIssue(ValidationErrorMessages.NoPublicPreReleaseAccessList));
        }

        if (await HasUnresolvedComments(releaseVersion.Id))
        {
            warnings.Add(new ReleaseChecklistIssue(ValidationErrorMessages.UnresolvedComments));
        }

        if (await IsMissingUpdatedApiDataSet(releaseVersion, dataFiles))
        {
            warnings.Add(new ReleaseChecklistIssue(ValidationErrorMessages.MissingUpdatedApiDataSet));
        }

        return warnings;
    }

    private async Task<bool> IsMissingUpdatedApiDataSet(ReleaseVersion releaseVersion, IList<File> dataFiles)
    {
        // if it's a new/unpublished publication, there's no previous release to check
        if (releaseVersion.Release.Publication.LatestPublishedReleaseVersionId is null)
        {
            return false;
        }

        // if no data files were uploaded, there's no data to associate with an existing API data set
        if (!dataFiles.Any())
        {
            return false;
        }

        // get data sets associated to this publication
        // TODO: Use results pattern
        var existingDataSetsForPublication = await _dataSetService.ListDataSets(releaseVersion.Release.PublicationId);

        // if there are fewer uploads than API data sets, this may be intentional (e.g. two API data sets, but only one needs updating)
        if (dataFiles.Count < existingDataSetsForPublication.Right.Count)
        {
            return false;
        }

        // if no new data set version has been associated to this release, add the warning
        if (existingDataSetsForPublication.Right.Any(r => DataSetVersionIsNotAssociatedToRelease(r, releaseVersion.Id)))
        {
            return true;
        }

        return false;
    }

    private static bool DataSetVersionIsNotAssociatedToRelease(
        DataSetSummaryViewModel dataSet,
        Guid releaseVersionId
    ) => // might be wrong logic? Been suggested that this shouldn't work with amendment scenarios when comparing releaseVersionIds
        dataSet.LatestLiveVersion?.ReleaseVersion.Id != releaseVersionId
        && (
            dataSet.DraftVersion is null
            || dataSet.DraftVersion.ReleaseVersion.Id != releaseVersionId
            || dataSet.DraftVersion.Status is not DataSetVersionStatus.Mapping and not DataSetVersionStatus.Draft
        );

    private async Task<bool> HasUnresolvedComments(Guid releaseVersionId)
    {
        return await _contentDbContext
            .Comment.Where(c => c.ContentBlock.ContentSection!.ReleaseVersionId == releaseVersionId)
            .AnyAsync(c => c.Resolved == null);
    }

    private async Task<List<Subject>> GetSubjectsWithNoFootnotes(
        ReleaseVersion releaseVersion,
        IEnumerable<File> dataFiles
    )
    {
        var allowedSubjectIds = dataFiles
            .Where(dataFile => dataFile.SubjectId.HasValue)
            .Select(dataFile => dataFile.SubjectId!.Value);

        return (await _footnoteRepository.GetSubjectsWithNoFootnotes(releaseVersion.Id))
            .Where(subject => allowedSubjectIds.Contains(subject.Id))
            .ToList();
    }

    private async Task<bool> HasFeaturedTable(Guid releaseVersionId)
    {
        var dataBlocks = await _dataBlockService.ListDataBlocks(releaseVersionId);
        var dataBlockIds = dataBlocks.Select(dataBlock => dataBlock.Id);
        return await _contentDbContext.FeaturedTables.AnyAsync(ft => dataBlockIds.Contains(ft.DataBlockId));
    }
}

public static class ReleaseChecklistQueryableExtensions
{
    public static IQueryable<ReleaseVersion> HydrateReleaseForChecklist(this IQueryable<ReleaseVersion> query)
    {
        return query.Include(rv => rv.Release).ThenInclude(r => r.Publication).Include(rv => rv.Updates);
    }
}
