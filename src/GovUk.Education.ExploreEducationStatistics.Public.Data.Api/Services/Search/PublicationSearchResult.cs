using System.Text.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Search;

/// <summary>
/// Represents fields of the Azure Search index relevant to publication search results in the Public API.
/// This is used as a type parameter when querying to return strongly-typed search results.
/// </summary>
public record PublicationSearchResult
{
    [JsonPropertyName("publicationId")]
    public required Guid PublicationId { get; init; }

    [JsonPropertyName("publicationSlug")]
    public required string PublicationSlug { get; init; }

    [JsonPropertyName("published")]
    public required DateTimeOffset Published { get; init; }

    [JsonPropertyName("summary")]
    public required string Summary { get; init; }

    [JsonPropertyName("title")]
    public required string Title { get; init; }
}
