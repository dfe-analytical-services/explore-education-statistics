#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
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
                    Code: "country_code",
                    Name: "country_name"
                )
            },
            {
                GeographicLevel.EnglishDevolvedArea,
                new GeographicCsvColumns(
                    Code: "english_devolved_area_code",
                    Name: "english_devolved_area_name"
                )
            },
            {
                GeographicLevel.Institution,
                new GeographicCsvColumns(
                    Code: "institution_id",
                    Name: "institution_name"
                )
            },
            {
                GeographicLevel.LocalAuthority,
                new GeographicCsvColumns(
                    Code: "new_la_code",
                    Name: "la_name",
                    Other: new[] { "old_la_code" }
                )
            },
            {
                GeographicLevel.LocalAuthorityDistrict,
                new GeographicCsvColumns(
                    Code: "lad_code",
                    Name: "lad_name"
                )
            },
            {
                GeographicLevel.LocalEnterprisePartnership,
                new GeographicCsvColumns(
                    Code: "local_enterprise_partnership_code",
                    Name: "local_enterprise_partnership_name"
                )
            },
            {
                GeographicLevel.LocalSkillsImprovementPlanArea,
                new GeographicCsvColumns(
                    Code: "lsip_code",
                    Name: "lsip_name"
                )
            },
            {
                GeographicLevel.MayoralCombinedAuthority,
                new GeographicCsvColumns(
                    Code: "mayoral_combined_authority_code",
                    Name: "mayoral_combined_authority_name"
                )
            },
            {
                GeographicLevel.MultiAcademyTrust,
                new GeographicCsvColumns(
                    Code: "trust_id",
                    Name: "trust_name"
                )
            },
            {
                GeographicLevel.OpportunityArea,
                new GeographicCsvColumns(
                    Code: "opportunity_area_code",
                    Name: "opportunity_area_name"
                )
            },
            {
                GeographicLevel.ParliamentaryConstituency,
                new GeographicCsvColumns(
                    Code: "pcon_code",
                    Name: "pcon_name"
                )
            },
            {
                GeographicLevel.PlanningArea,
                new GeographicCsvColumns(
                    Code: "planning_area_code",
                    Name: "planning_area_name"
                )
            },
            {
                GeographicLevel.Provider,
                new GeographicCsvColumns(
                    Code: "provider_ukprn",
                    Name: "provider_name"
                )
            },
            {
                GeographicLevel.Region,
                new GeographicCsvColumns(
                    Code: "region_code",
                    Name: "region_name"
                )
            },
            {
                GeographicLevel.RscRegion,
                new GeographicCsvColumns(
                    Code: "rsc_region_code",
                    Name: "rsc_region_name"
                )
            },
            {
                GeographicLevel.School,
                new GeographicCsvColumns(
                    Code: "school_urn",
                    Name: "school_name",
                    Other: new[] { "school_laestab" }
                )
            },
            {
                GeographicLevel.Sponsor,
                new GeographicCsvColumns(
                    Code: "sponsor_id",
                    Name: "sponsor_name"
                )
            },
            {
                GeographicLevel.Ward,
                new GeographicCsvColumns(
                    Code: "ward_code",
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
                var columns = new List<string> { pair.Value.Code, pair.Value.Name };

                columns.AddRange(pair.Value.Other);

                foreach (var column in columns)
                {
                    acc[column] = pair.Key;
                }

                return acc;
            }
        )
    );

    public static GeographicCsvColumns CsvColumns(this GeographicLevel level) => GeographicLevelCsvColumns.Value[level];

    public static string CsvCodeColumn(this GeographicLevel level) => level.CsvColumns().Code;

    public static string CsvNameColumn(this GeographicLevel level) => level.CsvColumns().Name;

    public static IReadOnlyList<string> CsvOtherColumns(this GeographicLevel level) => level.CsvColumns().Other;

    public static IReadOnlyDictionary<string, GeographicLevel> CsvColumnsToGeographicLevel
        => CsvColumnsToGeographicLevelLazy.Value;

    public record GeographicCsvColumns(string Code, string Name, IReadOnlyList<string>? Other = null)
    {
        public readonly IReadOnlyList<string> Other = Other ?? new List<string>();
    }
}
