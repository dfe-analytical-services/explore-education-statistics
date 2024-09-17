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
                var contentFilterCondition = TableClient.CreateQueryFilter<ReleasePublishingStatus>(status =>
                    status.ContentStage == content.ToString());

                filter = DataTableStorageUtils.CombineQueryFiltersAnd(filter, contentFilterCondition);
            }

            if (files.HasValue)
            {
                var filesFilterCondition = TableClient.CreateQueryFilter<ReleasePublishingStatus>(status =>
                    status.FilesStage == files.ToString());

                filter = DataTableStorageUtils.CombineQueryFiltersAnd(filter, filesFilterCondition);
            }

            if (publishing.HasValue)
            {
                var publishingFilterCondition = TableClient.CreateQueryFilter<ReleasePublishingStatus>(status =>
                    status.PublishingStage == publishing.ToString());

                filter = DataTableStorageUtils.CombineQueryFiltersAnd(filter, publishingFilterCondition);
            }

            if (overall.HasValue)
            {
                var overallFilterCondition = TableClient.CreateQueryFilter<ReleasePublishingStatus>(status =>
                    status.OverallStage == overall);

                filter = DataTableStorageUtils.CombineQueryFiltersAnd(filter, overallFilterCondition);
            }

            return filter;
        }
    }
}
