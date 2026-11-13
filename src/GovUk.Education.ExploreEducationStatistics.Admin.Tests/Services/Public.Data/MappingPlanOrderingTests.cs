#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Public.Data;

public abstract class MappingPlanOrderingTests
{
    public class LocationMappingPlanTests
    {
        [Fact]
        public void OrdersLevelsMappingsAndCandidatesByLabel()
        {
            var plan = new LocationMappingPlan
            {
                Levels = new Dictionary<GeographicLevel, LocationLevelMappings>
                {
                    [GeographicLevel.School] = new(),
                    [GeographicLevel.Country] = new(),
                    [GeographicLevel.LocalAuthority] = new()
                    {
                        Mappings = new Dictionary<string, LocationOptionMapping>
                        {
                            ["York :: E2"] = LocationMapping("York"),
                            ["barnsley :: E3"] = LocationMapping("barnsley"),
                            ["Barnsley :: E1"] = LocationMapping("Barnsley"),
                            ["Leeds :: E4"] = LocationMapping("Leeds"),
                        },
                        Candidates = new Dictionary<string, MappableLocationOption>
                        {
                            ["Wakefield :: E5"] = new() { Label = "Wakefield" },
                            ["Area 10 :: E6"] = new() { Label = "Area 10" },
                            ["Area 2 :: E7"] = new() { Label = "Area 2" },
                        },
                    },
                },
            };

            var ordered = plan.OrderByLabel();

            // Ordered by level label: "Local authority", "National", "School"
            Assert.Equal(
                [GeographicLevel.LocalAuthority, GeographicLevel.Country, GeographicLevel.School],
                ordered.Levels.Keys
            );

            var localAuthority = ordered.Levels[GeographicLevel.LocalAuthority];

            // Same label (case-insensitive) is tie-broken by key
            Assert.Equal(
                ["Barnsley :: E1", "barnsley :: E3", "Leeds :: E4", "York :: E2"],
                localAuthority.Mappings.Keys
            );
            Assert.Equal(["Area 2 :: E7", "Area 10 :: E6", "Wakefield :: E5"], localAuthority.Candidates.Keys);
        }

        [Fact]
        public void DoesNotMutateOriginalPlan()
        {
            var levelMappings = new LocationLevelMappings
            {
                Mappings = new Dictionary<string, LocationOptionMapping>
                {
                    ["b"] = LocationMapping("B"),
                    ["a"] = LocationMapping("A"),
                },
            };

            var plan = new LocationMappingPlan
            {
                Levels = new Dictionary<GeographicLevel, LocationLevelMappings>
                {
                    [GeographicLevel.LocalAuthority] = levelMappings,
                },
            };

            plan.OrderByLabel();

            Assert.Same(levelMappings, plan.Levels[GeographicLevel.LocalAuthority]);
            Assert.Equal(["b", "a"], levelMappings.Mappings.Keys);
        }

        private static LocationOptionMapping LocationMapping(string label) =>
            new()
            {
                Source = new MappableLocationOption { Label = label },
                PublicId = label,
            };
    }

    public class FilterMappingPlanTests
    {
        [Fact]
        public void OrdersFiltersOptionsAndCandidatesByLabel()
        {
            var plan = new FilterMappingPlan
            {
                Mappings = new Dictionary<string, FilterMapping>
                {
                    ["sex"] = new()
                    {
                        Source = new MappableFilter { Label = "Sex" },
                        PublicId = "sex",
                        OptionMappings = new Dictionary<string, FilterOptionMapping>
                        {
                            ["Male"] = FilterOptionMapping("Male"),
                            ["All"] = FilterOptionMapping("All"),
                            ["Female"] = FilterOptionMapping("Female"),
                        },
                    },
                    ["exam_cohort"] = new()
                    {
                        Source = new MappableFilter { Label = "Exam cohort" },
                        PublicId = "exam_cohort",
                    },
                    ["ethnicity_minor"] = new()
                    {
                        Source = new MappableFilter { Label = "Ethnicity" },
                        PublicId = "ethnicity_minor",
                    },
                },
                Candidates = new Dictionary<string, FilterMappingCandidate>
                {
                    ["year_group"] = new()
                    {
                        Label = "Year group",
                        Options = new Dictionary<string, MappableFilterOption>
                        {
                            ["Year 10"] = new() { Label = "Year 10" },
                            ["Year 2"] = new() { Label = "Year 2" },
                            ["Total"] = new() { Label = "Total" },
                        },
                    },
                    ["disadvantage_status"] = new() { Label = "Disadvantage status" },
                },
            };

            var ordered = plan.OrderByLabel();

            Assert.Equal(["ethnicity_minor", "exam_cohort", "sex"], ordered.Mappings.Keys);
            Assert.Equal(["All", "Female", "Male"], ordered.Mappings["sex"].OptionMappings.Keys);

            Assert.Equal(["disadvantage_status", "year_group"], ordered.Candidates.Keys);
            Assert.Equal(["Total", "Year 2", "Year 10"], ordered.Candidates["year_group"].Options.Keys);

            // Original plan is left untouched
            Assert.Equal(["Male", "All", "Female"], plan.Mappings["sex"].OptionMappings.Keys);
        }

        private static FilterOptionMapping FilterOptionMapping(string label) =>
            new()
            {
                Source = new MappableFilterOption { Label = label },
                PublicId = label,
            };
    }

    public class IndicatorMappingPlanTests
    {
        [Fact]
        public void OrdersMappingsAndCandidatesByLabel()
        {
            var plan = new IndicatorMappingPlan
            {
                Mappings = new Dictionary<string, IndicatorMapping>
                {
                    ["pupil_count"] = IndicatorMapping("Pupil count"),
                    ["attainment_8"] = IndicatorMapping("Attainment 8"),
                    ["entries"] = IndicatorMapping("Entries"),
                },
                Candidates = new Dictionary<string, MappableIndicator>
                {
                    ["z_score"] = new() { Label = "Z score" },
                    ["a_score"] = new() { Label = "A score" },
                },
            };

            var ordered = plan.OrderByLabel();

            Assert.Equal(["attainment_8", "entries", "pupil_count"], ordered.Mappings.Keys);
            Assert.Equal(["a_score", "z_score"], ordered.Candidates.Keys);
        }

        private static IndicatorMapping IndicatorMapping(string label) =>
            new()
            {
                Source = new MappableIndicator { Label = label },
                PublicId = label,
            };
    }
}
