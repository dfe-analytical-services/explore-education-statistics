#nullable enable
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query
{
    public record LocationQuery
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public GeographicLevel? GeographicLevel { get; set; }
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

        public int CountItems()
        {
            return GetType()
                .GetProperties()
                .Where(property => property.PropertyType == typeof(List<string>))
                .Select(property => property.GetValue(this))
                .Cast<List<string>>()
                .Sum(collection => collection?.Count ?? 0);
        }
    }
}
