using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum MetaColumns {
        col_name,
        col_type,
        label,
        filter_grouping_column,
        filter_hint,
        indicator_grouping,
        indicator_unit,
        indicator_dp
    }

    public class ImporterMetaService : IImporterMetaService
    {
        private readonly IGuidGenerator _guidGenerator;
        private readonly IDatabaseHelper _databaseHelper;
        
        public ImporterMetaService(
            IGuidGenerator guidGenerator, 
            IDatabaseHelper databaseHelper)
        {
            _guidGenerator = guidGenerator;
            _databaseHelper = databaseHelper;
        }

        public async Task<SubjectMeta> Import(
            List<string> metaFileCsvHeaders,
            List<List<string>> metaFileRows, 
            Subject subject,
            StatisticsDbContext context)
        {
            var metaRows = GetMetaRows(metaFileCsvHeaders, metaFileRows);
            var filtersAndMeta = ReadFiltersFromCsv(metaRows, subject);
            var indicatorsAndMeta = ReadIndicatorsFromCsv(metaRows, subject);

            var filtersAlreadyImported = filtersAndMeta.Count > 0 &&
                                          await context.Filter.AnyAsync(filter => filter.SubjectId == subject.Id);
            
            var indicatorsAlreadyImported = indicatorsAndMeta.Count > 0 && 
                                            await context.IndicatorGroup.AnyAsync(indicator => indicator.SubjectId == subject.Id);

            if (!filtersAlreadyImported || !indicatorsAlreadyImported)
            {
                var filters = filtersAndMeta.Select(f => f.Filter).ToList();
                filters.ForEach(filter => filter.Id = _guidGenerator.NewGuid());

                var indicators = indicatorsAndMeta.Select(i => i.Indicator).ToList();
                indicators.ForEach(indicator => indicator.Id = _guidGenerator.NewGuid());

                await _databaseHelper.DoInTransaction(
                    context,
                    async ctxDelegate =>
                    {
                        await ctxDelegate.Filter.AddRangeAsync(filters);
                        await ctxDelegate.Indicator.AddRangeAsync(indicators);
                        await ctxDelegate.SaveChangesAsync();
                    });
            }
            
            return new SubjectMeta
            {
                Filters = filtersAndMeta,
                Indicators = indicatorsAndMeta
            };
        }

        public SubjectMeta Get(
            List<string> metaFileCsvHeaders,
            List<List<string>> metaFileRows, 
            Subject subject,
            StatisticsDbContext context)
        {
            var metaRows = GetMetaRows(metaFileCsvHeaders, metaFileRows);
            var filters = GetFilters(metaRows, subject, context).ToList();
            var indicators = GetIndicators(metaRows, subject, context).ToList();
            
            return new SubjectMeta
            {
                Filters = filters,
                Indicators = indicators
            };
        }

        private static List<MetaRow> GetMetaRows(
            List<string> cols,
            List<List<string>> metaFileRows)
        {
            return metaFileRows.Select(row => GetMetaRow(cols, row)).ToList();
        }

        public static MetaRow GetMetaRow(List<string> cols, List<string> rowValues)
        {
            return CsvUtils.BuildType(
                rowValues, 
                cols, 
                Enum.GetNames(typeof(MetaColumns)), 
                values => new MetaRow
                {
                    ColumnName = values[0],
                    ColumnType = Enum.Parse<ColumnType>(values[1]),
                    Label = values[2],
                    FilterGroupingColumn = values[3],
                    FilterHint = values[4],
                    IndicatorGrouping = values[5],
                    IndicatorUnit = EnumUtil.GetFromString<Unit>(values[6] ?? ""),
                    DecimalPlaces = values[7] == null ? null : int.Parse(values[7])
                });
        }

        private List<(Filter Filter, string Column, string FilterGroupingColumn)> ReadFiltersFromCsv(
            IEnumerable<MetaRow> metaRows, 
            Subject subject)
        {
            return metaRows
                .Where(row => row.ColumnType == ColumnType.Filter)
                .Select(filter => (
                    filter: new Filter(filter.FilterHint, filter.Label, filter.ColumnName, subject.Id),
                    column: filter.ColumnName,
                    filterGroupingColumn: filter.FilterGroupingColumn))
                .ToList();
        }

        private IEnumerable<(Filter Filter, string Column, string FilterGroupingColumn)> GetFilters(
            IEnumerable<MetaRow> metaRows, Subject subject, StatisticsDbContext context)
        {
            return metaRows
                .Where(row => row.ColumnType == ColumnType.Filter)
                .Select(filter => (
                    filter: context.Filter.Single(f => f.SubjectId == subject.Id && f.Name == filter.ColumnName),
                    column: filter.ColumnName,
                    filterGroupingColumn: filter.FilterGroupingColumn))
                .ToList();
        }

        private List<(Indicator Indicator, string Column)> ReadIndicatorsFromCsv(
            IEnumerable<MetaRow> metaRows,
            Subject subject)
        {
            var indicatorRows = metaRows.Where(row => row.ColumnType == ColumnType.Indicator).ToList();

            indicatorRows.ForEach(row =>
            {
                if (string.IsNullOrWhiteSpace(row.IndicatorGrouping))
                {
                    row.IndicatorGrouping = "Default";
                }
            });

            var indicatorGroups = indicatorRows
                .GroupBy(row => row.IndicatorGrouping)
                .ToDictionary(
                    rows => rows.Key, 
                    rows => new IndicatorGroup(rows.Key, subject.Id));

            return indicatorRows
                .Select(row =>
                {
                    var indicatorGroup = indicatorGroups.GetValueOrDefault(row.IndicatorGrouping)!;
                    
                    return (
                        indicator:
                        new Indicator
                        {
                            IndicatorGroup = indicatorGroup,
                            Label = row.Label,
                            Name = row.ColumnName,
                            Unit = row.IndicatorUnit,
                            DecimalPlaces = row.DecimalPlaces
                        },
                        column: row.ColumnName
                    );
                })
                .ToList();
        }

        private IEnumerable<(Indicator Indicator, string Column)> GetIndicators(IEnumerable<MetaRow> metaRows,
            Subject subject, StatisticsDbContext context)
        {
            var indicatorRows = metaRows.Where(row => row.ColumnType == ColumnType.Indicator).ToList();
            
            indicatorRows.ForEach(row =>
            {
                if (string.IsNullOrWhiteSpace(row.IndicatorGrouping))
                {
                    row.IndicatorGrouping = "Default";
                }
            });
            
            var indicatorGroups = indicatorRows
                .GroupBy(row => row.IndicatorGrouping)
                .ToDictionary(
                    rows => rows.Key, 
                    rows => context.IndicatorGroup.Single(ig => 
                        ig.SubjectId == subject.Id && ig.Label == rows.Key));
            
            return indicatorRows
                .Select(row =>
                {
                    var indicatorGroup = indicatorGroups.GetValueOrDefault(row.IndicatorGrouping)!;
                    
                    return (
                        indicator:
                        context.Indicator.Single(i =>
                            i.IndicatorGroupId == indicatorGroup.Id && i.Label == row.Label &&
                            i.Unit == row.IndicatorUnit),
                        column: row.ColumnName
                    );
                });
        }
    }
}
