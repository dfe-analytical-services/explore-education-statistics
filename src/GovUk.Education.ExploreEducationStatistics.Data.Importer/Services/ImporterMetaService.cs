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
                IndicatorGroups = ImportIndicatorGroups(metaRows, subject)
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

        private IEnumerable<(Filter Filter, string FilterGroupingColumn)> ImportFilters(
            IEnumerable<MetaRow> metaRows, Subject subject)
        {
            var filters = GetFilters(metaRows, subject);
            _context.Filter.AddRange(filters.Select(tuple => tuple.Filter));
            _context.SaveChanges();
            return filters;
        }

        private IEnumerable<IndicatorGroup> ImportIndicatorGroups(IEnumerable<MetaRow> metaRows, Subject subject)
        {
            var indicatorGroups = GetIndicatorGroups(metaRows, subject);
            _context.IndicatorGroup.AddRange(indicatorGroups);
            _context.SaveChanges();
            return indicatorGroups;
        }

        private static IEnumerable<(Filter Filter, string FilterGroupingColumn)> GetFilters(
            IEnumerable<MetaRow> metaRows, Subject subject)
        {
            return metaRows
                .Where(row => row.ColumnType == ColumnType.Filter)
                .Select(filter => (filter: new Filter
                {
                    Hint = filter.FilterHint,
                    Label = filter.Label,
                    Subject = subject
                }, filterGroupingColumn: filter.FilterGroupingColumn));
        }

        private static IEnumerable<IndicatorGroup> GetIndicatorGroups(IEnumerable<MetaRow> metaRows, Subject subject)
        {
            var indicatorGroups = metaRows
                .Where(row => row.ColumnType == ColumnType.Indicator)
                .GroupBy(row => row.IndicatorGrouping)
                .Select(group => new IndicatorGroup
                {
                    Indicators = group.ToList().Select(row => new Indicator
                    {
                        Label = row.Label,
                        Unit = row.IndicatorUnit
                    }).ToList(),
                    Label = group.Key,
                    Subject = subject
                }).ToList();

            indicatorGroups.ForEach(group =>
            {
                foreach (var indicator in group.Indicators)
                {
                    indicator.IndicatorGroup = group;
                }
            });

            return indicatorGroups;
        }
    }
}