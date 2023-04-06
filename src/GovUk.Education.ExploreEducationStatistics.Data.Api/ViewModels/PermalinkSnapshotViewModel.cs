#nullable enable
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;

public record PermalinkSnapshotViewModel
{
    public Guid Id { get; set; }

    public DateTime Created { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]

    public PermalinkStatus Status { get; set; }

    public UniversalTableFormat Table { get; set; }
}

public abstract record UniversalTableFormat
{
    public TableCellJson[][] TableCellJson { get; set; }
}

public abstract record TableCellJson
{
    public int? ColSpan { get; set; }

    public int? RowSpan { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]

    public TableScope Scope { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]

    public Tag Tag { get; set; }

    public string? Text { get; set; }
}

public enum Tag
{
    Th,
    Td
}

public enum TableScope
{
    ColGroup,
    RowGroup,
    Col,
    Row
}