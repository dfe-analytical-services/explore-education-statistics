using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum MetaColumns
    {
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
            var metaFileReader = new MetaDataFileReader(metaFileCsvHeaders);
            var metaRows = metaFileReader.GetMetaRows(metaFileRows);
            var filtersAndMeta = metaFileReader.ReadFiltersFromCsv(metaRows, subject);
            var indicatorsAndMeta = metaFileReader.ReadIndicatorsFromCsv(metaRows, subject);

            var filtersAlreadyImported = filtersAndMeta.Count > 0 &&
                                         await context.Filter.AnyAsync(filter => filter.SubjectId == subject.Id);

            var indicatorsAlreadyImported = indicatorsAndMeta.Count > 0 &&
                                            await context.IndicatorGroup.AnyAsync(indicator =>
                                                indicator.SubjectId == subject.Id);

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

            return await GetSubjectMeta(metaFileCsvHeaders, metaFileRows, subject, context);
        }

        public async Task<SubjectMeta> GetSubjectMeta(
            List<string> metaFileCsvHeaders,
            List<List<string>> metaFileRows,
            Subject subject,
            StatisticsDbContext context)
        {
            var metaFileReader = new MetaDataFileReader(metaFileCsvHeaders);
            var metaRows = metaFileReader.GetMetaRows(metaFileRows);
            var filters = (await GetFilters(metaRows, subject, context)).ToList();
            var indicators = GetIndicators(metaRows, subject, context).ToList();

            return new SubjectMeta
            {
                Filters = filters,
                Indicators = indicators
            };
        }

        private async Task<IEnumerable<(Filter Filter, string Column, string FilterGroupingColumn)>> GetFilters(
            IEnumerable<MetaRow> metaRows, Subject subject, StatisticsDbContext context)
        {
            var filters = await context
                .Filter
                .AsNoTracking()
                .Include(filter => filter.FilterGroups)
                .ThenInclude(group => group.FilterItems)
                .Where(filter => filter.SubjectId == subject.Id)
                .ToListAsync();
            
            return metaRows
                .Where(row => row.ColumnType == ColumnType.Filter)
                .Select(filter => (
                    filter: filters.Single(f => f.Name == filter.ColumnName),
                    column: filter.ColumnName,
                    filterGroupingColumn: filter.FilterGroupingColumn))
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