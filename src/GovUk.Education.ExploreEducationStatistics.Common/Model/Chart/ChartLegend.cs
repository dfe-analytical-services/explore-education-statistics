#nullable enable
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;

public class ChartLegend
{
    public ChartLegendPosition? Position;
    public List<ChartLegendItem> Items = new List<ChartLegendItem>();
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
[JsonConverter(typeof(StringEnumConverter))]
public enum ChartLegendPosition
{
    none,
    bottom,
    top,
    inline,
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
[JsonConverter(typeof(StringEnumConverter))]
public enum ChartLegendLabelColour
{
    black,
    inherit,
}

public class ChartLegendItem
{
    public ChartBaseDataSet DataSet;
    public string Label;
    public string Colour;

    [JsonConverter(typeof(StringEnumConverter))]
    public ChartLegendLabelColour? LabelColour;

    [JsonConverter(typeof(StringEnumConverter))]
    public ChartLineSymbol? Symbol;

    [JsonConverter(typeof(StringEnumConverter))]
    public ChartLineStyle? LineStyle;

    [JsonConverter(typeof(StringEnumConverter))]
    public ChartInlinePosition? InlinePosition;

    public int? InlinePositionOffset;
}
