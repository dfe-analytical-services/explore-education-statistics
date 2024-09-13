using System;
using Azure.Data.Tables;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.Azure.Cosmos.Table;
using static Microsoft.Azure.Cosmos.Table.QueryComparisons;
using static Microsoft.Azure.Cosmos.Table.TableOperators;
using static Microsoft.Azure.Cosmos.Table.TableQuery;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public static class ReleaseStatusTableQueryUtil
    {
        public static TableQuery<ReleasePublishingStatusOld> QueryPublishTodayOrInFutureWithStages(
            ReleasePublishingStatusContentStage? content = null,
            ReleasePublishingStatusFilesStage? files = null,
            ReleasePublishingStatusPublishingStage? publishing = null,
            ReleasePublishingStatusOverallStage? overall = null)
        {
            return QueryPublishByDateWithStages(FilterPublishTodayOrInFuture(), content, files, publishing, overall);
        }

        public static string UpdateFilterForStages(
            string filter,
            ReleasePublishingStatusContentStage? content = null,
            ReleasePublishingStatusFilesStage? files = null,
            ReleasePublishingStatusPublishingStage? publishing = null,
            ReleasePublishingStatusOverallStage? overall = null)
        {
            if (content.HasValue)
            {
                var contentFilterCondition = TableClient.CreateQueryFilter<ReleasePublishingStatus>(status =>
                    status.ContentStage == content.ToString());

                filter = $"({filter}) and ({contentFilterCondition})"; // @MarkFix put this in a util method
            }

            if (files.HasValue)
            {
                var filesFilterCondition = TableClient.CreateQueryFilter<ReleasePublishingStatus>(status =>
                    status.FilesStage == files.ToString());

                filter = $"({filter}) and ({filesFilterCondition})";
            }

            if (publishing.HasValue)
            {
                var publishingFilterCondition = TableClient.CreateQueryFilter<ReleasePublishingStatus>(status =>
                    status.PublishingStage == publishing.ToString());

                filter = $"({filter}) and ({publishingFilterCondition})";
            }

            if (overall.HasValue)
            {
                var overallFilterCondition = TableClient.CreateQueryFilter<ReleasePublishingStatus>(status =>
                    status.OverallStage == overall);

                filter = $"({filter}) and ({overallFilterCondition})";
            }

            return filter;
        }

        private static TableQuery<ReleasePublishingStatusOld> QueryPublishByDateWithStages(
            string dateFilter,
            ReleasePublishingStatusContentStage? content = null,
            ReleasePublishingStatusFilesStage? files = null,
            ReleasePublishingStatusPublishingStage? publishing = null,
            ReleasePublishingStatusOverallStage? overall = null)
        {
            var filter = dateFilter;

            if (content.HasValue)
            {
                filter = CombineFilters(filter, And, FilterContentStageEquals(content.Value));
            }

            if (files.HasValue)
            {
                filter = CombineFilters(filter, And, FilterFilesStageEquals(files.Value));
            }

            if (publishing.HasValue)
            {
                filter = CombineFilters(filter, And, FilterPublishingStageEquals(publishing.Value));
            }

            if (overall.HasValue)
            {
                filter = CombineFilters(filter, And, FilterOverallStageEquals(overall.Value));
            }

            return new TableQuery<ReleasePublishingStatusOld>().Where(filter);
        }

        private static string FilterPublishTodayOrInFuture() =>
            GenerateFilterConditionForDate(nameof(ReleasePublishingStatusOld.Publish), GreaterThanOrEqual, DateTime.Today);

        private static string FilterContentStageEquals(ReleasePublishingStatusContentStage stage) =>
            GenerateFilterCondition(nameof(ReleasePublishingStatusOld.ContentStage), Equal, stage.ToString());

        private static string FilterFilesStageEquals(ReleasePublishingStatusFilesStage stage) =>
            GenerateFilterCondition(nameof(ReleasePublishingStatusOld.FilesStage), Equal, stage.ToString());

        private static string FilterPublishingStageEquals(ReleasePublishingStatusPublishingStage stage) =>
            GenerateFilterCondition(nameof(ReleasePublishingStatusOld.PublishingStage), Equal, stage.ToString());

        private static string FilterOverallStageEquals(ReleasePublishingStatusOverallStage stage) =>
            GenerateFilterCondition(nameof(ReleasePublishingStatusOld.OverallStage), Equal, stage.ToString());
    }
}
