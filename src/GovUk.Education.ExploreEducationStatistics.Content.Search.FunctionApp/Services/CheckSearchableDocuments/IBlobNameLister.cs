namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CheckSearchableDocuments;

public interface IBlobNameLister
{
    Task<IList<string>> ListBlobsInContainer(string containerName, CancellationToken cancellationToken = default);
}
