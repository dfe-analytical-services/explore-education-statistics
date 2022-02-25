using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query
{
    public class ObservationQueryContext
    {
        public Guid SubjectId { get; set; }
        public TimePeriodQuery TimePeriod { get; set; }
        public IEnumerable<Guid> Filters { get; set; }
        public long? BoundaryLevel { get; set; }
        public IEnumerable<Guid> Indicators { get; set; }
        public LocationQuery Locations { get; set; }
        public IEnumerable<Guid> LocationIds { get; set; }
        public bool? IncludeGeoJson { get; set; }

        public ObservationQueryContext Clone()
        {
            return new ObservationQueryContext
            {
                SubjectId = SubjectId,
                TimePeriod = TimePeriod?.Clone(),
                Filters = Filters?.ToList(),
                BoundaryLevel = BoundaryLevel,
                Indicators = Indicators?.ToList(),
                Locations = Locations == null ? null : Locations with { },
                LocationIds = LocationIds?.ToList(),
                IncludeGeoJson = IncludeGeoJson
            };
        }

        public override string ToString()
        {
            return
                $"{nameof(SubjectId)}: {SubjectId}, " +
                $"{nameof(TimePeriod)}: {TimePeriod}, " +
                $"{nameof(Filters)}: [{(Filters == null ? string.Empty : Filters.JoinToString(", "))}], " +
                $"{nameof(BoundaryLevel)}: {BoundaryLevel}, " +
                $"{nameof(Indicators)}: [{(Indicators == null ? string.Empty : Indicators.JoinToString(", "))}], " +
                $"{nameof(Locations)}: {Locations}, " +
                $"{nameof(LocationIds)}: [{(LocationIds == null ? string.Empty : LocationIds.JoinToString(", "))}], " +
                $"{nameof(IncludeGeoJson)}: {IncludeGeoJson}";
        }
    }
}
