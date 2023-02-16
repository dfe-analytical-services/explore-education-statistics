#nullable enable
using System;
using System.Runtime.Serialization;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using JsonKnownTypes;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

[JsonConverter(typeof(JsonKnownTypesConverter<KeyStatisticViewModel>))]
[JsonDiscriminator(Name = "type")]
[KnownType(typeof(KeyStatisticTextViewModel))]
[KnownType(typeof(KeyStatisticDataBlockViewModel))]
public abstract record KeyStatisticViewModel
{
    public Guid Id { get; set; }

    public string? Trend { get; set; }

    public string? GuidanceTitle { get; set; }

    public string? GuidanceText { get; set; }

    public int Order { get; set; }

    public DateTime Created { get; set; }

    public DateTime? Updated { get; set; }
}

[JsonKnownThisType(nameof(KeyStatisticDataBlock))]
public record KeyStatisticDataBlockViewModel : KeyStatisticViewModel
{
    public Guid DataBlockId { get; set; }
}

[JsonKnownThisType(nameof(KeyStatisticText))]
public record KeyStatisticTextViewModel : KeyStatisticViewModel
{
    public string Title { get; set; } = string.Empty;

    public string Statistic { get; set; } = string.Empty;
}
