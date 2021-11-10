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
        public List<string> Country { get; set; } = new();
        public List<string> EnglishDevolvedArea { get; set; } = new();
        public List<string> Institution { get; set; } = new();
        public List<string> LocalAuthority { get; set; } = new();
        public List<string> LocalAuthorityDistrict { get; set; } = new();
        public List<string> LocalEnterprisePartnership { get; set; } = new();
        public List<string> MultiAcademyTrust { get; set; } = new();
        public List<string> MayoralCombinedAuthority { get; set; } = new();
        public List<string> OpportunityArea { get; set; } = new();
        public List<string> ParliamentaryConstituency { get; set; } = new();
        public List<string> Provider { get; set; } = new();
        public List<string> PlanningArea { get; set; } = new();
        public List<string> Region { get; set; } = new();
        public List<string> RscRegion { get; set; } = new();
        public List<string> School { get; set; } = new();
        public List<string> Sponsor { get; set; } = new();
        public List<string> Ward { get; set; } = new();

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
