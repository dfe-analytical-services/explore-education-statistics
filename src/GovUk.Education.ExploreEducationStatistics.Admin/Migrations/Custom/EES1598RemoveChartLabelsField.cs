using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.Custom
{
    public class EES1598RemoveChartLabelsField : ICustomMigration
    {
        private readonly ContentDbContext _context;
        private readonly bool _isTest;

        public EES1598RemoveChartLabelsField(ContentDbContext context, bool isTest = false)
        {
            _context = context;
            _isTest = isTest;
        }

        public void Apply()
        {
            var dataBlocks = GetQuery();

            foreach (var dataBlock in dataBlocks)
            {
                var chart = dataBlock.Charts.ElementAtOrDefault(0);

                // No labels, this is data block is
                // already in the correct format.
                if (chart == null || chart.Type == ChartType.Infographic || chart.Labels == null)
                {
                    continue;
                }

                foreach (var (key, config) in chart.Labels)
                {
                    // The key is a junk label so we can ignore it.
                    // We use a length of 37 as this is a single
                    // indicator guid and ____ concatenated
                    // (the bare minimum to be considered a data set).
                    if (key.Length < 37 || !key.EndsWith("_____"))
                    {
                        continue;
                    }

                    var labelDataSet = ParseLabelDataSet(key, config);
                    MigrateLabelDataSet(chart, labelDataSet);
                }

                chart.Labels = null;

                _context.DataBlocks.Update(dataBlock);
            }

            _context.SaveChanges();
        }

        private IQueryable<DataBlock> GetQuery()
        {
            if (_isTest)
            {
                return _context.DataBlocks.Where(block => block.Charts != null);
            }

            return _context.DataBlocks.FromSqlRaw(
                @"
                SELECT *
                FROM ContentBlock
                WHERE Type = 'DataBlock'
                  AND JSON_VALUE(DataBlock_Charts, '$[0].Type') != 'infographic'
                  AND JSON_QUERY(DataBlock_Charts, '$[0].Labels') IS NOT NULL");
        }

        private ChartDataSet ParseLabelDataSet(string key, ChartLabelConfiguration labelConfig)
        {
            var parts = key.Split('_');

            var indicatorFilters = parts.SkipLast(5).ToList();
            var indicator = indicatorFilters.First();
            var filters = indicatorFilters.Skip(1);

            return new ChartDataSet
            {
                Config = new ChartDataSetConfiguration
                {
                    Colour = labelConfig.Colour,
                    Label = labelConfig.Label,
                    Symbol = labelConfig.Symbol,
                    LineStyle = labelConfig.LineStyle
                },
                Filters = filters.Select(Guid.Parse).ToList(),
                Indicator = Guid.Parse(indicator),
            };
        }

        private void MigrateLabelDataSet(IChart chart, ChartDataSet labelDataSet)
        {
            // This chart has no major axis for some reason so we can ignore it
            if (chart.Axes?.GetValueOrDefault("major") == null)
            {
                return;
            }

            chart.Axes["major"].DataSets ??= new List<ChartDataSet>();

            var majorAxisDataSets = chart.Axes["major"].DataSets;

            var matchingDataSet = majorAxisDataSets
                .FirstOrDefault(
                    dataSet => dataSet.Filters.TrueForAll(filter => labelDataSet.Filters.Contains(filter))
                               && dataSet.Indicator == labelDataSet.Indicator
                );

            if (matchingDataSet != null)
            {
                matchingDataSet.Config ??= labelDataSet.Config;
            }
            else
            {
                majorAxisDataSets.Add(labelDataSet);
            }
        }
    }
}