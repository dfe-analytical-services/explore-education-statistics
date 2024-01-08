#nullable enable
using System;
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

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public ReleaseType Type { get; init; }

    public int? Rank { get; set; }
}
