#nullable enable
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.Chart.ChartType;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Chart
{

    [JsonConverter(typeof(StringEnumConverter), typeof(CamelCaseNamingStrategy))]
    public enum BarChartDataLabelPosition
    {
        Inside, Outside
    }

    [JsonConverter(typeof(StringEnumConverter), typeof(CamelCaseNamingStrategy))]
    public enum LineChartDataLabelPosition
    {
        Above, Below
    }

    [JsonConverter(typeof(ContentBlockChartConverter))]
    public interface IChart
    {
        ChartType Type { get; }
        string? Title { get; set; }
        string Alt { get; set; }
        int Height { get; set; }
        int? Width { get; set; }
        bool IncludeNonNumericData { get; set; }

        Dictionary<string, ChartAxisConfiguration>? Axes { get; set; }
        public ChartLegend? Legend { get; set; }

    }

    public abstract class Chart : IChart
    {
        public string? Title { get; set; }
        public string Alt { get; set; }
        public int Height { get; set; }
        public int? Width { get; set; }
        public bool IncludeNonNumericData { get; set; } = false;

        public abstract ChartType Type { get; }

        public Dictionary<string, ChartAxisConfiguration>? Axes { get; set; }
        public ChartLegend? Legend { get; set; }
    }

    public class LineChart : Chart
    {
        public override ChartType Type => Line;
        public bool ShowDataLabels { get; set; }
        public LineChartDataLabelPosition? DataLabelPosition { get; set; }
    }

    public class HorizontalBarChart : Chart
    {
        public override ChartType Type => HorizontalBar;

        public int? BarThickness { get; set; }
        public bool Stacked;
        public bool ShowDataLabels { get; set; }
        public BarChartDataLabelPosition? DataLabelPosition { get; set; }
    }

    public class VerticalBarChart : Chart
    {
        public override ChartType Type => VerticalBar;

        public int? BarThickness { get; set; }
        public bool Stacked;
        public bool ShowDataLabels { get; set; }
        public BarChartDataLabelPosition? DataLabelPosition { get; set; }
    }

    public class MapChart : Chart
    {
        public override ChartType Type => Map;

        // TODO EES-3319 - make mandatory when all Map Charts are migrated to have a Boundary Level set
        public long? BoundaryLevel { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ChartDataClassification? DataClassification { get; set; }

        public int? DataGroups { get; set; }

        public List<ChartCustomDataGroup>? CustomDataGroups { get; set; }

    }

    public class InfographicChart : Chart
    {
        public override ChartType Type => Infographic;
        public string FileId { get; set; }
    }
}
