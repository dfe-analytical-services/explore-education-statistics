using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Chart
{
    [JsonConverter(typeof(ContentBlockChartConverter))]
    public interface IContentBlockChart
    {
        string Type { get; }
        string Title { get; set; }
        int Height { get; set; }
        int Width { get; set; }
    }

    public class LineChart : IContentBlockChart
    {
        public string Type => "line";
        public string Title { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Legend Legend;

        public int LegendHeight { get; set; }

        public Dictionary<string, ChartConfiguration> Labels;
        public Dictionary<string, ChartAxisConfigurationItem> Axes;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum Legend
    {
        none,
        bottom,
        top
    }

    public class HorizontalBarChart : IContentBlockChart
    {
        public string Type => "horizontalbar";
        public string Title { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Legend Legend;

        public int LegendHeight { get; set; }

        public Dictionary<string, ChartConfiguration> Labels;
        public Dictionary<string, ChartAxisConfigurationItem> Axes;
        public bool Stacked;
    }

    public class VerticalBarChart : IContentBlockChart
    {
        public string Type => "verticalbar";
        public Dictionary<string, ChartConfiguration> Labels;
        public Dictionary<string, ChartAxisConfigurationItem> Axes;
        public bool Stacked;
        public string Title { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Legend Legend;

        public int LegendHeight { get; set; }
    }

    public class MapChart : IContentBlockChart
    {
        public string Type => "map";
        public string Title { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Dictionary<string, ChartConfiguration> Labels;
        public Dictionary<string, ChartAxisConfigurationItem> Axes;
    }

    public class InfographicChart : IContentBlockChart
    {
        public string Type => "infographic";
        public string Title { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public string ReleaseId;
        public string FileId;
    }
}