#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using IReleaseVersionService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseVersionService;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

    public class ReplacementService(
        ContentDbContext contentDbContext,
        StatisticsDbContext statisticsDbContext,
        IFilterRepository filterRepository,
        IIndicatorGroupRepository indicatorGroupRepository,
        IReleaseVersionService releaseVersionService,
        IReleaseFileRepository releaseFileRepository,
        IReplacementPlanService replacementPlanService,
        ICacheKeyService cacheKeyService,
        IPrivateBlobCacheService privateCacheService)
        : IReplacementService
    {
        public async Task<Either<ActionResult, Unit>> Replace(
                Guid releaseVersionId,
                Guid originalFileId,
                CancellationToken cancellationToken = default)
        {
            return await releaseFileRepository.CheckLinkedOriginalAndReplacementReleaseFilesExist(
                    releaseVersionId: releaseVersionId,
                    originalFileId: originalFileId)
                .OnSuccessCombineWith(async releaseFiles =>
                {
                    var originalReleaseFile = releaseFiles.originalReleaseFile;
                    var replacementReleaseFile = releaseFiles.replacementReleaseFile;

                    return await replacementPlanService.GenerateReplacementPlan(
                        originalReleaseFile: originalReleaseFile,
                        replacementReleaseFile: replacementReleaseFile,
                        cancellationToken: cancellationToken);
                })
                .OnSuccess(releaseFilesAndPlan =>
                {
                    var ((originalReleaseFile, replacementReleaseFile), plan) = releaseFilesAndPlan;
                    if (!plan.Valid)
                    {
                        return new Either<ActionResult,
                            (ReleaseFile originalReleaseFile,
                            ReleaseFile replacementReleaseFile,
                            DataReplacementPlanViewModel plan)>(
                            ValidationActionResult(ReplacementMustBeValid));
                    }

                    var replacementImportHasCompleted = contentDbContext.DataImports
                        .Any(import =>
                            import.FileId == replacementReleaseFile.FileId
                            && import.Status == DataImportStatus.COMPLETE);
                    if (!replacementImportHasCompleted)
                    {
                        return ValidationActionResult(ReplacementImportMustBeComplete);
                    }

                    return (originalReleaseFile, replacementReleaseFile, plan);
                })
                .OnSuccess(async releaseFilesAndPlan =>
                {
                    var originalReleaseFile = releaseFilesAndPlan.originalReleaseFile;
                    var replacementReleaseFile = releaseFilesAndPlan.replacementReleaseFile;
                    var plan = releaseFilesAndPlan.plan;

                    contentDbContext.Update(originalReleaseFile);
                    contentDbContext.Update(replacementReleaseFile);

                    var originalSubjectId = plan.OriginalSubjectId;
                    var replacementSubjectId = plan.ReplacementSubjectId;

                    await plan.DataBlocks
                        .ToAsyncEnumerable()
                        .ForEachAwaitAsync(async dataBlockPlan =>
                        {
                            await InvalidateDataBlockCachedResults(dataBlockPlan, releaseVersionId);
                            await ReplaceLinksForDataBlock(dataBlockPlan, replacementSubjectId);
                        }, cancellationToken);

                    await plan.Footnotes
                        .ToAsyncEnumerable()
                        .ForEachAwaitAsync(footnotePlan =>
                            ReplaceLinksForFootnote(footnotePlan, originalSubjectId, replacementSubjectId),
                            cancellationToken);

                    replacementReleaseFile.FilterSequence =
                        await ReplaceFilterSequence(originalReleaseFile, replacementReleaseFile);
                    replacementReleaseFile.IndicatorSequence =
                        await ReplaceIndicatorSequence(originalReleaseFile, replacementReleaseFile);
                    replacementReleaseFile.Summary = originalReleaseFile.Summary; // Set Data guidance

                    // To remove original, we first unlink the files. If we don't do this,
                    // ReleaseVersionService.RemoveDataFiles will remove the replacement file as well!
                    contentDbContext.Update(originalReleaseFile.File);
                    contentDbContext.Update(replacementReleaseFile.File);
                    originalReleaseFile.File.ReplacedById = null;
                    replacementReleaseFile.File.ReplacingId = null;

                    await contentDbContext.SaveChangesAsync(cancellationToken);
                    await statisticsDbContext.SaveChangesAsync(cancellationToken); // For footnotes

                    return await releaseVersionService.RemoveDataFiles(
                        releaseVersionId: releaseVersionId,
                        fileId: originalReleaseFile.FileId);
                });
        }

        private async Task ReplaceLinksForDataBlock(DataBlockReplacementPlanViewModel replacementPlan,
            Guid replacementSubjectId)
        {
            var dataBlock = await contentDbContext.ContentBlocks
                .AsQueryable()
                .OfType<DataBlock>()
                .SingleAsync(block => block.Id == replacementPlan.Id);

            contentDbContext.Update(dataBlock);

            dataBlock.Query.SubjectId = replacementSubjectId;

            var filterItemTargets = replacementPlan.Filters
                .SelectMany(filter =>
                    filter.Value.Groups.SelectMany(group => group.Value.Filters))
                .ToDictionary(ReplacementPlanOriginalId, ReplacementPlanTargetId);

            ReplaceDataBlockQueryFilters(filterItemTargets, dataBlock);

            var filterTargets = replacementPlan.Filters
                .Where(plan => plan.Value.Target != null)
                .ToDictionary(plan => plan.Value.Id, plan => plan.Value.Target!.Value);

            ReplaceDataBlockQueryFilterHierarchiesOptions(filterTargets, filterItemTargets, dataBlock);
            ReplaceDataBlockQueryIndicators(replacementPlan, dataBlock);
            ReplaceDataBlockQueryLocations(replacementPlan, dataBlock);

            var indicatorTargets = replacementPlan.IndicatorGroups
                .SelectMany(group => group.Value.Indicators)
                .ToDictionary(ReplacementPlanOriginalId, ReplacementPlanTargetId);
            var locationTargets = replacementPlan.Locations
                .Values
                .SelectMany(group => group.LocationAttributes)
                .ToDictionary(ReplacementPlanOriginalId, ReplacementPlanTargetId);

            ReplaceDataBlockTableHeaders(
                filterItemTargets: filterItemTargets,
                indicatorTargets: indicatorTargets,
                locationTargets: locationTargets,
                dataBlock);
            ReplaceDataBlockCharts(
                filterItemTargets: filterItemTargets,
                indicatorTargets: indicatorTargets,
                locationTargets: locationTargets,
                dataBlock);
        }

        private static void ReplaceDataBlockQueryFilters(
            Dictionary<Guid, Guid> filterItemTargets,
            DataBlock dataBlock)
        {
            var originalFilterItemIds = dataBlock.Query.GetNonHierarchicalFilterItemIds();
            dataBlock.Query.Filters = originalFilterItemIds
                .Select(originalFilterItemId => filterItemTargets[originalFilterItemId])
                .ToList();
        }

        private static void ReplaceDataBlockQueryFilterHierarchiesOptions(
            Dictionary<Guid, Guid> filterTargets,
            Dictionary<Guid, Guid> filterItemTargets,
            DataBlock dataBlock)
        {
            var originalFilterHierarchiesOptions = dataBlock.Query.FilterHierarchiesOptions;
            if (originalFilterHierarchiesOptions is null || originalFilterHierarchiesOptions.Count == 0)
            {
                dataBlock.Query.FilterHierarchiesOptions = null;
                return;
            }

            var newFilterHierarchiesOptions = new List<FilterHierarchyOptions>();

            foreach (var originalHierarchyOptions in originalFilterHierarchiesOptions) // for each filter hierarchy
            {
                var originalOptions = originalHierarchyOptions.Options;
                var newOptions = originalOptions
                        .Select(originalOption =>
                            originalOption // a filter hierarchy option is a list of filterItemIds, one per filter/tier
                                .Select(originalFilterItemId => filterItemTargets[originalFilterItemId])
                                .ToList())
                        .ToList();

                var replacementFilterId = filterTargets[originalHierarchyOptions.LeafFilterId];
                newFilterHierarchiesOptions.Add(new FilterHierarchyOptions
                {
                    LeafFilterId = replacementFilterId,
                    Options = newOptions,
                });
            }

            dataBlock.Query.FilterHierarchiesOptions = newFilterHierarchiesOptions;
        }

        private static void ReplaceDataBlockQueryIndicators(DataBlockReplacementPlanViewModel replacementPlan,
            DataBlock dataBlock)
        {
            var indicators = dataBlock.Query.Indicators.ToList();

            replacementPlan.IndicatorGroups
                .SelectMany(group => group.Value.Indicators)
                .ToList()
                .ForEach(plan =>
                {
                    indicators.Remove(plan.Id);
                    indicators.Add(plan.TargetValue);
                });

            dataBlock.Query.Indicators = indicators;
        }

        private static void ReplaceDataBlockQueryLocations(DataBlockReplacementPlanViewModel replacementPlan,
            DataBlock dataBlock)
        {
            var locations = dataBlock.Query.LocationIds.ToList();

            replacementPlan.Locations
                .Values
                .SelectMany(group => group.LocationAttributes)
                .ToList()
                .ForEach(plan =>
                {
                    locations.Remove(plan.Id);
                    locations.Add(plan.TargetValue);
                });

            dataBlock.Query.LocationIds = locations;
        }

        private static void ReplaceDataBlockTableHeaders(
            IReadOnlyDictionary<Guid, Guid> filterItemTargets,
            IReadOnlyDictionary<Guid, Guid> indicatorTargets,
            IReadOnlyDictionary<Guid, Guid> locationTargets,
            DataBlock dataBlock)
        {
            var tableHeaders = dataBlock.Table.TableHeaders;

            // Replace Columns
            ReplaceDataBlockTableHeaders(
                tableHeaders.Columns.FilterByType(TableHeaderType.Filter), dataBlock, filterItemTargets);
            ReplaceDataBlockTableHeaders(
                tableHeaders.Columns.FilterByType(TableHeaderType.Indicator), dataBlock, indicatorTargets);
            ReplaceDataBlockTableHeaders(
                tableHeaders.Columns.FilterByType(TableHeaderType.Location), dataBlock, locationTargets);

            // Replace Rows
            ReplaceDataBlockTableHeaders(
                tableHeaders.Rows.FilterByType(TableHeaderType.Filter), dataBlock, filterItemTargets);
            ReplaceDataBlockTableHeaders(
                tableHeaders.Rows.FilterByType(TableHeaderType.Indicator), dataBlock, indicatorTargets);
            ReplaceDataBlockTableHeaders(
                tableHeaders.Rows.FilterByType(TableHeaderType.Location), dataBlock, locationTargets);

            // Replace Column Groups
            tableHeaders.ColumnGroups.ForEach(group =>
            {
                ReplaceDataBlockTableHeaders(
                    group.FilterByType(TableHeaderType.Filter), dataBlock, filterItemTargets);

                ReplaceDataBlockTableHeaders(
                    group.FilterByType(TableHeaderType.Indicator), dataBlock, indicatorTargets);

                ReplaceDataBlockTableHeaders(
                    group.FilterByType(TableHeaderType.Location), dataBlock, locationTargets);
            });

            // Replace Row Groups
            tableHeaders.RowGroups.ForEach(group =>
            {
                ReplaceDataBlockTableHeaders(
                    group.FilterByType(TableHeaderType.Filter), dataBlock, filterItemTargets);

                ReplaceDataBlockTableHeaders(
                    group.FilterByType(TableHeaderType.Indicator), dataBlock, indicatorTargets);

                ReplaceDataBlockTableHeaders(
                    group.FilterByType(TableHeaderType.Location), dataBlock, locationTargets);
            });
        }

        private static void ReplaceDataBlockTableHeaders(
            List<TableHeader> tableHeaders,
            DataBlock dataBlock,
            IReadOnlyDictionary<Guid, Guid> targets)
        {
            foreach (var tableHeader in tableHeaders)
            {
                if (Guid.TryParse(tableHeader.Value, out var idAsGuid))
                {
                    if (targets.TryGetValue(idAsGuid, out var targetId))
                    {
                        tableHeader.Value = targetId.ToString();
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            $"Expected target replacement value for dataBlock {dataBlock.Id} {tableHeader.Type} table header value: {idAsGuid}");
                    }
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Expected Guid for dataBlock {dataBlock.Id} {tableHeader.Type} table header value but found: {tableHeader.Value}");
                }
            }
        }

        private static void ReplaceDataBlockCharts(
            IReadOnlyDictionary<Guid, Guid> filterItemTargets,
            IReadOnlyDictionary<Guid, Guid> indicatorTargets,
            IReadOnlyDictionary<Guid, Guid> locationTargets,
            DataBlock dataBlock)
        {
            dataBlock.Charts.ForEach(
                chart =>
                {
                    ReplaceChartMajorAxisDataSets(
                        filterItemTargets: filterItemTargets,
                        indicatorTargets: indicatorTargets,
                        locationTargets: locationTargets,
                        dataBlock,
                        chart);
                    ReplaceChartLegendDataSets(
                        filterItemTargets: filterItemTargets,
                        indicatorTargets: indicatorTargets,
                        locationTargets: locationTargets,
                        dataBlock,
                        chart);
                    if (chart is MapChart mapChart)
                    {
                        ReplaceMapChartDataSetConfigs(
                            filterItemTargets: filterItemTargets,
                            indicatorTargets: indicatorTargets,
                            locationTargets: locationTargets,
                            dataBlock,
                            mapChart);
                    }
                }
            );
        }

        private static void ReplaceMapChartDataSetConfigs(
            IReadOnlyDictionary<Guid, Guid> filterItemTargets,
            IReadOnlyDictionary<Guid, Guid> indicatorTargets,
            IReadOnlyDictionary<Guid, Guid> locationTargets,
            DataBlock dataBlock,
            MapChart mapChart)
        {
            mapChart.Map.DataSetConfigs.ForEach(
                dataSetConfig =>
                {
                    var dataSet = dataSetConfig.DataSet;

                    dataSet.Filters = dataSet.Filters.Select(
                        filter =>
                        {
                            if (filterItemTargets.TryGetValue(filter, out var targetFilterId))
                            {
                                return targetFilterId;
                            }

                            throw new InvalidOperationException(
                                $"Expected target replacement value for dataBlock {dataBlock.Id} chart data set config filter: {filter}"
                            );
                        }
                    ).ToList();

                    if (dataSet.Indicator.HasValue
                        && indicatorTargets.TryGetValue(dataSet.Indicator.Value, out var targetIndicatorId))
                    {
                        dataSet.Indicator = targetIndicatorId;
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            $"Expected target replacement value for dataBlock {dataBlock.Id} chart data set config indicator: {dataSet.Indicator}"
                        );
                    }

                    if (dataSet.Location != null)
                    {
                        if (locationTargets.TryGetValue(dataSet.Location.Value, out var targetLocationId))
                        {
                            dataSet.Location.Value = targetLocationId;
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                $"Expected target replacement value for dataBlock {dataBlock.Id} chart data set config location: {dataSet.Location.Value}"
                            );
                        }
                    }
                });
        }

        private static void ReplaceChartLegendDataSets(
            IReadOnlyDictionary<Guid, Guid> filterItemTargets,
            IReadOnlyDictionary<Guid, Guid> indicatorTargets,
            IReadOnlyDictionary<Guid, Guid> locationTargets,
            DataBlock dataBlock,
            IChart chart)
        {
            chart.Legend?.Items.ForEach(
                item =>
                {
                    var dataSet = item.DataSet;

                    dataSet.Filters = dataSet.Filters.Select(
                        filter =>
                        {
                            if (filterItemTargets.TryGetValue(filter, out var targetFilterId))
                            {
                                return targetFilterId;
                            }

                            throw new InvalidOperationException(
                                $"Expected target replacement value for dataBlock {dataBlock.Id} chart legend data set filter: {filter}"
                            );
                        }
                    ).ToList();


                    if (dataSet.Indicator.HasValue)
                    {
                        if (indicatorTargets.TryGetValue(dataSet.Indicator.Value, out var targetIndicatorId))
                        {
                            dataSet.Indicator = targetIndicatorId;
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                $"Expected target replacement value for dataBlock {dataBlock.Id} chart legend data set indicator: {dataSet.Indicator}"
                            );
                        }
                    }

                    if (dataSet.Location != null)
                    {
                        if (locationTargets.TryGetValue(dataSet.Location.Value, out var targetLocationId))
                        {
                            dataSet.Location.Value = targetLocationId;
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                $"Expected target replacement value for dataBlock {dataBlock.Id} chart legend data set location: {dataSet.Location.Value}"
                            );
                        }
                    }
                }
            );
        }

        private static void ReplaceChartMajorAxisDataSets(
            IReadOnlyDictionary<Guid, Guid> filterItemTargets,
            IReadOnlyDictionary<Guid, Guid> indicatorTargets,
            IReadOnlyDictionary<Guid, Guid> locationTargets,
            DataBlock dataBlock,
            IChart chart)
        {
            chart.Axes?.GetValueOrDefault("major")?.DataSets.ForEach(
                dataSet =>
                {
                    dataSet.Filters = dataSet.Filters.Select(
                        filter =>
                        {
                            if (filterItemTargets.TryGetValue(filter, out var targetFilterId))
                            {
                                return targetFilterId;
                            }

                            throw new InvalidOperationException(
                                $"Expected target replacement value for dataBlock {dataBlock.Id} chart data set filter: {filter}"
                            );
                        }
                    ).ToList();

                    if (dataSet.Indicator.HasValue
                        && indicatorTargets.TryGetValue(dataSet.Indicator.Value, out var targetIndicatorId))
                    {
                        dataSet.Indicator = targetIndicatorId;
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            $"Expected target replacement value for dataBlock {dataBlock.Id} chart data set indicator: {dataSet.Indicator}"
                        );
                    }

                    if (dataSet.Location != null)
                    {
                        if (locationTargets.TryGetValue(dataSet.Location.Value, out var targetLocationId))
                        {
                            dataSet.Location.Value = targetLocationId;
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                $"Expected target replacement value for dataBlock {dataBlock.Id} chart data set location: {dataSet.Location.Value}"
                            );
                        }
                    }
                }
            );
        }

        private async Task ReplaceLinksForFootnote(FootnoteReplacementPlanViewModel replacementPlan,
            Guid originalSubjectId,
            Guid replacementSubjectId)
        {
            await ReplaceFootnoteSubject(replacementPlan.Id, originalSubjectId, replacementSubjectId);

            await replacementPlan.Filters
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(async plan =>
                    await ReplaceFootnoteFilter(replacementPlan.Id, plan));

            await replacementPlan.FilterGroups
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(async plan =>
                    await ReplaceFootnoteFilterGroup(replacementPlan.Id, plan));

            await replacementPlan.FilterItems
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(async plan =>
                    await ReplaceFootnoteFilterItem(replacementPlan.Id, plan));

            await replacementPlan.IndicatorGroups
                .SelectMany(group => group.Value.Indicators)
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(async plan =>
                    await ReplaceIndicatorFootnote(replacementPlan.Id, plan));
        }

        private async Task ReplaceFootnoteSubject(Guid footnoteId, Guid originalSubjectId, Guid replacementSubjectId)
        {
            var subjectFootnote = await statisticsDbContext.SubjectFootnote
                .AsQueryable()
                .Where(f =>
                    f.FootnoteId == footnoteId && f.SubjectId == originalSubjectId).SingleOrDefaultAsync();

            if (subjectFootnote != null)
            {
                statisticsDbContext.Remove(subjectFootnote);
                await statisticsDbContext.SubjectFootnote.AddAsync(new SubjectFootnote
                {
                    FootnoteId = footnoteId,
                    SubjectId = replacementSubjectId
                });
            }
        }

        private async Task ReplaceFootnoteFilter(Guid footnoteId, TargetableReplacementViewModel plan)
        {
            var filterFootnote = await statisticsDbContext.FilterFootnote
                .AsQueryable()
                .SingleAsync(f =>
                    f.FootnoteId == footnoteId && f.FilterId == plan.Id
                );

            statisticsDbContext.Remove(filterFootnote);
            await statisticsDbContext.FilterFootnote.AddAsync(new FilterFootnote
            {
                FootnoteId = footnoteId,
                FilterId = plan.TargetValue
            });
        }

        private async Task ReplaceFootnoteFilterGroup(Guid footnoteId, TargetableReplacementViewModel plan)
        {
            var filterGroupFootnote = await statisticsDbContext.FilterGroupFootnote
                .AsQueryable()
                .SingleAsync(f =>
                    f.FootnoteId == footnoteId && f.FilterGroupId == plan.Id
                );

            statisticsDbContext.Remove(filterGroupFootnote);
            await statisticsDbContext.FilterGroupFootnote.AddAsync(new FilterGroupFootnote
            {
                FootnoteId = footnoteId,
                FilterGroupId = plan.TargetValue
            });
        }

        private async Task ReplaceFootnoteFilterItem(Guid footnoteId, TargetableReplacementViewModel plan)
        {
            var filterItemFootnote = await statisticsDbContext.FilterItemFootnote
                .AsQueryable()
                .SingleAsync(f =>
                    f.FootnoteId == footnoteId && f.FilterItemId == plan.Id
                );

            statisticsDbContext.Remove(filterItemFootnote);
            await statisticsDbContext.FilterItemFootnote.AddAsync(new FilterItemFootnote
            {
                FootnoteId = footnoteId,
                FilterItemId = plan.TargetValue
            });
        }

        private async Task ReplaceIndicatorFootnote(Guid footnoteId, TargetableReplacementViewModel plan)
        {
            var indicatorFootnote = await statisticsDbContext.IndicatorFootnote
                .AsQueryable()
                .SingleAsync(f =>
                    f.FootnoteId == footnoteId && f.IndicatorId == plan.Id
                );

            statisticsDbContext.Remove(indicatorFootnote);
            await statisticsDbContext.IndicatorFootnote.AddAsync(new IndicatorFootnote
            {
                FootnoteId = footnoteId,
                IndicatorId = plan.TargetValue
            });
        }

        private async Task<List<FilterSequenceEntry>?> ReplaceFilterSequence(ReleaseFile originalReleaseFile,
            ReleaseFile replacementReleaseFile)
        {
            // If the sequence is undefined then leave it so we continue to fallback to ordering by label alphabetically
            if (originalReleaseFile.FilterSequence == null)
            {
                return null;
            }

            var originalFilters =
                await filterRepository.GetFiltersIncludingItems(originalReleaseFile.File.SubjectId!.Value);
            var replacementFilters =
                await filterRepository.GetFiltersIncludingItems(replacementReleaseFile.File.SubjectId!.Value);

            return ReplacementServiceHelper.ReplaceFilterSequence(
                originalFilters: originalFilters,
                replacementFilters: replacementFilters,
                originalReleaseFile);
        }

        private async Task<List<IndicatorGroupSequenceEntry>?> ReplaceIndicatorSequence(
            ReleaseFile originalReleaseFile,
            ReleaseFile replacementReleaseFile)
        {
            // If the sequence is undefined then leave it so we continue to fallback to ordering by label alphabetically
            if (originalReleaseFile.IndicatorSequence == null)
            {
                return null;
            }

            var originalIndicatorGroups =
                await indicatorGroupRepository.GetIndicatorGroups(originalReleaseFile.File.SubjectId!.Value);
            var replacementIndicatorGroups =
                await indicatorGroupRepository.GetIndicatorGroups(replacementReleaseFile.File.SubjectId!.Value);

            return ReplacementServiceHelper.ReplaceIndicatorSequence(
                originalIndicatorGroups: originalIndicatorGroups,
                replacementIndicatorGroups: replacementIndicatorGroups,
                originalReleaseFile);
        }

        private Task<Either<ActionResult, Unit>> InvalidateDataBlockCachedResults(
            DataBlockReplacementPlanViewModel plan, Guid releaseVersionId)
        {
            return cacheKeyService
                .CreateCacheKeyForDataBlock(releaseVersionId: releaseVersionId,
                    dataBlockId: plan.Id)
                .OnSuccessVoid(privateCacheService.DeleteItemAsync);
        }

        private static Guid ReplacementPlanOriginalId(TargetableReplacementViewModel plan)
        {
            return plan.Id;
        }

        private static Guid ReplacementPlanTargetId(TargetableReplacementViewModel plan)
        {
            return plan.TargetValue;
        }
    }
