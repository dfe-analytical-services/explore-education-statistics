using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;

namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Services
{
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
        public ImporterMetaService()
        {
        }

        public SubjectMeta Import(DataColumnCollection cols, DataRowCollection rows, Subject subject, StatisticsDbContext context)
        {
            var metaRows = GetMetaRows(CsvUtil.GetColumnValues(cols), rows);
            var filters = ImportFilters(metaRows, subject, context).ToList();
            var indicators = ImportIndicators(metaRows, subject, context).ToList();
            
            return new SubjectMeta
            {
                Filters = filters,
                Indicators = indicators
            };
        }

        public SubjectMeta Get(DataColumnCollection cols, DataRowCollection rows, Subject subject, StatisticsDbContext context)
        {
            var metaRows = GetMetaRows(CsvUtil.GetColumnValues(cols), rows);
            var filters = GetFilters(metaRows, subject, context).ToList();
            var indicators = GetIndicators(metaRows, subject, context).ToList();
            
            return new SubjectMeta
            {
                Filters = filters,
                Indicators = indicators
            };
        }

        private static IEnumerable<MetaRow> GetMetaRows(
            List<string> cols,
            DataRowCollection rows)
        {
            List<MetaRow> metaRows = new List<MetaRow>();
            foreach (DataRow row in rows)
            {
                metaRows.Add(GetMetaRow(cols, row));
            }

            return metaRows;
        }

        public static MetaRow GetMetaRow(List<string> cols, DataRow row)
        {
            return CsvUtil.BuildType(CsvUtil.GetRowValues(row), 
                cols, Enum.GetNames(typeof(MetaColumns)), values => new MetaRow
            {
                ColumnName = values[0],
                ColumnType = (ColumnType) Enum.Parse(typeof(ColumnType), values[1]),
                Label = values[2],
                FilterGroupingColumn = values[3],
                FilterHint = values[4],
                IndicatorGrouping = values[5],
                IndicatorUnit = EnumUtil.GetFromString<Unit>(values[6] ?? ""),
                DecimalPlaces = values[7] == null ? (int?) null : int.Parse(values[7])
            });
        }

        private IEnumerable<(Filter Filter, string Column, string FilterGroupingColumn)> ImportFilters(
            IEnumerable<MetaRow> metaRows, Subject subject, StatisticsDbContext context)
        {
            var filters = GetFilters(metaRows, subject, context).ToList();
            context.Filter.AddRange(filters.Select(triple => triple.Filter));

            return filters;
        }

        private IEnumerable<(Indicator Indicator, string Column)> ImportIndicators(IEnumerable<MetaRow> metaRows,
            Subject subject, StatisticsDbContext context)
        {
            var indicators = GetIndicators(metaRows, subject, context).ToList();

            context.Indicator.AddRange(indicators.Select(tuple => tuple.Indicator));

            return indicators;
        }

        private IEnumerable<(Filter Filter, string Column, string FilterGroupingColumn)> GetFilters(
            IEnumerable<MetaRow> metaRows, Subject subject, StatisticsDbContext context)
        {
            return metaRows
                .Where(row => row.ColumnType == ColumnType.Filter)
                .Select(filter => (
                    filter: 
                        context.Filter.FirstOrDefault(f => f.SubjectId == subject.Id && f.Name == filter.ColumnName) ??
                        new Filter(filter.FilterHint, filter.Label, filter.ColumnName, subject),
                    column: filter.ColumnName,
                    filterGroupingColumn: filter.FilterGroupingColumn));
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
                .ToDictionary(rows => rows.Key, rows =>
                    context.IndicatorGroup.FirstOrDefault(ig => ig.SubjectId == subject.Id && ig.Label == rows.Key) ??
                    new IndicatorGroup(rows.Key, subject)
                );

            return indicatorRows
                .Select(row =>
                {
                    indicatorGroups.TryGetValue(row.IndicatorGrouping, out var indicatorGroup);
                    return (
                        indicator:
                        context.Indicator.FirstOrDefault(i =>
                            i.IndicatorGroupId == indicatorGroup.Id && i.Label == row.Label &&
                            i.Unit == row.IndicatorUnit) ?? new Indicator
                        {
                            Id = Guid.NewGuid(),
                            IndicatorGroup = indicatorGroup,
                            Label = row.Label,
                            Name = row.ColumnName,
                            Unit = row.IndicatorUnit,
                            DecimalPlaces = row.DecimalPlaces
                        },
                        column: row.ColumnName
                    );
                });
        }
    }
}