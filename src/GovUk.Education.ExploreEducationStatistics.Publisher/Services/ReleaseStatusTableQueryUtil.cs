using System;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.Azure.Cosmos.Table;
using static Microsoft.Azure.Cosmos.Table.QueryComparisons;
using static Microsoft.Azure.Cosmos.Table.TableOperators;
using static Microsoft.Azure.Cosmos.Table.TableQuery;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public static class ReleaseStatusTableQueryUtil
    {
        public static TableQuery<ReleasePublishingStatus> QueryPublishLessThanEndOfTodayWithStages(
            ReleasePublishingStatusContentStage? content = null,
            ReleasePublishingStatusFilesStage? files = null,
            ReleasePublishingStatusPublishingStage? publishing = null,
            ReleasePublishingStatusOverallStage? overall = null)
        {
            return QueryPublishByDateWithStages(FilterPublishLessThanEndOfToday(), content, files, publishing, overall);
        }

        public static TableQuery<ReleasePublishingStatus> QueryPublishTodayOrInFutureWithStages(
            ReleasePublishingStatusContentStage? content = null,
            ReleasePublishingStatusFilesStage? files = null,
            ReleasePublishingStatusPublishingStage? publishing = null,
            ReleasePublishingStatusOverallStage? overall = null)
        {
            return QueryPublishByDateWithStages(FilterPublishTodayOrInFuture(), content, files, publishing, overall);
        }

        private static TableQuery<ReleasePublishingStatus> QueryPublishByDateWithStages(
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

            return new TableQuery<ReleasePublishingStatus>().Where(filter);
        }

        private static string FilterPublishLessThanEndOfToday() =>
            GenerateFilterConditionForDate(nameof(ReleasePublishingStatus.Publish), LessThan, DateTime.Today.AddDays(1));

        private static string FilterPublishTodayOrInFuture() =>
            GenerateFilterConditionForDate(nameof(ReleasePublishingStatus.Publish), GreaterThanOrEqual, DateTime.Today);

        private static string FilterContentStageEquals(ReleasePublishingStatusContentStage stage) =>
            GenerateFilterCondition(nameof(ReleasePublishingStatus.ContentStage), Equal, stage.ToString());

        private static string FilterFilesStageEquals(ReleasePublishingStatusFilesStage stage) =>
            GenerateFilterCondition(nameof(ReleasePublishingStatus.FilesStage), Equal, stage.ToString());

        private static string FilterPublishingStageEquals(ReleasePublishingStatusPublishingStage stage) =>
            GenerateFilterCondition(nameof(ReleasePublishingStatus.PublishingStage), Equal, stage.ToString());

        private static string FilterOverallStageEquals(ReleasePublishingStatusOverallStage stage) =>
            GenerateFilterCondition(nameof(ReleasePublishingStatus.OverallStage), Equal, stage.ToString());
    }
}