#nullable enable
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Chart
{
    public class ChartAxisConfiguration
    {
        public string Name = null!;

        [JsonConverter(typeof(StringEnumConverter))]
        public AxisType Type;

        [JsonConverter(typeof(StringEnumConverter))]
        public AxisGroupBy? GroupBy;

        public string? GroupByFilter;

        public string SortBy = null!;
        public bool SortAsc = true;

        public List<ChartDataSet> DataSets = new();
        public List<AxisReferenceLine> ReferenceLines = new();
        public bool Visible = true;
        public string Title = null!;
        public string Unit = null!;
        public bool ShowGrid = true;

        public AxisLabel Label = null!;

        public int? Min;
        public int? Max;
        public int? Size;

        [JsonConverter(typeof(StringEnumConverter))]
        public AxisTickConfig TickConfig;

        public int? TickSpacing;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum AxisGroupBy
    {
        timePeriod,
        locations,
        filters,
        indicators
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum AxisType
    {
        major,
        minor
    }

    public class AxisLabel
    {
        public string Text = null!;
        public int? Width;
        public bool? Rotated;
    }

    public class AxisReferenceLine
    {
        public string Label = null!;
        public string Position = null!;
        
        [JsonConverter(typeof(StringEnumConverter))]
        public AxisReferenceLineStyle? Style;

        public int? OtherAxisPosition;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum AxisTickConfig
    {
        [EnumMember(Value = "default")] Default,
        startEnd,
        custom
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum AxisReferenceLineStyle
    {
        dashed,
        solid,
        none
    }
}
