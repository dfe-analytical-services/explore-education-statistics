using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Query
{
    public class SubjectMetaQueryContext
    {
        public Guid SubjectId { get; set; }
        public TimePeriodQuery TimePeriod { get; set; }
        public long? BoundaryLevel { get; set; }
        public IEnumerable<Guid> Indicators { get; set; }
        public LocationQuery Locations { get; set; }
        public IEnumerable<Guid> LocationIds { get; set; }
        public bool? IncludeGeoJson { get; set; }

        public static SubjectMetaQueryContext FromObservationQueryContext(
            ObservationQueryContext observationQueryContext)
        {
            return new()
            {
                SubjectId = observationQueryContext.SubjectId,
                TimePeriod = observationQueryContext.TimePeriod,
                BoundaryLevel = observationQueryContext.BoundaryLevel,
                Indicators = observationQueryContext.Indicators,
                Locations = observationQueryContext.Locations,
                LocationIds = observationQueryContext.LocationIds,
                IncludeGeoJson = observationQueryContext.IncludeGeoJson
            };
        }

        public override string ToString()
        {
            return $"{nameof(SubjectId)}: {SubjectId}, " +
                   $"{nameof(TimePeriod)}: {TimePeriod}, " +
                   $"{nameof(BoundaryLevel)}: {BoundaryLevel}, " +
                   $"{nameof(Indicators)}: [{(Indicators == null ? string.Empty : Indicators.JoinToString(", "))}], " +
                   $"{nameof(Locations)}: {Locations}, " +
                   $"{nameof(LocationIds)}: [{(LocationIds == null ? string.Empty : LocationIds.JoinToString(", "))}], " +
                   $"IncludeGeoJson: {IncludeGeoJson}";
        }
    }
}
