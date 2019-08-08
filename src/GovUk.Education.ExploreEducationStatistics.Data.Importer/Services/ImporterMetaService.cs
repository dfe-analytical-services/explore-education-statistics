using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;

namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Services
{
    public class ImporterMetaService : IImporterMetaService
    {
        private readonly ApplicationDbContext _context;

        public ImporterMetaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public SubjectMeta Import(IEnumerable<string> lines, Subject subject)
        {
            var headers = GetHeaders(lines);
            var metaRows = GetMetaRows(lines, headers);

            return new SubjectMeta
            {
                Filters = ImportFilters(metaRows, subject),
                Indicators = ImportIndicators(metaRows, subject)
            };
        }

        public SubjectMeta Get(IEnumerable<string> lines, Subject subject)
        {
            var headers = GetHeaders(lines);
            var metaRows = GetMetaRows(lines, headers);

            return new SubjectMeta
            {
                Filters = GetFilters(metaRows, subject),
                Indicators = GetIndicators(metaRows, subject)
            };
        }

        private static List<string> GetHeaders(IEnumerable<string> lines)
        {
            return lines.First().Split(',').ToList();
        }

        private static IEnumerable<MetaRow> GetMetaRows(IEnumerable<string> lines, List<string> headers)
        {
            return lines
                .Skip(1)
                .Select(line => GetMetaRow(line, headers));
        }

        private static MetaRow GetMetaRow(string line, List<string> headers)
        {
            var columns = new[]
            {
                "col_name",
                "col_type",
                "label",
                "filter_grouping_column",
                "filter_hint",
                "indicator_grouping",
                "indicator_unit"
            };

            return CsvUtil.BuildType(line.Split(','), headers, columns, values => new MetaRow
            {
                ColumnName = values[0],
                ColumnType = (ColumnType) Enum.Parse(typeof(ColumnType), values[1]),
                Label = values[2],
                FilterGroupingColumn = values[3],
                FilterHint = values[4],
                IndicatorGrouping = values[5],
                IndicatorUnit = EnumUtil.GetFromString<Unit>(values[6] ?? "")
            });
        }

        private IEnumerable<(Filter Filter, string Column, string FilterGroupingColumn)> ImportFilters(
            IEnumerable<MetaRow> metaRows, Subject subject)
        {
            var filters = GetFilters(metaRows, subject).ToList();
            _context.Filter.AddRange(filters.Select(triple => triple.Filter));

            return filters;
        }

        private IEnumerable<(Indicator Indicator, string Column)> ImportIndicators(IEnumerable<MetaRow> metaRows,
            Subject subject)
        {
            var indicators = GetIndicators(metaRows, subject).ToList();

            _context.Indicator.AddRange(indicators.Select(tuple => tuple.Indicator));

            return indicators;
        }

        private IEnumerable<(Filter Filter, string Column, string FilterGroupingColumn)> GetFilters(
            IEnumerable<MetaRow> metaRows, Subject subject)
        {
            return metaRows
                .Where(row => row.ColumnType == ColumnType.Filter)
                .Select(filter => (
                    filter: 
                        _context.Filter.FirstOrDefault(f => f.SubjectId == subject.Id && f.Name == filter.ColumnName) ??
                        new Filter(filter.FilterHint, filter.Label, filter.ColumnName, subject),
                    column: filter.ColumnName,
                    filterGroupingColumn: filter.FilterGroupingColumn));
        }

        private IEnumerable<(Indicator Indicator, string Column)> GetIndicators(IEnumerable<MetaRow> metaRows,
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
                .ToDictionary(rows => rows.Key, rows =>
                    _context.IndicatorGroup.FirstOrDefault(ig => ig.SubjectId == subject.Id && ig.Label == rows.Key) ??
                    new IndicatorGroup(rows.Key, subject)
                );

            return indicatorRows
                .Select(row =>
                {
                    indicatorGroups.TryGetValue(row.IndicatorGrouping, out var indicatorGroup);
                    return (
                        indicator:
                        _context.Indicator.FirstOrDefault(i =>
                            i.IndicatorGroupId == indicatorGroup.Id && i.Label == row.Label &&
                            i.Unit == row.IndicatorUnit) ?? new Indicator
                        {
                            IndicatorGroup = indicatorGroup,
                            Label = row.Label,
                            Name = row.ColumnName,
                            Unit = row.IndicatorUnit
                        },
                        column: row.ColumnName
                    );
                });
        }
    }
}