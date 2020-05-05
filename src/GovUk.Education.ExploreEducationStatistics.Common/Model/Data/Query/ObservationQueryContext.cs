using System;
using System.Collections.Generic;

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
        public bool? IncludeGeoJson { get; set; }

        public override string ToString()
        {
            return
                $"{nameof(SubjectId)}: {SubjectId}, " +
                $"{nameof(TimePeriod)}: {TimePeriod}, " +
                $"{nameof(Filters)}: [{(Filters == null ? string.Empty : string.Join(", ", Filters))}], " +
                $"{nameof(BoundaryLevel)}: {BoundaryLevel}, " +
                $"{nameof(Indicators)}: [{(Indicators == null ? string.Empty : string.Join(", ", Indicators))}], " +
                $"{nameof(Locations)}: {Locations}, " +
                $"{nameof(IncludeGeoJson)}: {IncludeGeoJson}";
        }
    }
}