using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Chart
{
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    [JsonConverter(typeof(EnumToEnumValueJsonConverter<ChartType>))]
    public enum ChartType
    {
        [EnumLabelValue("Line", "line")] Line,

        [EnumLabelValue("Horizontal Bar", "horizontalbar")]
        HorizontalBar,

        [EnumLabelValue("Vertical Bar", "verticalbar")]
        VerticalBar,

        [EnumLabelValue("Map", "map")] Map,

        [EnumLabelValue("Infographic", "infographic")]
        Infographic
    }
}