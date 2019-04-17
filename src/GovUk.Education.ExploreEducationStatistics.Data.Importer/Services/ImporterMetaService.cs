using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Services
{
    public class ImporterMetaService
    {
        public SubjectMeta Import(IEnumerable<string> metaLines)
        {
            var headers = metaLines.First().Split(',').ToList();
            var lines = metaLines
                .Skip(1)
                .Select(line => GetMetaRow(line, headers));

            return new SubjectMeta
            {
                Filters = GetFilters(lines),
                IndicatorGroups = GetIndicatorGroups(lines)
            };
        }

        private static MetaRow GetMetaRow(string raw, List<string> headers)
        {
            var line = raw.Split(',');

            var columns = new[]
            {
                "col_name", "col_type", "label", "filter_grouping_column", "filter_hint", "indicator_grouping",
                "indicator_unit"
            };

            return CsvUtil.BuildType(line, headers, columns, values => new MetaRow
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

        private static IEnumerable<Filter> GetFilters(IEnumerable<MetaRow> lines)
        {
            return lines
                .Where(row => row.ColumnType == ColumnType.Filter)
                .Select(filter => new Filter
                {
                    Label = filter.Label,
                    Hint = filter.FilterHint
                });
        }

        private static IEnumerable<IndicatorGroup> GetIndicatorGroups(IEnumerable<MetaRow> lines)
        {
            return lines
                .Where(row => row.ColumnType == ColumnType.Indicator)
                .GroupBy(row => row.IndicatorGrouping)
                .Select(group => new IndicatorGroup
                {
                    Indicators = group.ToList().Select(row => new Indicator
                    {
                        Label = row.Label,
                        Unit = row.IndicatorUnit
                    }),
                    Label = group.Key
                });
        }
    }
}