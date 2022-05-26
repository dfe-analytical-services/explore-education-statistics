#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta
{
    public record LocationsMetaViewModel
    {
        public string Legend { get; init; } = string.Empty;
        public List<LocationAttributeViewModel> Options { get; init; } = new();
    }

    public record LocationAttributeViewModel : LabelValue
    {
        public Guid? Id { get; set; }

        public dynamic? GeoJson { get; init; }

        [JsonConverter(typeof(StringEnumConverter), typeof(CamelCaseNamingStrategy))]
        public GeographicLevel? Level { get; init; }

        public List<LocationAttributeViewModel>? Options { get; init; }
    }
}
