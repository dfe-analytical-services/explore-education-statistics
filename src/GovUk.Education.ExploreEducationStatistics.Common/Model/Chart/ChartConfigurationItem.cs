using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Chart
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ChartConfiguration
    {
        public string Label;
        public string Value;
        public string Name;
        public string Unit;
        public string Colour;

        [JsonConverter(typeof(StringEnumConverter))]
        public ChartSymbol symbol;

        [JsonConverter(typeof(StringEnumConverter))]
        public LineStyle LineStyle = LineStyle.solid;
    }

    // this enum needs these like this as they match what is used in the front end
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum ChartSymbol
    {
        circle,
        cross,
        diamond,
        square,
        star,
        triangle,
        wye
    }
}