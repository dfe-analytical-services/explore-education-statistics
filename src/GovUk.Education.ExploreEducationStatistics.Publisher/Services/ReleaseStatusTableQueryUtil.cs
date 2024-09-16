using Azure.Data.Tables;
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
    }
}
