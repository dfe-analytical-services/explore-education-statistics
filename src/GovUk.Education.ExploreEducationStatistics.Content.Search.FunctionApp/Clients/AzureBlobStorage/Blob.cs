using Generator.Equals;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;


[Equatable]
public partial record Blob(string Contents,  [property: UnorderedEquality] IDictionary<string, string>? Metadata = null);
