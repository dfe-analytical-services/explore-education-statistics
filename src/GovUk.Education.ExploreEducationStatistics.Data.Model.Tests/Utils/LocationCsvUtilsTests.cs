#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Utils;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils;

public class LocationCsvUtilsTests
{
    private readonly Country _england = new("E92000001", "England");
    private readonly Region _northWest = new("E12000002", "North West");
    private readonly LocalAuthority _nottingham = new("E06000018", "", "Nottingham");

    [Fact]
    public void AllCsvColumns()
    {
        var expectedCols = new List<string>
        {
            "country_code",
            "country_name",
            "region_code",
            "region_name",
            "new_la_code",
            "old_la_code",
            "la_name",
            "rsc_region_lead_name",
            "pcon_code",
            "pcon_name",
            "lad_code",
            "lad_name",
            "local_enterprise_partnership_code",
            "local_enterprise_partnership_name",
            "english_devolved_area_code",
            "english_devolved_area_name",
            "mayoral_combined_authority_code",
            "mayoral_combined_authority_name",
            "opportunity_area_code",
            "opportunity_area_name",
            "ward_code",
            "ward_name",
            "trust_id",
            "trust_name",
            "sponsor_id",
            "sponsor_name",
            "school_urn",
            "school_name",
            "provider_ukprn",
            "provider_name",
            "planning_area_code",
            "planning_area_name",
            "institution_id",
            "institution_name",
        };

        Assert.Equal(expectedCols, LocationCsvUtils.AllCsvColumns().ToList());
    }

    [Fact]
    public void GetCsvValues()
    {
        var location = new Location
        {
            Id = Guid.NewGuid(),
            Country = _england,
            Region = _northWest,
            LocalAuthority = _nottingham,
            GeographicLevel = GeographicLevel.LocalAuthority
        };

        var csvValues = location.GetCsvValues();

        Assert.Equal(7, csvValues.Count);
        Assert.Equal(_england.Code, csvValues["country_code"]);
        Assert.Equal(_england.Name, csvValues["country_name"]);
        Assert.Equal(_northWest.Code, csvValues["region_code"]);
        Assert.Equal(_northWest.Name, csvValues["region_name"]);
        Assert.Equal(_nottingham.Code, csvValues["new_la_code"]);
        Assert.Equal(_nottingham.OldCode, csvValues["old_la_code"]);
        Assert.Equal(_nottingham.Name, csvValues["la_name"]);
    }
}
