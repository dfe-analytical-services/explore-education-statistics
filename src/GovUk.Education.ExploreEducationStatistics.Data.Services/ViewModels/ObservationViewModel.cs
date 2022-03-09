#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels
{
    public record ObservationViewModel
    {
        public Guid Id { get; set; }

        public List<Guid> Filters { get; set; } = new();

        [JsonConverter(typeof(StringEnumConverter), typeof(CamelCaseNamingStrategy))]
        public GeographicLevel GeographicLevel { get; set; }

        public Guid LocationId { get; set; }

        // Legacy Location field that exists in table results of historical Permalinks
        public LocationViewModel? Location { get; set; }

        public Dictionary<Guid, string> Measures { get; set; } = new();

        public string TimePeriod { get; set; } = string.Empty;
    }
}
