using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Chart
{
    // TODO: Remove in EES-1600
    [Obsolete("Data sets should be used instead")]
    public class ChartLabelConfiguration
    {
        public string Label;
        public string Value;
        public string Name;
        public string Unit;
        public string Colour;

        [JsonConverter(typeof(StringEnumConverter))]
        public ChartLineSymbol? Symbol;

        [JsonConverter(typeof(StringEnumConverter))]
        public ChartLineStyle? LineStyle;
    }
}