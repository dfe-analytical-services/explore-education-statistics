using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;

namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Services
{
    public class ImporterMetaService
    {
        private readonly ApplicationDbContext _context;

        public ImporterMetaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public SubjectMeta Import(IEnumerable<string> lines, Subject subject)
        {
            var headers = lines.First().Split(',').ToList();
            var metaRows = lines
                .Skip(1)
                .Select(line => GetMetaRow(line, headers));

            return new SubjectMeta
            {
                Filters = ImportFilters(metaRows, subject),
                Indicators = ImportIndicators(metaRows, subject)
            };
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
                IndicatorUnit = values[6] == "%" ? Unit.Percent : Unit.Number
            });
        }

        private IEnumerable<(Filter Filter, string Column, string FilterGroupingColumn)> ImportFilters(
            IEnumerable<MetaRow> metaRows, Subject subject)
        {
            var filters = GetFilters(metaRows, subject);
            _context.Filter.AddRange(filters.Select(triple => triple.Filter));
            _context.SaveChanges();
            return filters;
        }

        private IEnumerable<(Indicator Indicator, string Column)> ImportIndicators(IEnumerable<MetaRow> metaRows,
            Subject subject)
        {
            var indicators = GetIndicators(metaRows, subject);
            _context.Indicator.AddRange(indicators.Select(tuple => tuple.Indicator));
            _context.SaveChanges();
            return indicators;
        }

        private static IEnumerable<(Filter Filter, string Column, string FilterGroupingColumn)> GetFilters(
            IEnumerable<MetaRow> metaRows, Subject subject)
        {
            return metaRows
                .Where(row => row.ColumnType == ColumnType.Filter)
                .Select(filter => (
                    filter: new Filter
                    {
                        Hint = filter.FilterHint,
                        Label = filter.Label,
                        Subject = subject
                    },
                    column: filter.ColumnName,
                    filterGroupingColumn: filter.FilterGroupingColumn));
        }

        private static IEnumerable<(Indicator Indicator, string Column)> GetIndicators(IEnumerable<MetaRow> metaRows,
            Subject subject)
        {
            var indicatorRows = metaRows.Where(row => row.ColumnType == ColumnType.Indicator);

            var indicatorGroups = indicatorRows
                .GroupBy(row => row.IndicatorGrouping)
                .ToDictionary(rows => rows.Key, rows => new IndicatorGroup
                {
                    Label = rows.Key,
                    Subject = subject
                });

            return indicatorRows
                .Select(row =>
                {
                    indicatorGroups.TryGetValue(row.IndicatorGrouping, out var indicatorGroup);
                    return (
                        indicator: new Indicator
                        {
                            IndicatorGroup = indicatorGroup,
                            Label = row.Label,
                            Unit = row.IndicatorUnit
                        },
                        column: row.ColumnName
                    );
                });
        }
    }
}