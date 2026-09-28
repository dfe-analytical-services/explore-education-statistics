#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services;

public class DataGuidanceDataSetService : IDataGuidanceDataSetService
{
    private readonly ContentDbContext _contentDbContext;
    private readonly IStorageDataSetResolver _storageDataSetResolver;
    private readonly IFootnoteRepository _footnoteRepository;
    private readonly ITimePeriodService _timePeriodService;

    public DataGuidanceDataSetService(
        ContentDbContext contentDbContext,
        IStorageDataSetResolver storageDataSetResolver,
        IFootnoteRepository footnoteRepository,
        ITimePeriodService timePeriodService
    )
    {
        _contentDbContext = contentDbContext;
        _storageDataSetResolver = storageDataSetResolver;
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
                        .ThenInclude(f => f.DataSetFileVersionGeographicLevels)
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
                        var dataSet = await _storageDataSetResolver.Resolve(subjectId, cancellationToken);

                        var timePeriods = await _timePeriodService.GetTimePeriodLabels(dataSet);
                        var variables = await ListVariables(dataSet, cancellationToken);
                        var footnotes = await ListFootnotes(releaseVersionId: releaseVersionId, subjectId: subjectId);

                        return BuildDataGuidanceDataSetViewModel(releaseFile, timePeriods, variables, footnotes);
                    })
                    .OrderBy(viewModel => viewModel.Order)
                    .ThenBy(viewModel => viewModel.Name) // For data sets existing before ordering was added
                    .ToListAsync(cancellationToken);
            });
    }

    private static async Task<List<LabelValue>> ListVariables(
        IStorageDataSet dataSet,
        CancellationToken cancellationToken = default
    )
    {
        var filters = (await dataSet.ListFilters(cancellationToken)).Select(filter => new LabelValue(
            string.IsNullOrWhiteSpace(filter.Hint) ? filter.Label : $"{filter.Label} - {filter.Hint}",
            filter.Name
        ));

        var indicators = (await dataSet.ListIndicators(cancellationToken)).Select(indicator => new LabelValue(
            indicator.Label,
            indicator.Name
        ));

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
            GeographicLevels = releaseFile
                .File.DataSetFileVersionGeographicLevels.Where(level => !level.CsvOnly)
                .Select(level => level.GeographicLevel.GetEnumLabel())
                .Order()
                .ToList(),
            GeographicLevelsCsvOnly = releaseFile
                .File.DataSetFileVersionGeographicLevels.Where(level => level.CsvOnly)
                .Select(level => level.GeographicLevel.GetEnumLabel())
                .Order()
                .ToList(),
            TimePeriods = timePeriods,
            Variables = variables,
            Footnotes = footnotes,
        };
    }
}
