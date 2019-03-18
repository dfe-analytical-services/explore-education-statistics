using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Converters;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Models {
    public class DataQuery
    {
        public string path;
        public string method;
        public string body;

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
        public List<string> Attributes;
        public Axis XAxis;
        public Axis YAxis;
    }

}