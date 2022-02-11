#nullable enable
using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query
{
    public record LocationQuery
    {
        public List<string>? Country { get; set; }
        public List<string>? EnglishDevolvedArea { get; set; }
        public List<string>? Institution { get; set; }
        public List<string>? LocalAuthority { get; set; }
        public List<string>? LocalAuthorityDistrict { get; set; }
        public List<string>? LocalEnterprisePartnership { get; set; }
        public List<string>? MultiAcademyTrust { get; set; }
        public List<string>? MayoralCombinedAuthority { get; set; }
        public List<string>? OpportunityArea { get; set; }
        public List<string>? ParliamentaryConstituency { get; set; }
        public List<string>? Provider { get; set; }
        public List<string>? PlanningArea { get; set; }
        public List<string>? Region { get; set; }
        public List<string>? RscRegion { get; set; }
        public List<string>? School { get; set; }
        public List<string>? Sponsor { get; set; }
        public List<string>? Ward { get; set; }

        public LocationQuery()
        {
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="query"></param>
        public LocationQuery(LocationQuery query)
        {
            Country = query.Country?.ToList();
            EnglishDevolvedArea = query.EnglishDevolvedArea?.ToList();
            Institution = query.Institution?.ToList();
            LocalAuthority = query.LocalAuthority?.ToList();
            LocalAuthorityDistrict = query.LocalAuthorityDistrict?.ToList();
            LocalEnterprisePartnership = query.LocalEnterprisePartnership?.ToList();
            MultiAcademyTrust = query.MultiAcademyTrust?.ToList();
            MayoralCombinedAuthority = query.MayoralCombinedAuthority?.ToList();
            OpportunityArea = query.OpportunityArea?.ToList();
            ParliamentaryConstituency = query.ParliamentaryConstituency?.ToList();
            Provider = query.Provider?.ToList();
            PlanningArea = query.PlanningArea?.ToList();
            Region = query.Region?.ToList();
            RscRegion = query.RscRegion?.ToList();
            School = query.School?.ToList();
            Sponsor = query.Sponsor?.ToList();
            Ward = query.Ward?.ToList();
        }
    }
}
