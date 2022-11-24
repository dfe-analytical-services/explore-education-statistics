#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;

public record PublicationSearchResultViewModel
{
    public Guid Id { get; init; }
    public string Slug { get; init; }
    public string Summary { get; init; }
    public string Title { get; init; }
    public string Theme { get; init; }
    public DateTime Published { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    public ReleaseType Type { get; init; }

    public int? Rank { get; set; }
}
