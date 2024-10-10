using Azure.Data.Tables;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public static class ReleaseStatusTableQueryUtil
    {
        public static string UpdateFilterForStages(
            string filter,
            ReleasePublishingStatusContentStage? content = null,
            ReleasePublishingStatusFilesStage? files = null,
            ReleasePublishingStatusPublishingStage? publishing = null,
            ReleasePublishingStatusOverallStage? overall = null)
        {
            if (content.HasValue)
            {
                var contentStr = System.Enum.GetName(content.Value);
                var contentFilterCondition = TableClient.CreateQueryFilter<ReleasePublishingStatus>(status =>
                    status.ContentStage == contentStr);

                filter = DataTableStorageUtils.CombineQueryFiltersAnd(filter, contentFilterCondition);
            }

            if (files.HasValue)
            {
                var filesStr = System.Enum.GetName(files.Value);
                var filesFilterCondition = TableClient.CreateQueryFilter<ReleasePublishingStatus>(status =>
                    status.FilesStage == filesStr);

                filter = DataTableStorageUtils.CombineQueryFiltersAnd(filter, filesFilterCondition);
            }

            if (publishing.HasValue)
            {
                var publishingStr = System.Enum.GetName(publishing.Value);
                var publishingFilterCondition = TableClient.CreateQueryFilter<ReleasePublishingStatus>(status =>
                    status.PublishingStage == publishingStr);

                filter = DataTableStorageUtils.CombineQueryFiltersAnd(filter, publishingFilterCondition);
            }

            if (overall.HasValue)
            {
                var overallStr = System.Enum.GetName(overall.Value);
                var overallFilterCondition = TableClient.CreateQueryFilter<ReleasePublishingStatus>(status =>
                    status.OverallStage == overallStr);

                filter = DataTableStorageUtils.CombineQueryFiltersAnd(filter, overallFilterCondition);
            }

            return filter;
        }
    }
}
