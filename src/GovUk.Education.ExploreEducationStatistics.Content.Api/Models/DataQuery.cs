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

    public class DataBlockRequest {
        public int subjectId;
        public string geographicLevel;
        public List<int> countries;
        public List<int> localAuthorities;
        public List<int> regions;
        public int startYear;
        public int endYear;
        public List<int> filters;
        public List<int> indicators;
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

}