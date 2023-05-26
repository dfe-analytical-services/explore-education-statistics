#nullable enable
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Extensions;

// TODO EES-3755 Remove after Permalink snapshot migration work is complete
public static class LocationTestExtensions
{
    public static LocationViewModel AsLocationViewModel(this Location location)
    {
        return new LocationViewModel
        {
            Country = location.Country?.AsCodeNameViewModel(),
            EnglishDevolvedArea = location.EnglishDevolvedArea?.AsCodeNameViewModel(),
            Institution = location.Institution?.AsCodeNameViewModel(),
            LocalAuthority = location.LocalAuthority?.AsCodeNameViewModel(),
            LocalAuthorityDistrict = location.LocalAuthorityDistrict?.AsCodeNameViewModel(),
            LocalEnterprisePartnership = location.LocalEnterprisePartnership?.AsCodeNameViewModel(),
            MayoralCombinedAuthority = location.MayoralCombinedAuthority?.AsCodeNameViewModel(),
            MultiAcademyTrust = location.MultiAcademyTrust?.AsCodeNameViewModel(),
            OpportunityArea = location.OpportunityArea?.AsCodeNameViewModel(),
            ParliamentaryConstituency = location.ParliamentaryConstituency?.AsCodeNameViewModel(),
            PlanningArea = location.PlanningArea?.AsCodeNameViewModel(),
            Provider = location.Provider?.AsCodeNameViewModel(),
            Region = location.Region?.AsCodeNameViewModel(),
            RscRegion = location.RscRegion?.AsCodeNameViewModel(),
            School = location.School?.AsCodeNameViewModel(),
            Sponsor = location.Sponsor?.AsCodeNameViewModel(),
            Ward = location.Ward?.AsCodeNameViewModel()
        };
    }

    private static CodeNameViewModel AsCodeNameViewModel(this LocationAttribute locationAttribute)
    {
        return new CodeNameViewModel
        {
            Code = locationAttribute.GetCodeOrFallback(),
            Name = locationAttribute.Name ?? string.Empty
        };
    }
}
