using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record PublicationSearchResultViewModel
{
    public Guid Id { get; init; }
    public string Slug { get; init; }
    public string Summary { get; init; }
    public string Title { get; init; }
    public string Theme { get; init; }
    public DateTime Published { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public ReleaseType Type { get; init; }

    public int? Rank { get; set; }
}
