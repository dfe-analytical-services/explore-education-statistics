using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta
{
    public class ObservationalUnitMetaViewModel : LabelValue
    {
        [JsonConverter(typeof(StringEnumConverter), true)]
        public GeographicLevel Level { get; set; }

        public dynamic GeoJson { get; set; }

        protected bool Equals(ObservationalUnitMetaViewModel other)
        {
            return base.Equals(other) && Level == other.Level;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ObservationalUnitMetaViewModel) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), (int) Level);
        }
    }
}