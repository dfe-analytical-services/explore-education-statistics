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

        public SubjectMeta Import(IEnumerable<string> lines, Subject subject, bool existingSubject)
        {
            var headers = lines.First().Split(',').ToList();
            var metaRows = lines
                .Skip(1)
                .Select(line => GetMetaRow(line, headers));

            return new SubjectMeta
            {
                Filters = ImportFilters(metaRows, subject, existingSubject),
                Indicators = ImportIndicators(metaRows, subject, existingSubject)
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
            IEnumerable<MetaRow> metaRows, Subject subject, bool existingSubject)
        {
            var filters = GetFilters(metaRows, subject).ToList();
            
            // Persist for a new subject
            if (!existingSubject)
            {
                _context.Filter.AddRange(filters.Select(triple => triple.Filter));
                _context.SaveChanges();
            }

            return filters;
        }

        private IEnumerable<(Indicator Indicator, string Column)> ImportIndicators(IEnumerable<MetaRow> metaRows,
            Subject subject, bool existingSubject)
        {
            var indicators = GetIndicators(metaRows, subject).ToList();
            
            // Persist for a new subject
            if (!existingSubject)
            {
                _context.Indicator.AddRange(indicators.Select(tuple => tuple.Indicator));
                _context.SaveChanges();
            }

            return indicators;
        }

        private IEnumerable<(Filter Filter, string Column, string FilterGroupingColumn)> GetFilters(
            IEnumerable<MetaRow> metaRows, Subject subject)
        {
            return metaRows
                .Where(row => row.ColumnType == ColumnType.Filter)
                .Select(filter => (
                    filter: 
                        _context.Filter.FirstOrDefault(f => f.SubjectId == subject.Id && f.Label == filter.Label && f.Hint == filter.FilterHint) ??
                        new Filter(filter.FilterHint, filter.Label, subject),
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
                _context.IndicatorGroup.Any(ig => ig.SubjectId == subject.Id && ig.Label == rows.Key) ?
                _context.IndicatorGroup.First(ig => ig.SubjectId == subject.Id && ig.Label == rows.Key) : 
                new IndicatorGroup(rows.Key, subject)
                );

            return indicatorRows
                .Select(row =>
                {
                    indicatorGroups.TryGetValue(row.IndicatorGrouping, out var indicatorGroup);
                    return (
                        
                        indicator: 
                            _context.Indicator.Any(i => i.IndicatorGroupId == indicatorGroup.Id && i.Label == row.Label && i.Unit == row.IndicatorUnit) ?
                            _context.Indicator.First(i => i.IndicatorGroupId == indicatorGroup.Id && i.Label == row.Label && i.Unit == row.IndicatorUnit) : 
                            new Indicator
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