using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Query
{
    public class SubjectMetaQueryContext
    {
        public Guid SubjectId { get; set; }
        public TimePeriodQuery TimePeriod { get; set; }
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

        public static SubjectMetaQueryContext FromObservationQueryContext(
            ObservationQueryContext observationQueryContext)
        {
            return new SubjectMetaQueryContext
            {
                SubjectId = observationQueryContext.SubjectId,
                TimePeriod = observationQueryContext.TimePeriod,
                BoundaryLevel = observationQueryContext.BoundaryLevel,
                GeographicLevel = observationQueryContext.GeographicLevel,
                Indicators = observationQueryContext.Indicators,
                Country = observationQueryContext.Country,
                Institution = observationQueryContext.Institution,
                LocalAuthority = observationQueryContext.LocalAuthority,
                LocalAuthorityDistrict = observationQueryContext.LocalAuthorityDistrict,
                LocalEnterprisePartnership = observationQueryContext.LocalEnterprisePartnership,
                MultiAcademyTrust = observationQueryContext.MultiAcademyTrust,
                MayoralCombinedAuthority = observationQueryContext.MayoralCombinedAuthority,
                OpportunityArea = observationQueryContext.OpportunityArea,
                ParliamentaryConstituency = observationQueryContext.ParliamentaryConstituency,
                Region = observationQueryContext.Region,
                RscRegion = observationQueryContext.RscRegion,
                Sponsor = observationQueryContext.Sponsor,
                Ward = observationQueryContext.Ward
            };
        }

        public override string ToString()
        {
            return $"{nameof(SubjectId)}: {SubjectId}, " +
                   $"{nameof(TimePeriod)}: {TimePeriod}, " +
                   $"{nameof(BoundaryLevel)}: {BoundaryLevel}, " +
                   $"{nameof(GeographicLevel)}: {GeographicLevel?.GetEnumValue()}, " +
                   $"{nameof(Indicators)}: [{(Indicators == null ? string.Empty : string.Join(", ", Indicators))}]";
        }
    }
}