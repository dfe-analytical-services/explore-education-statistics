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
        public static TableQuery<ReleaseStatus> QueryPublishLessThanEndOfTodayWithStages(
            ReleaseStatusContentStage? content = null,
            ReleaseStatusDataStage? data = null,
            ReleaseStatusFilesStage? files = null,
            ReleaseStatusPublishingStage? publishing = null,
            ReleaseStatusOverallStage? overall = null)
        {
            var filter = FilterPublishLessThanEndOfToday();

            if (content.HasValue)
            {
                filter = CombineFilters(filter, And, FilterContentStageEquals(content.Value));
            }

            if (data.HasValue)
            {
                filter = CombineFilters(filter, And, FilterDataStageEquals(data.Value));
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

            return new TableQuery<ReleaseStatus>().Where(filter);
        }

        private static string FilterPublishLessThanEndOfToday() =>
            GenerateFilterConditionForDate(nameof(ReleaseStatus.Publish), LessThan, DateTime.Today.AddDays(1));

        private static string FilterContentStageEquals(ReleaseStatusContentStage stage) =>
            GenerateFilterCondition(nameof(ReleaseStatus.ContentStage), Equal, stage.ToString());

        private static string FilterDataStageEquals(ReleaseStatusDataStage stage) =>
            GenerateFilterCondition(nameof(ReleaseStatus.DataStage), Equal, stage.ToString());

        private static string FilterFilesStageEquals(ReleaseStatusFilesStage stage) =>
            GenerateFilterCondition(nameof(ReleaseStatus.FilesStage), Equal, stage.ToString());

        private static string FilterPublishingStageEquals(ReleaseStatusPublishingStage stage) =>
            GenerateFilterCondition(nameof(ReleaseStatus.PublishingStage), Equal, stage.ToString());

        private static string FilterOverallStageEquals(ReleaseStatusOverallStage stage) =>
            GenerateFilterCondition(nameof(ReleaseStatus.OverallStage), Equal, stage.ToString());
    }
}