using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Chart
{
    public class ChartDataSet
    {
        public Guid Indicator;
        public List<Guid> Filters = new List<Guid>();
        public ChartDataSetLocation Location;
        public string TimePeriod;
        public ChartDataSetConfiguration Config;
    }

    public class ChartDataSetLocation
    {
        public string Level;
        public string Value;
    }

    public class ChartDataSetConfiguration
    {
        public string Label;
        public string Colour;

        [JsonConverter(typeof(StringEnumConverter))]
        public ChartLineSymbol? Symbol;

        [JsonConverter(typeof(StringEnumConverter))]
        public ChartLineStyle? LineStyle;
    }
}