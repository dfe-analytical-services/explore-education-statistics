#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta
{
    public record IndicatorMetaViewModel
    {
        public string Label { get; init; } = string.Empty;

        [JsonConverter(typeof(EnumToEnumValueJsonConverter<Unit>))]
        public Unit Unit { get; init; }

        public string Value { get; init; } = string.Empty;

        public string Name { get; init; } = string.Empty;

        public int? DecimalPlaces { get; init; }
    }
}
