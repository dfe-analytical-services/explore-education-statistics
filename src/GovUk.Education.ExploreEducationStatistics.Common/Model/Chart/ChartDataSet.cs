using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Chart
{
    public class ChartDataSet
    {
        public Guid Indicator;
        public List<Guid> Filters = new();
        public ChartDataSetLocation Location;
        public string TimePeriod;
        public int? Order;

        // TODO EES-1649 Migrate data set configs to legend item configs
        public ChartDataSetConfiguration Config;
    }

    public class ChartDataSetLocation
    {
        public string Level;
        public Guid Value;
    }

    [Obsolete("Use legend item configurations")]
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
