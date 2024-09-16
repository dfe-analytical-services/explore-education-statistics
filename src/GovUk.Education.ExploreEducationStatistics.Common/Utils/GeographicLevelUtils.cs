#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils;

public static class GeographicLevelUtils
{
    private static readonly Lazy<IReadOnlyDictionary<GeographicLevel, GeographicCsvColumns>> GeographicLevelCsvColumns =
        new(() => new Dictionary<GeographicLevel, GeographicCsvColumns>
        {
            {
                GeographicLevel.Country,
                new GeographicCsvColumns(
                    Codes: new[] { "country_code" },
                    Name: "country_name"
                )
            },
            {
                GeographicLevel.EnglishDevolvedArea,
                new GeographicCsvColumns(
                    Codes: new[] { "english_devolved_area_code" },
                    Name: "english_devolved_area_name"
                )
            },
            {
                GeographicLevel.Institution,
                new GeographicCsvColumns(
                    Codes: new[] { "institution_id" },
                    Name: "institution_name"
                )
            },
            {
                GeographicLevel.LocalAuthority,
                new LocalAuthorityCsvColumns()
            },
            {
                GeographicLevel.LocalAuthorityDistrict,
                new GeographicCsvColumns(
                    Codes: new[] { "lad_code" },
                    Name: "lad_name"
                )
            },
            {
                GeographicLevel.LocalEnterprisePartnership,
                new GeographicCsvColumns(
                    Codes: new[] { "local_enterprise_partnership_code" },
                    Name: "local_enterprise_partnership_name"
                )
            },
            {
                GeographicLevel.LocalSkillsImprovementPlanArea,
                new GeographicCsvColumns(
                    Codes: new[] { "lsip_code" },
                    Name: "lsip_name"
                )
            },
            {
                GeographicLevel.MayoralCombinedAuthority,
                new GeographicCsvColumns(
                    Codes: new[] { "mayoral_combined_authority_code" },
                    Name: "mayoral_combined_authority_name"
                )
            },
            {
                GeographicLevel.MultiAcademyTrust,
                new GeographicCsvColumns(
                    Codes: new[] { "trust_id" },
                    Name: "trust_name"
                )
            },
            {
                GeographicLevel.OpportunityArea,
                new GeographicCsvColumns(
                    Codes: new[] { "opportunity_area_code" },
                    Name: "opportunity_area_name"
                )
            },
            {
                GeographicLevel.ParliamentaryConstituency,
                new GeographicCsvColumns(
                    Codes: new[] { "pcon_code" },
                    Name: "pcon_name"
                )
            },
            {
                GeographicLevel.PlanningArea,
                new GeographicCsvColumns(
                    Codes: new[] { "planning_area_code" },
                    Name: "planning_area_name"
                )
            },
            {
                GeographicLevel.Provider,
                new ProviderCsvColumns()
            },
            {
                GeographicLevel.Region,
                new GeographicCsvColumns(
                    Codes: new[] { "region_code" },
                    Name: "region_name"
                )
            },
            {
                GeographicLevel.RscRegion,
                new GeographicCsvColumns(
                    Codes: Array.Empty<string>(),
                    Name: "rsc_region_lead_name"
                )
            },
            {
                GeographicLevel.School,
                new SchoolCsvColumns()
            },
            {
                GeographicLevel.Sponsor,
                new GeographicCsvColumns(
                    Codes: new[] { "sponsor_id" },
                    Name: "sponsor_name"
                )
            },
            {
                GeographicLevel.Ward,
                new GeographicCsvColumns(
                    Codes: new[] { "ward_code" },
                    Name: "ward_name"
                )
            },
        }
    );

    private static readonly Lazy<IReadOnlyDictionary<string, GeographicLevel>> CsvColumnsToGeographicLevelLazy = new(
        () => GeographicLevelCsvColumns.Value.Aggregate(
            new Dictionary<string, GeographicLevel>(),
            (acc, pair) =>
            {
                var columns = new List<string>(pair.Value.Codes) { pair.Value.Name };

                foreach (var column in columns)
                {
                    acc[column] = pair.Key;
                }

                return acc;
            }
        )
    );

    public static readonly IReadOnlyList<GeographicLevel> Levels = EnumUtil.GetEnums<GeographicLevel>();

    public static readonly IReadOnlyList<string> OrderedCodes =
        EnumUtil.GetEnumValues<GeographicLevel>()
            .NaturalOrder()
            .ToList();

    public static readonly IReadOnlyList<string> OrderedLabels = EnumUtil.GetEnumLabels<GeographicLevel>()
            .NaturalOrder()
            .ToList();

    public static GeographicCsvColumns CsvColumns(this GeographicLevel level) => GeographicLevelCsvColumns.Value[level];

    public static string[] CsvCodeColumns(this GeographicLevel level) => level.CsvColumns().Codes;

    public static string CsvNameColumn(this GeographicLevel level) => level.CsvColumns().Name;

    public static IReadOnlyDictionary<string, GeographicLevel> CsvColumnsToGeographicLevel
        => CsvColumnsToGeographicLevelLazy.Value;

    public record GeographicCsvColumns(string[] Codes, string Name);

    public record SchoolCsvColumns() : GeographicCsvColumns(Codes: new[] { Urn, LaEstab }, Name: "school_name")
    {
        public const string Urn = "school_urn";

        public const string LaEstab = "school_laestab";
    }

    public record ProviderCsvColumns() : GeographicCsvColumns(Codes: new[] { Ukprn }, Name: "provider_name")
    {
        public const string Ukprn = "provider_ukprn";
    }

    public record LocalAuthorityCsvColumns() : GeographicCsvColumns(Codes: new[] { OldCode, NewCode }, Name: "la_name")
    {
        public const string OldCode = "old_la_code";

        public const string NewCode = "new_la_code";
    }
}
