using Generator.Equals;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;

[Equatable]
public partial record Blob(
    string Contents,
    [property: UnorderedEquality] IDictionary<string, string>? Metadata = null
)
{
    public override string ToString() =>
        $"Blob[Contents:{Contents}|Metadata:{Metadata.ToDetailedString()}]";
}
