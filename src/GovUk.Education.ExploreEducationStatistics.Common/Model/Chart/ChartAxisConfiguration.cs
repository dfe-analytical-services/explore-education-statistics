using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Chart
{
    public class ChartAxisConfiguration
    {
        public string Name;

        [JsonConverter(typeof(StringEnumConverter))]
        public AxisType Type;

        [JsonConverter(typeof(StringEnumConverter))]
        public AxisGroupBy GroupBy;

        public string SortBy;
        public bool SortAsc = true;

        public List<ChartDataSet> DataSets = new List<ChartDataSet>();
        public List<AxisReferenceLine> ReferenceLines = new List<AxisReferenceLine>();
        public bool Visible = true;
        public string Title;
        public string Unit;
        public bool ShowGrid = true;

        public AxisLabel Label;

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
        public string Text;
        public int? Width;
        public bool? Rotated;
    }

    public class AxisReferenceLine
    {
        public string Label;
        public string Position;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum AxisTickConfig
    {
        [EnumMember(Value = "default")] Default,
        startEnd,
        custom
    }
}
