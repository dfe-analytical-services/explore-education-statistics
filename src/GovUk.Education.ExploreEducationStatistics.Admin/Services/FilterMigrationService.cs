#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

/// <summary>
/// TODO EES-4372 Remove after the EES-4364 filter migration is complete
/// </summary>
public class FilterMigrationService : IFilterMigrationService
{
    private readonly ContentDbContext _contentDbContext;
    private readonly StatisticsDbContext _statisticsDbContext;
    private readonly IPrivateBlobStorageService _privateBlobStorageService;
    private readonly IUserService _userService;
    private readonly ILogger<FilterMigrationService> _logger;

    public FilterMigrationService(ContentDbContext contentDbContext,
        StatisticsDbContext statisticsDbContext,
        IPrivateBlobStorageService privateBlobStorageService,
        IUserService userService,
        ILogger<FilterMigrationService> logger)
    {
        _contentDbContext = contentDbContext;
        _statisticsDbContext = statisticsDbContext;
        _privateBlobStorageService = privateBlobStorageService;
        _userService = userService;
        _logger = logger;
    }

    public async Task<Either<ActionResult, FilterMigrationReport>> MigrateGroupCsvColumns(
        bool dryRun = true,
        CancellationToken cancellationToken = default)
    {
        return await _userService.CheckIsBauUser()
            .OnSuccess(() => DoMigrateGroupCsvColumns(dryRun, cancellationToken));
    }

    private async Task<Either<ActionResult, FilterMigrationReport>> DoMigrateGroupCsvColumns(
        bool dryRun = true,
        CancellationToken cancellationToken = default)
    {
        var report = new FilterMigrationReport();

        var filtersWithNonDefaultGroups = await GetFiltersWithNonDefaultGroups(cancellationToken);

        await filtersWithNonDefaultGroups
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(async tuple =>
            {
                var (subjectId, filters) = tuple;

                // If all filters have a group csv column value already, skip this data set
                if (filters.All(filter => filter.GroupCsvColumn != null))
                {
                    report.AddAllFiltersHaveGroupCsvColumnValues(subjectId);
                    return;
                }

                var file = await GetMetaFile(subjectId, cancellationToken);
                if (file == null)
                {
                    report.AddMetaFileNotFound(subjectId);
                    return;
                }

                Dictionary<string, FilterRow> metaCsvFilters;
                try
                {
                    var stream = await GetMetaCsvStream(file, cancellationToken);
                    metaCsvFilters = await GetMetaCsvFilters(stream);
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception,
                        "Exception caught getting meta filter rows. (SubjectId={SubjectId}, FileId={FileId}), Path={Path})",
                        subjectId,
                        file.Id,
                        file.Path());
                    report.AddExceptionGettingMetaFilters(subjectId, file, exception);
                    return;
                }

                filters.ForEach(filter =>
                {
                    if (!metaCsvFilters.TryGetValue(filter.Name, out var metaCsvFilter))
                    {
                        report.AddMetaFilterNotFound(subjectId, file, filter);
                        return;
                    }

                    // Sanity check that the filter label matches the label in the metadata file
                    if (filter.Label != metaCsvFilter.Label)
                    {
                        report.AddFilterLabelMismatch(subjectId, file, filter);
                        return;
                    }

                    if (!filter.GroupCsvColumn.IsNullOrEmpty())
                    {
                        report.AddFilterAlreadyHasGroupCsvColumnValue(subjectId, file, filter);
                        return;
                    }

                    _statisticsDbContext.Update(filter);
                    filter.GroupCsvColumn = metaCsvFilter.GroupCsvColumn;
                    report.FiltersUpdated.Add(filter.Id);
                });
            }, cancellationToken: cancellationToken);

        if (!dryRun && report.FiltersUpdated.Any())
        {
            await _statisticsDbContext.SaveChangesAsync(cancellationToken);
        }

        return report;
    }

    private static async Task<Dictionary<string, FilterRow>> GetMetaCsvFilters(Stream stream)
    {
        await using (stream)
        {
            Task<Stream> StreamProvider() => Task.FromResult(stream);
            var metaCsvHeaders = await CsvUtils.GetCsvHeaders(StreamProvider, leaveOpen: true);
            stream.SeekToBeginning();
            var metaCsvRows = await CsvUtils.GetCsvRows(StreamProvider);

            return new MetaDataFileReader(metaCsvHeaders)
                .GetFilterRows(metaCsvRows)
                .ToDictionary(filterRow => filterRow.Name);
        }
    }

    private async Task<Stream> GetMetaCsvStream(
        File file,
        CancellationToken cancellationToken = default)
    {
        return await _privateBlobStorageService.StreamBlob(
            containerName: BlobContainers.PrivateReleaseFiles,
            path: file.Path(),
            cancellationToken: cancellationToken
        );
    }

    private async Task<File?> GetMetaFile(Guid subjectId,
        CancellationToken cancellationToken = default)
    {
        return await _contentDbContext.Files
            .SingleOrDefaultAsync(file => file.SubjectId == subjectId && file.Type == FileType.Metadata,
                cancellationToken: cancellationToken);
    }

    private async Task<Dictionary<Guid, List<Filter>>> GetFiltersWithNonDefaultGroups(
        CancellationToken cancellationToken = default)
    {
        var filtersWithNonDefaultGroups = await _statisticsDbContext.Filter
            .Where(filter => filter.FilterGroups.Any(filterGroup =>
                filterGroup.Label != FilterGroup.DefaultFilterGroupLabel))
            .ToListAsync(cancellationToken: cancellationToken);

        return filtersWithNonDefaultGroups.GroupBy(filter => filter.SubjectId)
            .ToDictionary(group => group.Key,
                group => group.ToList());
    }
}

internal class MetaDataFileReader
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private enum MetaColumns
    {
        col_name,
        col_type,
        label,
        filter_grouping_column
    }

    private readonly Dictionary<MetaColumns, int> _metaColumnIndexes;

    public MetaDataFileReader(List<string> metaCsvHeaders)
    {
        _metaColumnIndexes = EnumUtil.GetEnumValues<MetaColumns>()
            .ToDictionary(
                column => column,
                column => metaCsvHeaders.FindIndex(h => h.Equals(column.ToString()))
            );
    }

    public List<FilterRow> GetFilterRows(IReadOnlyList<IReadOnlyList<string>> metaRowValues)
    {
        return metaRowValues
            .Where(rowValues =>
            {
                var columnType = ReadMetaColumnValue(MetaColumns.col_type, rowValues);
                return "filter".Equals(columnType, StringComparison.OrdinalIgnoreCase);
            })
            .Select(GetFilterRow)
            .ToList();
    }

    private FilterRow GetFilterRow(IReadOnlyList<string> rowValues)
    {
        return new FilterRow(
            Name: ReadMetaColumnValue(MetaColumns.col_name, rowValues) ??
                  throw new ArgumentNullException(MetaColumns.col_name.ToString(), "Csv filter name is null"),
            Label: ReadMetaColumnValue(MetaColumns.label, rowValues)
                   ?? throw new ArgumentNullException(MetaColumns.label.ToString(), "Csv filter label is null"),
            GroupCsvColumn: ReadMetaColumnValue(MetaColumns.filter_grouping_column, rowValues));
    }

    private string? ReadMetaColumnValue(MetaColumns column, IReadOnlyList<string> rowValues)
    {
        var columnIndex = _metaColumnIndexes[column];
        if (columnIndex == -1)
        {
            throw new ArgumentOutOfRangeException(nameof(column), $"Meta column: {column} not found in csv headers");
        }

        return rowValues[columnIndex].Trim().NullIfWhiteSpace();
    }
}

internal record FilterRow(string Name, string Label, string? GroupCsvColumn);
