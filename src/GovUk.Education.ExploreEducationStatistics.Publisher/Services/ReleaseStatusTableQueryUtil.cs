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
        public static TableQuery<ReleasePublishingStatusOld> QueryPublishLessThanEndOfTodayWithStages(
            ReleasePublishingStatusContentStage? content = null,
            ReleasePublishingStatusFilesStage? files = null,
            ReleasePublishingStatusPublishingStage? publishing = null,
            ReleasePublishingStatusOverallStage? overall = null)
        {
            return QueryPublishByDateWithStages(FilterPublishLessThanEndOfToday(), content, files, publishing, overall);
        }

        public static TableQuery<ReleasePublishingStatusOld> QueryPublishTodayOrInFutureWithStages(
            ReleasePublishingStatusContentStage? content = null,
            ReleasePublishingStatusFilesStage? files = null,
            ReleasePublishingStatusPublishingStage? publishing = null,
            ReleasePublishingStatusOverallStage? overall = null)
        {
            return QueryPublishByDateWithStages(FilterPublishTodayOrInFuture(), content, files, publishing, overall);
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

        private static string FilterPublishLessThanEndOfToday() =>
            GenerateFilterConditionForDate(nameof(ReleasePublishingStatusOld.Publish), LessThan, DateTime.Today.AddDays(1));

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
