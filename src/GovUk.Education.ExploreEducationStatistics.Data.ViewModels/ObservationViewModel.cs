using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Data.ViewModels;

public record ObservationViewModel
{
    public Guid Id { get; set; }

    public List<Guid> Filters { get; set; } = new();

    [JsonConverter(typeof(StringEnumConverter), typeof(CamelCaseNamingStrategy))]
    public GeographicLevel GeographicLevel { get; set; }

    public Guid LocationId { get; set; }

    public Dictionary<Guid, string> Measures { get; set; } = new();

    public string TimePeriod { get; set; } = string.Empty;
}
