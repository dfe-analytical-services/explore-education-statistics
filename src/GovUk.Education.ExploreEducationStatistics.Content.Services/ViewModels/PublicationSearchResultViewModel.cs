#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;

public record PublicationSearchResultViewModel
{
    public Guid Id { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Theme { get; init; } = string.Empty;
    public DateTime Published { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    public ReleaseType Type { get; init; }

    public int Rank { get; set; }
}
