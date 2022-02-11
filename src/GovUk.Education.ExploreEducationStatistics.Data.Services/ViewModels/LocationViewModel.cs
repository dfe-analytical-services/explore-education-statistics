#nullable enable

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels
{
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
    }
}
