using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Chart
{
    public class ChartLegend
    {
        public ChartLegendPosition? Position;
        public List<ChartLegendItem> Items = new List<ChartLegendItem>();
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ChartLegendPosition
    {
        none,
        bottom,
        top
    }

    public class ChartLegendItem
    {
        public ChartDataSet DataSet;
        public string Label;
        public string Colour;

        [JsonConverter(typeof(StringEnumConverter))]
        public ChartLineSymbol? Symbol;

        [JsonConverter(typeof(StringEnumConverter))]
        public ChartLineStyle? LineStyle;
    }
}