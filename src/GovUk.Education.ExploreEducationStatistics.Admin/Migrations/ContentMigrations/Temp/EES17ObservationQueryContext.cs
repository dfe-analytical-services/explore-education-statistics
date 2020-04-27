using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations.Temp
{
    public class EES17ObservationQueryContext
    {
        public Guid SubjectId { get; set; }
        public TimePeriodQuery TimePeriod { get; set; }
        public IEnumerable<Guid> Filters { get; set; }
        public long? BoundaryLevel { get; set; }
        public GeographicLevel? GeographicLevel { get; set; }
        public IEnumerable<Guid> Indicators { get; set; }
        public IEnumerable<string> Country { get; set; }
        public IEnumerable<string> Institution { get; set; }
        public IEnumerable<string> LocalAuthority { get; set; }
        public IEnumerable<string> LocalAuthorityDistrict { get; set; }
        public IEnumerable<string> LocalEnterprisePartnership { get; set; }
        public IEnumerable<string> MultiAcademyTrust { get; set; }
        public IEnumerable<string> MayoralCombinedAuthority { get; set; }
        public IEnumerable<string> OpportunityArea { get; set; }
        public IEnumerable<string> ParliamentaryConstituency { get; set; }
        public IEnumerable<string> Region { get; set; }
        public IEnumerable<string> RscRegion { get; set; }
        public IEnumerable<string> Sponsor { get; set; }
        public IEnumerable<string> Ward { get; set; }
        public IEnumerable<string> PlanningArea { get; set; }
        public bool? IncludeGeoJson { get; set; }

        public override string ToString()
        {
            return
                $"{nameof(SubjectId)}: {SubjectId}, " +
                $"{nameof(TimePeriod)}: {TimePeriod}, " +
                $"{nameof(Filters)}: [{(Filters == null ? string.Empty : string.Join(", ", Filters))}], " +
                $"{nameof(BoundaryLevel)}: {BoundaryLevel}, " +
                $"{nameof(GeographicLevel)}: {GeographicLevel?.GetEnumValue()}, " +
                $"{nameof(Indicators)}: [{(Indicators == null ? string.Empty : string.Join(", ", Indicators))}], " +
                $"{nameof(IncludeGeoJson)}: {IncludeGeoJson}";
        }
    }
}