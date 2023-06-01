#nullable enable

using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels
{
    // TODO EES-3755 Remove after Permalink snapshot work is complete
    public record LocationViewModel
    {
        public CodeNameViewModel? Country { get; init; }
        public CodeNameViewModel? EnglishDevolvedArea { get; init; }
        public CodeNameViewModel? Institution { get; init; }
        public CodeNameViewModel? LocalAuthority { get; init; }
        public CodeNameViewModel? LocalAuthorityDistrict { get; init; }
        public CodeNameViewModel? LocalEnterprisePartnership { get; init; }
        public CodeNameViewModel? MayoralCombinedAuthority { get; init; }
        public CodeNameViewModel? MultiAcademyTrust { get; init; }
        public CodeNameViewModel? OpportunityArea { get; init; }
        public CodeNameViewModel? ParliamentaryConstituency { get; init; }
        public CodeNameViewModel? PlanningArea { get; init; }
        public CodeNameViewModel? Provider { get; init; }
        public CodeNameViewModel? Region { get; init; }
        public CodeNameViewModel? RscRegion { get; init; }
        public CodeNameViewModel? School { get; init; }
        public CodeNameViewModel? Sponsor { get; init; }
        public CodeNameViewModel? Ward { get; init; }

        private Location AsLocation()
        {
            return new Location
            {
                Country = new Country(
                    Country?.Code,
                    Country?.Name),
                EnglishDevolvedArea = new EnglishDevolvedArea(
                    EnglishDevolvedArea?.Code,
                    EnglishDevolvedArea?.Name),
                LocalAuthority = new LocalAuthority(
                    LocalAuthority?.Code,
                    OldCode: null,
                    LocalAuthority?.Name),
                LocalAuthorityDistrict = new LocalAuthorityDistrict(
                    LocalAuthorityDistrict?.Code,
                    LocalAuthorityDistrict?.Name),
                LocalEnterprisePartnership = new LocalEnterprisePartnership(
                    LocalEnterprisePartnership?.Code,
                    LocalEnterprisePartnership?.Name),
                Institution = new Institution(
                    Institution?.Code,
                    Institution?.Name),
                MayoralCombinedAuthority = new MayoralCombinedAuthority(
                    MayoralCombinedAuthority?.Code,
                    MayoralCombinedAuthority?.Name),
                MultiAcademyTrust = new MultiAcademyTrust(
                    MultiAcademyTrust?.Code,
                    MultiAcademyTrust?.Name),
                OpportunityArea = new OpportunityArea(
                    OpportunityArea?.Code,
                    OpportunityArea?.Name),
                ParliamentaryConstituency = new ParliamentaryConstituency(
                    ParliamentaryConstituency?.Code,
                    ParliamentaryConstituency?.Name),
                PlanningArea = new PlanningArea(
                    PlanningArea?.Code,
                    PlanningArea?.Name),
                Provider = new Provider(
                    Provider?.Code,
                    Provider?.Name),
                Region = new Region(
                    Region?.Code,
                    Region?.Name),
                RscRegion = new RscRegion(
                    RscRegion?.Code),
                School = new School(
                    School?.Code,
                    School?.Name),
                Sponsor = new Sponsor(
                    Sponsor?.Code,
                    Sponsor?.Name),
                Ward = new Ward(
                    Ward?.Code,
                    Ward?.Name)
            };
        }

        public Dictionary<string, string> GetCsvValues()
        {
            var location = AsLocation();
            return location.GetAttributes()
                .SelectMany(attribute => attribute.CsvValues)
                .ToDictionary(col => col.Key, col => col.Value);
        }
    }
}
