using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.Chart.ChartType;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Chart
{
    [JsonConverter(typeof(ContentBlockChartConverter))]
    public interface IChart
    {
        ChartType Type { get; }
        string Title { get; set; }
        string Alt { get; set; }
        int Height { get; set; }
        int? Width { get; set; }
    }

    public abstract class Chart : IChart
    {
        public string Title { get; set; }
        public string Alt { get; set; }
        public int Height { get; set; }
        public int? Width { get; set; }

        public abstract ChartType Type { get; }
    }

    public class LineChart : Chart
    {
        public override ChartType Type => Line;

        public ChartLegendPosition Legend;
        public int LegendHeight { get; set; }

        public Dictionary<string, ChartLabelConfiguration> Labels;
        public Dictionary<string, ChartAxisConfiguration> Axes;
    }

    public class HorizontalBarChart : Chart
    {
        public override ChartType Type => HorizontalBar;

        public ChartLegendPosition Legend;
        public int LegendHeight { get; set; }
        public int? BarThickness { get; set; }

        public Dictionary<string, ChartLabelConfiguration> Labels;
        public Dictionary<string, ChartAxisConfiguration> Axes;
        public bool Stacked;
    }

    public class VerticalBarChart : Chart
    {
        public override ChartType Type => VerticalBar;

        public ChartLegendPosition Legend;
        public int LegendHeight { get; set; }
        public int? BarThickness { get; set; }

        public Dictionary<string, ChartLabelConfiguration> Labels;
        public Dictionary<string, ChartAxisConfiguration> Axes;
        public bool Stacked;
    }

    public class MapChart : Chart
    {
        public override ChartType Type => Map;

        public Dictionary<string, ChartLabelConfiguration> Labels;
        public Dictionary<string, ChartAxisConfiguration> Axes;
    }

    public class InfographicChart : Chart
    {
        public override ChartType Type => Infographic;
        public string FileId { get; set; }
    }
}