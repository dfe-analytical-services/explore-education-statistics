using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Chart
{
    public class ChartAxisConfigurationItem
    {
        public string Name;

        [JsonConverter(typeof(StringEnumConverter))]
        public AxisType Type;

        [JsonConverter(typeof(StringEnumConverter))]
        public AxisGroupBy GroupBy;

        public string SortBy;
        public bool SortAsc = true;

        public List<ChartDataSet> DataSets;
        public List<ReferenceLine> ReferenceLines;
        public bool Visible = true;
        public string Title;
        public string Unit;
        public bool ShowGrid = true;

        [JsonConverter(typeof(StringEnumConverter))]
        public LabelPosition LabelPosition;

        public int? Min;
        public int? Max;
        public int? Size;

        [JsonConverter(typeof(StringEnumConverter))]
        public TickConfig TickConfig;

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

    public class ChartDataSet
    {
        public Guid Indicator;
        public List<Guid> Filters;
        public List<ChartDataLocation> Location;
        public string TimePeriod;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum LabelPosition
    {
        axis,
        graph,
        top,
        left,
        right,
        bottom,
        inside,
        outside,
        insideLeft,
        insideRight,
        insideTop,
        insideBottom,
        insideTopLeft,
        insideBottomLeft,
        insideTopRight
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum LineStyle
    {
        solid,
        dashed,
        dotted
    }

    public class ReferenceLine
    {
        public string Label;
        public string Position;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum TickConfig
    {
        [EnumMember(Value = "default")] Default,
        startEnd,
        custom
    }
}
