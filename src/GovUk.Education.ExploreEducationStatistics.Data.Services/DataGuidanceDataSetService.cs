#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services;

public class DataGuidanceDataSetService : IDataGuidanceDataSetService
{
    private readonly StatisticsDbContext _statisticsDbContext;
    private readonly ContentDbContext _contentDbContext;
    private readonly IIndicatorRepository _indicatorRepository;
    private readonly IFootnoteRepository _footnoteRepository;
    private readonly ITimePeriodService _timePeriodService;

    public DataGuidanceDataSetService(
        StatisticsDbContext statisticsDbContext,
        ContentDbContext contentDbContext,
        IIndicatorRepository indicatorRepository,
        IFootnoteRepository footnoteRepository,
        ITimePeriodService timePeriodService
    )
    {
        _statisticsDbContext = statisticsDbContext;
        _contentDbContext = contentDbContext;
        _indicatorRepository = indicatorRepository;
        _footnoteRepository = footnoteRepository;
        _timePeriodService = timePeriodService;
    }

    public async Task<Either<ActionResult, List<DataGuidanceDataSetViewModel>>> ListDataSets(
        Guid releaseVersionId,
        IList<Guid>? dataFileIds = null,
        CancellationToken cancellationToken = default
    )
    {
        return await _contentDbContext
            .ReleaseVersions.FirstOrNotFoundAsync(
                releaseVersion => releaseVersion.Id == releaseVersionId,
                cancellationToken
            )
            .OnSuccess(async () =>
            {
                var releaseFilesQueryable = _contentDbContext
                    .ReleaseFiles.Include(rf => rf.File)
                    .Where(rf =>
                        rf.ReleaseVersionId == releaseVersionId
                        && rf.File.Type == FileType.Data
                        && rf.File.ReplacingId == null
                    );

                if (dataFileIds != null)
                {
                    releaseFilesQueryable = releaseFilesQueryable.Where(rf => dataFileIds.Contains(rf.FileId));
                }

                return await releaseFilesQueryable
                    .ToAsyncEnumerable()
                    .SelectAwait(async releaseFile =>
                    {
                        var subjectId = releaseFile.File.SubjectId!.Value;

                        var geographicLevels = await ListGeographicLevels(subjectId, cancellationToken);
                        var timePeriods = await _timePeriodService.GetTimePeriodLabels(subjectId);
                        var variables = await ListVariables(subjectId, cancellationToken);
                        var footnotes = await ListFootnotes(releaseVersionId: releaseVersionId, subjectId: subjectId);

                        return BuildDataGuidanceDataSetViewModel(
                            releaseFile,
                            geographicLevels,
                            timePeriods,
                            variables,
                            footnotes
                        );
                    })
                    .OrderBy(viewModel => viewModel.Order)
                    .ThenBy(viewModel => viewModel.Name) // For data sets existing before ordering was added
                    .ToListAsync(cancellationToken);
            });
    }

    public async Task<List<string>> ListGeographicLevels(Guid subjectId, CancellationToken cancellationToken = default)
    {
        return await _statisticsDbContext
            .Observation.AsNoTracking()
            .Where(o => o.SubjectId == subjectId)
            .Select(observation => observation.Location.GeographicLevel.GetEnumLabel())
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    private async Task<List<LabelValue>> ListVariables(Guid subjectId, CancellationToken cancellationToken = default)
    {
        var filters = await _statisticsDbContext
            .Filter.Where(filter => filter.SubjectId == subjectId)
            .Select(filter => new LabelValue(
                string.IsNullOrWhiteSpace(filter.Hint) ? filter.Label : $"{filter.Label} - {filter.Hint}",
                filter.Name
            ))
            .ToListAsync(cancellationToken);

        var indicators = _indicatorRepository
            .GetIndicators(subjectId)
            .Select(indicator => new LabelValue(indicator.Label, indicator.Name));

        return filters.Concat(indicators).OrderBy(labelValue => labelValue.Value).ToList();
    }

    private async Task<List<FootnoteViewModel>> ListFootnotes(Guid releaseVersionId, Guid subjectId)
    {
        var footnotes = await _footnoteRepository.GetFootnotes(
            releaseVersionId: releaseVersionId,
            subjectId: subjectId
        );
        return FootnotesViewModelBuilder.BuildFootnotes(footnotes);
    }

    private static DataGuidanceDataSetViewModel BuildDataGuidanceDataSetViewModel(
        ReleaseFile releaseFile,
        List<string> geographicLevels,
        TimePeriodLabels timePeriods,
        List<LabelValue> variables,
        List<FootnoteViewModel> footnotes
    )
    {
        return new DataGuidanceDataSetViewModel
        {
            FileId = releaseFile.FileId,
            Content = releaseFile.Summary ?? "",
            Filename = releaseFile.File.Filename,
            Order = releaseFile.Order,
            Name = releaseFile.Name ?? "",
            GeographicLevels = geographicLevels,
            TimePeriods = timePeriods,
            Variables = variables,
            Footnotes = footnotes,
        };
    }
}
