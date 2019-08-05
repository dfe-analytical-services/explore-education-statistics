using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query
{
    public class SubjectMetaQueryContext
    {
        public long SubjectId { get; set; }
        public TimePeriodQuery TimePeriod { get; set; }
        public long? BoundaryLevel { get; set; }
        public GeographicLevel? GeographicLevel { get; set; }
        public IEnumerable<string> Indicators { get; set; }
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

        public Expression<Func<Observation, bool>> ObservationPredicate()
        {
            var predicate = PredicateBuilder.True<Observation>()
                .And(observation => observation.SubjectId == SubjectId);

            if (TimePeriod != null)
            {
                // Don't use the observation.GetTimePeriod() extension in the expression here as it can't be translated
                predicate = predicate.And(observation => GetTimePeriodRange(TimePeriod)
                    .Contains(observation.Year + "_" + observation.TimeIdentifier));
            }

            if (GeographicLevel != null)
            {
                predicate = predicate.And(observation => observation.GeographicLevel == GeographicLevel);
            }

            if (ObservationalUnitExists())
            {
                predicate = predicate.And(ObservationalUnitsPredicate());
            }

            return predicate;
        }

        private Expression<Func<Observation, bool>> ObservationalUnitsPredicate()
        {
            var predicate = PredicateBuilder.False<Observation>();

            if (Country != null)
            {
                predicate = predicate.Or(CountryPredicate());
            }

            if (Institution != null)
            {
                predicate = predicate.Or(InstitutionPredicate());
            }

            if (LocalAuthority != null)
            {
                predicate = predicate.Or(LocalAuthorityPredicate());
            }

            if (LocalAuthorityDistrict != null)
            {
                predicate = predicate.Or(LocalAuthorityDistrictPredicate());
            }

            if (LocalEnterprisePartnership != null)
            {
                predicate = predicate.Or(LocalEnterprisePartnershipPredicate());
            }

            if (MayoralCombinedAuthority != null)
            {
                predicate = predicate.Or(MayoralCombinedAuthorityPredicate());
            }

            if (MultiAcademyTrust != null)
            {
                predicate = predicate.Or(MultiAcademyTrustPredicate());
            }

            if (OpportunityArea != null)
            {
                predicate = predicate.Or(OpportunityAreaPredicate());
            }

            if (ParliamentaryConstituency != null)
            {
                predicate = predicate.Or(ParliamentaryConstituencyPredicate());
            }

            if (Region != null)
            {
                predicate = predicate.Or(RegionPredicate());
            }

            if (RscRegion != null)
            {
                predicate = predicate.Or(RscRegionPredicate());
            }

            if (Sponsor != null)
            {
                predicate = predicate.Or(SponsorPredicate());
            }

            if (Ward != null)
            {
                predicate = predicate.Or(WardPredicate());
            }

            return predicate;
        }

        private bool ObservationalUnitExists()
        {
            return !(Country == null &&
                     Institution == null &&
                     LocalAuthority == null &&
                     LocalAuthorityDistrict == null &&
                     LocalEnterprisePartnership == null &&
                     MultiAcademyTrust == null &&
                     MayoralCombinedAuthority == null &&
                     OpportunityArea == null &&
                     ParliamentaryConstituency == null &&
                     Region == null &&
                     RscRegion == null &&
                     Sponsor == null &&
                     Ward == null);
        }

        private Expression<Func<Observation, bool>> CountryPredicate()
        {
            return ObservationalUnitPredicate(Model.GeographicLevel.Country,
                observation => Country.Contains(observation.Location.Country.Code));
        }

        private Expression<Func<Observation, bool>> InstitutionPredicate()
        {
            return ObservationalUnitPredicate(Model.GeographicLevel.Institution,
                observation => Institution.Contains(observation.Location.Institution.Code));
        }

        private Expression<Func<Observation, bool>> LocalAuthorityPredicate()
        {
            return ObservationalUnitPredicate(Model.GeographicLevel.Local_Authority,
                observation => LocalAuthority.Contains(observation.Location.LocalAuthority.Code));
        }

        private Expression<Func<Observation, bool>> LocalAuthorityDistrictPredicate()
        {
            return ObservationalUnitPredicate(Model.GeographicLevel.Local_Authority_District,
                observation => LocalAuthorityDistrict.Contains(observation.Location.LocalAuthorityDistrict.Code));
        }

        private Expression<Func<Observation, bool>> LocalEnterprisePartnershipPredicate()
        {
            return ObservationalUnitPredicate(Model.GeographicLevel.Local_Enterprise_Partnership,
                observation =>
                    LocalEnterprisePartnership.Contains(observation.Location.LocalEnterprisePartnership.Code));
        }

        private Expression<Func<Observation, bool>> MayoralCombinedAuthorityPredicate()
        {
            return ObservationalUnitPredicate(Model.GeographicLevel.Mayoral_Combined_Authority,
                observation => MayoralCombinedAuthority.Contains(observation.Location.MayoralCombinedAuthority.Code));
        }

        private Expression<Func<Observation, bool>> MultiAcademyTrustPredicate()
        {
            return ObservationalUnitPredicate(Model.GeographicLevel.Multi_Academy_Trust,
                observation => MultiAcademyTrust.Contains(observation.Location.MultiAcademyTrust.Code));
        }

        private Expression<Func<Observation, bool>> OpportunityAreaPredicate()
        {
            return ObservationalUnitPredicate(Model.GeographicLevel.Opportunity_Area,
                observation => OpportunityArea.Contains(observation.Location.OpportunityArea.Code));
        }

        private Expression<Func<Observation, bool>> ParliamentaryConstituencyPredicate()
        {
            return ObservationalUnitPredicate(Model.GeographicLevel.Parliamentary_Constituency,
                observation => ParliamentaryConstituency.Contains(observation.Location.ParliamentaryConstituency.Code));
        }

        private Expression<Func<Observation, bool>> RegionPredicate()
        {
            return ObservationalUnitPredicate(Model.GeographicLevel.Region,
                observation => Region.Contains(observation.Location.Region.Code));
        }

        private Expression<Func<Observation, bool>> RscRegionPredicate()
        {
            return ObservationalUnitPredicate(Model.GeographicLevel.RSC_Region,
                observation => RscRegion.Contains(observation.Location.RscRegion.Code));
        }

        private Expression<Func<Observation, bool>> SponsorPredicate()
        {
            return ObservationalUnitPredicate(Model.GeographicLevel.Sponsor,
                observation => Sponsor.Contains(observation.Location.Sponsor.Code));
        }

        private Expression<Func<Observation, bool>> WardPredicate()
        {
            return ObservationalUnitPredicate(Model.GeographicLevel.Ward,
                observation => Ward.Contains(observation.Location.Ward.Code));
        }

        private Expression<Func<Observation, bool>> ObservationalUnitPredicate(
            GeographicLevel geographicLevel, Expression<Func<Observation, bool>> expression)
        {
            var predicate = PredicateBuilder.True<Observation>()
                .And(expression);

            if (GeographicLevel == null)
            {
                predicate = predicate.And(observation => observation.GeographicLevel.Equals(geographicLevel));
            }

            return predicate;
        }

        private static IEnumerable<string> GetTimePeriodRange(TimePeriodQuery timePeriod)
        {
            if (timePeriod.StartCode.IsNumberOfTerms() || timePeriod.EndCode.IsNumberOfTerms())
            {
                return TimePeriodUtil.RangeForNumberOfTerms(timePeriod.StartYear, timePeriod.EndYear)
                    .Select(tuple => tuple.GetTimePeriod());
            }

            return TimePeriodUtil.Range(timePeriod).Select(tuple => tuple.GetTimePeriod());
        }
    }
}