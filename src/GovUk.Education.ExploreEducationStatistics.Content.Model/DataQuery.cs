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

    public class LineChart : IContentBlockChart
    {
        public string Type => "line";
        public List<string> Indicators;
        public Axis XAxis;
        public Axis YAxis;
    }

    public class HorizontalBarChart : IContentBlockChart 
    {
        public string Type => "horizontalbar";
        public List<string> Indicators;
        public Axis XAxis;
        public Axis YAxis;

    }

    public class VerticalBarChart : IContentBlockChart 
    {
        public string Type => "verticalbar";
        public List<string> Indicators;
        public Axis XAxis;
        public Axis YAxis;

    }

    public class MapChart : IContentBlockChart 
    {
        public string Type => "map";
        public List<string> Indicators;
        public Axis XAxis => new Axis { title = "map"};
        public Axis YAxis => new Axis { title = "map"};

    }

}