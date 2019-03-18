using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Models {
    public class DataQuery
    {
        public string path;
        public string method;
        public string body;

    }        

    public class Chart
    {
        public string Type;
        public List<string> Attributes;
        public Axis XAxis;
        public Axis YAxis;
    }

    public class Axis
    {
        public string title;
    }
}