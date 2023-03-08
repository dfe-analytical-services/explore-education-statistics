using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query
{
    public class ObservationQueryContext
    {
        public Guid SubjectId { get; set; }
        public TimePeriodQuery TimePeriod { get; set; }
        public IEnumerable<Guid> Filters { get; set; } = new List<Guid>();
        // TODO EES-3328 - remove BoundaryLevel from ObservationQueryContext as we now store it on
        // MapChart configuration instead
        public long? BoundaryLevel { get; set; }
        public IEnumerable<Guid> Indicators { get; set; } = new List<Guid>();
        public List<Guid> LocationIds { get; set; } = new();
        [Obsolete("Legacy Location field that exists in queries of historical Permalinks", false)]
        public LocationQuery Locations { get; set; }

        public override string ToString()
        {
            return
                $"{nameof(SubjectId)}: {SubjectId}, " +
                $"{nameof(TimePeriod)}: {TimePeriod}, " +
                $"{nameof(Filters)}: [{(Filters == null ? string.Empty : Filters.JoinToString(", "))}], " +
                $"{nameof(BoundaryLevel)}: {BoundaryLevel}, " +
                $"{nameof(Indicators)}: [{(Indicators == null ? string.Empty : Indicators.JoinToString(", "))}], " +
                $"{nameof(LocationIds)}: [{LocationIds.JoinToString(", ")}]";
        }
    }
}
