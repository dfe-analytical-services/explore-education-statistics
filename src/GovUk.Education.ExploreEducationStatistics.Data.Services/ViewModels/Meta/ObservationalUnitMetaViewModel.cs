using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta
{
    /// <summary>
    /// Legacy locations view model returned in Table result Subject meta data.
    /// </summary>
    public record ObservationalUnitMetaViewModel : LabelValue
    {
        [JsonConverter(typeof(StringEnumConverter), true)]
        public GeographicLevel Level { get; set; }

        public dynamic GeoJson { get; set; }
    }
}
