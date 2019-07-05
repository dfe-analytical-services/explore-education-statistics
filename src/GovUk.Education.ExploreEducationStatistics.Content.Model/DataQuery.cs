using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Converters;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model {
    public class DataQuery
    {
        public string path;
        public string method;
        public string body;

    }

    public class DataBlockRequest {
        public int subjectId;
        public string geographicLevel;
        public List<string> countries;
        public List<string> localAuthorities;
        public List<string> regions;
        public string startYear;
        public string endYear;
        public List<string> filters;
        public List<string> indicators;
    }

    public class Axis
    {
        public string title;
    }

    [JsonConverter(typeof(ContentBlockChartConverter))]
    public interface IContentBlockChart
    {
        string Type { get; }
    }

    public class ChartDataSet
    {
        public string Indicator;
        public List<string> filters;
        public List<string> location;
        public string timePeriod;
    }

    public class ChartAxisConfiguration
    {
        public string Name;
        public List<string> GroupBy;
        public List<ChartDataSet> DataSets;
        public bool Visible;
        public string Title;
    }

    public class ChartLabelConfiguration
    {
        public string Name;
        public string Label;
        public string Unit;
        public string Value;
    }

    public class LineChart : IContentBlockChart
    {
        public string Type => "line";
        public Dictionary<string, ChartLabelConfiguration> Labels;
        public Dictionary<string, ChartAxisConfiguration> Axes;
    }

    public class HorizontalBarChart : IContentBlockChart 
    {
        public string Type => "horizontalbar";
        public Dictionary<string, ChartLabelConfiguration> Labels;
        public Dictionary<string, ChartAxisConfiguration> Axes;

    }

    public class VerticalBarChart : IContentBlockChart 
    {
        public string Type => "verticalbar";
        public Dictionary<string, ChartLabelConfiguration> Labels;
        public Dictionary<string, ChartAxisConfiguration> Axes;

    }

    public class MapChart : IContentBlockChart 
    {
        public string Type => "map";
        public Dictionary<string, ChartLabelConfiguration> Labels;
        public Dictionary<string, ChartAxisConfiguration> Axes;

    }

}