#nullable enable
using System;
using System.Runtime.Serialization;
using JsonKnownTypes;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;

[JsonConverter(typeof(JsonKnownTypesConverter<KeyStatisticViewModel>))]
[JsonDiscriminator(Name = "Type")]
[KnownType(typeof(KeyStatisticTextViewModel))]
[KnownType(typeof(KeyStatisticDataBlockViewModel))]
public abstract record KeyStatisticViewModel
{
    public Guid Id { get; set; }

    public Guid ReleaseId { get; set; }

    public string? Trend { get; set; } = string.Empty;

    public string? GuidanceTitle { get; set; } = string.Empty;

    public string? GuidanceText { get; set; } = string.Empty;

    public int Order { get; set; }

    public DateTime Created { get; set; }

    public DateTime? Updated { get; set; }
}

public record KeyStatisticDataBlockViewModel : KeyStatisticViewModel
{
    public Guid DataBlockId { get; set; }
}

public record KeyStatisticTextViewModel : KeyStatisticViewModel
{
    public string Title { get; set; } = string.Empty;

    public string Statistic { get; set; } = string.Empty;
}
