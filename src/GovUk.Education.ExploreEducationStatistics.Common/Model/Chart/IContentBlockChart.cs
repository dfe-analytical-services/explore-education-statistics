using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.Chart.ChartType;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Chart
{
    [JsonConverter(typeof(ContentBlockChartConverter))]
    public interface IContentBlockChart
    {
        ChartType Type { get; }
        string Title { get; set; }
        string Alt { get; set; }
        int Height { get; set; }
        int? Width { get; set; }
    }

    public class LineChart : AbstractChart
    {
        public override ChartType Type => Line;
        
        public ChartLegend Legend;
        public int LegendHeight { get; set; }

        public Dictionary<string, ChartConfiguration> Labels;
        public Dictionary<string, ChartAxisConfigurationItem> Axes;
    }

    public class HorizontalBarChart : AbstractChart
    {
        public override ChartType Type => HorizontalBar;
        
        public ChartLegend Legend;
        public int LegendHeight { get; set; }
        public int? BarThickness { get; set; }

        public Dictionary<string, ChartConfiguration> Labels;
        public Dictionary<string, ChartAxisConfigurationItem> Axes;
        public bool Stacked;
    }

    public class VerticalBarChart : AbstractChart
    {
        public override ChartType Type => VerticalBar;
        
        public ChartLegend Legend;
        public int LegendHeight { get; set; }
        public int? BarThickness { get; set; }

        public Dictionary<string, ChartConfiguration> Labels;
        public Dictionary<string, ChartAxisConfigurationItem> Axes;
        public bool Stacked;
    }

    public class MapChart : AbstractChart
    {
        public override ChartType Type => Map;
        
        public Dictionary<string, ChartConfiguration> Labels;
        public Dictionary<string, ChartAxisConfigurationItem> Axes;
    }

    public class InfographicChart : AbstractChart
    {
        public override ChartType Type => Infographic;
        public string FileId { get; set; }
    }
}