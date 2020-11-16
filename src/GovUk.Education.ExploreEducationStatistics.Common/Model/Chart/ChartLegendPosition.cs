using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Chart
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ChartLegendPosition
    {
        none,
        bottom,
        top
    }
}