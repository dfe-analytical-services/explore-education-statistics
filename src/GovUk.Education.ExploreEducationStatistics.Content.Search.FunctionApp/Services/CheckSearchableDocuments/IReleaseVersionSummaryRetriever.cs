using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CheckSearchableDocuments;

public interface IReleaseVersionSummaryRetriever
{
    Task<IList<ReleaseVersionSummary>> GetAllPublishedReleaseVersionSummaries(
        CancellationToken cancellationToken = default
    );
}
