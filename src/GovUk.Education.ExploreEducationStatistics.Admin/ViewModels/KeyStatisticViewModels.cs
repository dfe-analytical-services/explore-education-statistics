#nullable enable
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
    public Guid Id { get; init; }

    public required string? Trend { get; init; }

    public required string? GuidanceTitle { get; init; }

    public required string? GuidanceText { get; init; }

    public required int Order { get; init; }

    public required DateTime Created { get; init; }

    public required DateTime? Updated { get; init; }

    public static KeyStatisticViewModel FromKeyStatistic(KeyStatistic keyStatistic) =>
        keyStatistic switch
        {
            KeyStatisticDataBlock dataBlock => KeyStatisticDataBlockViewModel.FromKeyStatisticDataBlock(dataBlock),
            KeyStatisticText text => KeyStatisticTextViewModel.FromKeyStatisticText(text),
            _ => throw new ArgumentOutOfRangeException(
                nameof(keyStatistic),
                $"Unhandled {nameof(KeyStatistic)} type: {keyStatistic.GetType().Name}"
            ),
        };
}

[JsonKnownThisType(nameof(KeyStatisticDataBlock))]
public record KeyStatisticDataBlockViewModel : KeyStatisticViewModel
{
    public required Guid DataBlockId { get; init; }

    public required Guid DataBlockParentId { get; init; }

    public static KeyStatisticDataBlockViewModel FromKeyStatisticDataBlock(KeyStatisticDataBlock model) =>
        new()
        {
            Id = model.Id,
            DataBlockId = model.DataBlockId,
            DataBlockParentId = model.DataBlockParentId,
            Trend = model.Trend,
            GuidanceTitle = model.GuidanceTitle,
            GuidanceText = model.GuidanceText,
            Order = model.Order,
            Created = model.Created,
            Updated = model.Updated,
        };
}

[JsonKnownThisType(nameof(KeyStatisticText))]
public record KeyStatisticTextViewModel : KeyStatisticViewModel
{
    public required string Title { get; init; } = string.Empty;

    public required string Statistic { get; init; } = string.Empty;

    public static KeyStatisticTextViewModel FromKeyStatisticText(KeyStatisticText model) =>
        new()
        {
            Id = model.Id,
            Title = model.Title,
            Statistic = model.Statistic,
            Trend = model.Trend,
            GuidanceTitle = model.GuidanceTitle,
            GuidanceText = model.GuidanceText,
            Order = model.Order,
            Created = model.Created,
            Updated = model.Updated,
        };
}
